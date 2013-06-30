using System;
using System.Linq;
using Microsoft.Deployment.WindowsInstaller;

namespace IronFoundry.Warden.Service.InstallerCA
{
    using System.IO;
    using System.Threading;
    using System.Windows.Forms;
    using Win32;

    // Ref: Standard Actions - http://msdn.microsoft.com/en-us/library/aa372023(VS.85).aspx
    public class CustomActions
    {
        private const string CredentialsValidPropertyName = "CREDENTIALS_VALID";
        private const string ServiceCredentialsUserPropertyName = "SERVICE_CREDENTIALS_USER";
        private const string ServiceCredentialsPasswordPropertyName = "SERVICE_CREDENTIALS_PASSWORD";

        private const string ContainerPathPropertyName = "CONTAINER_BASE_PATH";
        private const string ContainerPathValidPropertyName = "CONTAINER_PATH_VALID";

        private const string TcpPortPropertyName = "TCP_PORT";
        private const string TcpPortValidPropertyName = "TCP_PORT_VALID";

        [CustomAction]
        public static ActionResult ValidateCredentials(Session session)
        {
            session.Log("Validating service credentials...");

            var userName = session[ServiceCredentialsUserPropertyName];
            var password = session[ServiceCredentialsPasswordPropertyName];

            if (!userName.Contains('\\'))
            {
                userName = Environment.MachineName + "\\" + userName;
                session[ServiceCredentialsUserPropertyName] = userName;
            }

            var valid = new LogonUser(new SessionLogger(session)).ValidateCredentials(userName, password);
            session[CredentialsValidPropertyName] = valid ? "1" : "0";

            return ActionResult.Success;
        }

        [CustomAction]
        public static ActionResult ValidateSettings(Session session)
        {
            session.Log("Begin validating warden settings...");

            ushort tcpPort;
            UInt16.TryParse(session[TcpPortPropertyName], out tcpPort);
            var portIsValid = (tcpPort > 1024 && tcpPort <= UInt16.MaxValue - 1);
            session[TcpPortValidPropertyName] = portIsValid ? "1" : "0";

            var containerPathValid = true;

            try
            {
                var containerPath = session[ContainerPathPropertyName];
                session.Log("Container Path: {0}", containerPath);

                var invalidPathChars = Path.GetInvalidPathChars();
                if (String.IsNullOrWhiteSpace(containerPath) || containerPath.Any(invalidPathChars.Contains))
                {
                    containerPathValid = false;
                }
                else
                {
                    Path.GetDirectoryName(session[ContainerPathPropertyName]);
                }
            }
            catch
            {
                containerPathValid = false;
            }

            session[ContainerPathValidPropertyName] = containerPathValid ? "1" : "0";
            return ActionResult.Success;
        }

        [CustomAction]
        public static ActionResult OpenFolderBrowser(Session session)
        {
            session.Log("Start Action -> OpenFolderBrowser...");
            var task = new Thread(() => ShowFolderBrowser(session, @"Select the folder that will be used as the base for all user containers that are created. e.g. C:\IronFoundry\Warden\Containers"));
            task.SetApartmentState(ApartmentState.STA);
            task.Start();
            task.Join();

            return ActionResult.Success;
        }

        private static void ShowFolderBrowser(Session session, string description)
        {
            var currentPath = session[ContainerPathPropertyName];

            using (var dialog = new FolderBrowserDialog())
            {
                dialog.ShowNewFolderButton = true;
                dialog.Description = description;

                if (!String.IsNullOrWhiteSpace(currentPath) && Directory.Exists(currentPath))
                {
                    dialog.SelectedPath = currentPath;
                }
                else
                {
                    dialog.RootFolder = Environment.SpecialFolder.MyComputer;
                }

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    session[ContainerPathPropertyName] = dialog.SelectedPath;
                }
            }
        }
    }
}
