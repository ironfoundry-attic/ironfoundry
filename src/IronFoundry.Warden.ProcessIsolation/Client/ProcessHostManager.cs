namespace IronFoundry.Warden.ProcessIsolation.Client
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Security.AccessControl;
    using System.ServiceProcess;
    using System.Threading;
    using NLog;

    public class ProcessHostManager : IDisposable
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private readonly string processHostDirectory;
        private readonly string serviceName;
        private readonly string exeFileName;
        private readonly string containerDirectory;
        private readonly string processHostTargetDirectory;
        private readonly NetworkCredential credential;

        public ProcessHostManager(string processHostDirectory, string containerDirectory, string exeFileName, string serviceName, NetworkCredential credential) 
            : this(serviceName, containerDirectory)
        {
            if (!Directory.Exists(processHostDirectory))
            {
                throw new ArgumentException("Directory not found", "processHostDirectory");
            }
            this.processHostDirectory = processHostDirectory;

            if (string.IsNullOrWhiteSpace(exeFileName))
            {
                throw new ArgumentException("You must specify an executable file name.", "exeFileName");
            }
            this.exeFileName = exeFileName;

            this.credential = credential;
        }

        private ProcessHostManager(string serviceName, string containerDirectory)
        {
            if (string.IsNullOrWhiteSpace(serviceName))
            {
                throw new ArgumentException("Service name must be specified", "serviceName");
            }
            this.serviceName = serviceName;

            if (string.IsNullOrWhiteSpace(containerDirectory))
            {
                throw new ArgumentException("Container directory is not valid");
            }
            this.containerDirectory = containerDirectory;
            this.processHostTargetDirectory = Path.Combine(containerDirectory, "processhost");
        }

        private void CopyExecutableToHostTargetDirectory()
        {
            if (Directory.Exists(processHostTargetDirectory))
            {
                this.Try(CleanServiceDirectory);
            }

            if (credential != null)
            {
                var acl = new FileSystemAccessRule(credential.UserName,
                                                   FileSystemRights.FullControl,
                                                   InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                                                   PropagationFlags.None,
                                                   AccessControlType.Allow);

                DirectorySecurity tempDirectorySecurity = new DirectoryInfo(containerDirectory).GetAccessControl();
                tempDirectorySecurity.AddAccessRule(acl);

                Directory.CreateDirectory(processHostTargetDirectory, tempDirectorySecurity);
            }
            else
            {
                Directory.CreateDirectory(processHostTargetDirectory);
            }

            log.Trace("Copying service files to: {0}", processHostTargetDirectory);
            foreach (var file in Directory.GetFiles(processHostDirectory))
            {
                File.Copy(file, file.Replace(processHostDirectory, processHostTargetDirectory));
            }
        }

        public static void Cleanup(string serviceName, string containerDirectory)
        {
            using (new ProcessHostManager(serviceName, containerDirectory))
            {
                log.Info("Cleaning up process host {0} installed in {1}.", serviceName, containerDirectory);
            }
        }

        /// <summary>
        /// Ensures that the specified service is installed and in a running state.
        /// </summary>
        public void RunService()
        {
            try
            {
                /*
                 * NB: using sc.exe to install/uninstall and query service rather than ServiceController
                 * because using ServiceController causes issues.
                 */
                if (ServiceInstalled())
                {
                    log.Trace("Service '{0}' already installed, skipping.", serviceName);
                }
                else
                {
                    CopyExecutableToHostTargetDirectory();
                    InstallService();
                }

                if (!ServiceRunning())
                {
                    StartService();
                }
            }
            catch (Exception ex)
            {
                log.ErrorException("Unable to run service", ex);
                throw;
            }
        }

        private void StartService()
        {
            log.Trace("Starting service {0}", serviceName);
            try
            {
                using (var sc = new ServiceController(serviceName))
                {
                    sc.Start();
                    sc.WaitForStatus(ServiceControllerStatus.Running);
                }
            }
            catch (Exception ex)
            {
                log.ErrorException("Unable to stop service", ex);
            }
        }

        private void StopService()
        {
            log.Trace("Stopping service {0}", serviceName);

            try
            {
                using (var sc = new ServiceController(serviceName))
                {
                    sc.Stop();
                    sc.WaitForStatus(ServiceControllerStatus.Stopped);
                }
            }
            catch (Exception ex)
            {
                log.ErrorException("Unable to stop service", ex);
            }
        }

        private void InstallService()
        {
            log.Trace("Installing windows service {0}", serviceName);
            var binPath = Path.Combine(processHostTargetDirectory, exeFileName);

            if (credential != null)
            {
                RunServiceCommand(String.Format(@"create {0} binPath= ""{1}"" start= auto obj= .\\{2} password= {3}",
                                                serviceName, binPath, credential.UserName, credential.Password));
            }
            else
            {
                RunServiceCommand(String.Format(@"create {0} binPath= ""{1}"" start= auto obj= LocalSystem", serviceName, binPath));
            }
        }

        private void UninstallService()
        {
            if (!ServiceInstalled())
            {
                log.Trace("Service '{0}' doesn't exist, skipping uninstall.", serviceName);
                return;
            }

            StopService();
            log.Trace("Removing service {0}", serviceName);
            var cmdArgs = String.Format("delete {0}", serviceName);
            this.Try(RunServiceCommand, cmdArgs, ex => log.ErrorException(String.Format("Error running: sc.exe {0}", cmdArgs), ex));
        }

        private bool ServiceInstalled()
        {
            // this sorta works except when a svc is marked for delete (which means it wasn't cleaned up proper)
            return this.Try(() => ExecServiceCommand(String.Format("queryex {0}", serviceName), false).ExitCode != 1060, () => false);
        }

        private bool ServiceRunning()
        {
            return this.Try(() => { using (var sc = new ServiceController(serviceName)) return sc.Status == ServiceControllerStatus.Running; }, () => false);
        }

        private void RunServiceCommand(string cmdArgs)
        {
            var result = ExecServiceCommand(cmdArgs, true);
            if (result.TimedOut || result.ExitCode != 0)
            {
                log.Error("Error or timeout running: sc.exe {0}", cmdArgs);
                throw new Exception("Error running service control command");
            }
        }

        private ExecutableResult ExecServiceCommand(string cmdArgs, bool showOutput)
        {
            using (var p = new Process())
            {
                p.StartInfo.FileName = "sc.exe";
                p.StartInfo.Arguments = cmdArgs;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                var result = p.StartWithRedirectedOutputIO(TimeSpan.FromMinutes(2), CancellationToken.None);

                if (showOutput && result.ExitCode != 0)
                {
                    log.Debug("Exit Code: {0}", result.ExitCode);

                    if (!String.IsNullOrWhiteSpace(result.StandardOut))
                    {
                        log.Debug("{0}{1}", Environment.NewLine, result.StandardOut);
                    }
                    if (!String.IsNullOrWhiteSpace(result.StandardError))
                    {
                        log.Debug("{0}{1}", Environment.NewLine, result.StandardError);
                    }
                    if (result.TimedOut)
                    {
                        log.Debug("Timed Out: {0}", result.TimedOut);
                    }
                }

                return result;
            }
        }

        private void CleanServiceDirectory()
        {
            if (Directory.Exists(processHostTargetDirectory))
            {
                log.Trace("Cleaning up service directory {0}", processHostTargetDirectory);
                Directory.Delete(processHostTargetDirectory, true);
            }
        }

        public void Dispose()
        {
            this.Try(UninstallService);
            this.Try(CleanServiceDirectory, ex => log.ErrorException(String.Format("Unable to cleanup service directory {0}", processHostTargetDirectory), ex));
        }
    }
}
