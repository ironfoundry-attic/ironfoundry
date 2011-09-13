using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualStudio.PlatformUI;
using EnvDTE;
using VsWebSite;
using System.IO;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using System.Diagnostics;
using Microsoft.Build.Utilities;
using CloudFoundry.Net.Vmc;

namespace CloudFoundry.Net.VsExtension
{
    /// <summary>
    /// Interaction logic for ManageWindow.xaml
    /// </summary>
    public partial class ManageWindow : DialogWindow
    {
        private VSWebSite site;
        private Project project;
        private string defaultProjectLocation;
        private string cloudFoundryStagingPath;
        private string projectName;
        private string precompiledSitePath;
        private BackgroundWorker worker;
        private ProgressDialog progressDialog;
        private Globals globals;
        private string frameworkPath;
        private string projectDirectory;

        public ManageWindow(Project project)
        {
            this.project = project;
            this.site = project.Object as VSWebSite;

            InitializeComponent();
            this.Closing += new CancelEventHandler(ManageWindow_Closing);

            var prop = this.project.DTE.Properties["Environment", "ProjectsAndSolution"];
            defaultProjectLocation = prop.Item("ProjectsLocation").Value as string; 
            cloudFoundryStagingPath = System.IO.Path.Combine(defaultProjectLocation, "CloudFoundry_Staging"); ;

            if (!Directory.Exists(cloudFoundryStagingPath))
                Directory.CreateDirectory(cloudFoundryStagingPath);

            projectDirectory = System.IO.Path.GetDirectoryName(project.FullName);
            projectName = System.IO.Path.GetFileNameWithoutExtension(projectDirectory).ToLower();
            if (site == null)
            {
                projectName = project.Name;
            }

            precompiledSitePath = System.IO.Path.Combine(cloudFoundryStagingPath, projectName);
            ApplicationNameTextBox.Text = projectName;
            ApplicationUrlTextBox.Text = projectName + ".vcap.me";
            ApplicationUrlTextBox.IsEnabled = false;

            SetGlobalVariables();

            SetFrameworkPath(project);
           
        }

        private void SetFrameworkPath(Project project)
        {
            var targetPlatform = project.ConfigurationManager.ActiveConfiguration.Properties.Item("PlatformTarget").Value as string;
            int targetFramework = Convert.ToInt32(project.Properties.Item("TargetFramework").Value);

            TargetDotNetFrameworkVersion version = TargetDotNetFrameworkVersion.Version40;
            switch (targetFramework)
            {
                case Fx40:
                    version = TargetDotNetFrameworkVersion.Version40;
                    break;
                case Fx35:
                    version = TargetDotNetFrameworkVersion.Version35;
                    break;
                case Fx30:
                    version = TargetDotNetFrameworkVersion.Version30;
                    break;
                case Fx20:
                    version = TargetDotNetFrameworkVersion.Version20;
                    break;
            }

            DotNetFrameworkArchitecture arch = DotNetFrameworkArchitecture.Bitness32;
            if (targetPlatform == "AnyCpu")
                arch = DotNetFrameworkArchitecture.Current;
            if (targetPlatform == "x64")
                arch = DotNetFrameworkArchitecture.Bitness64;

            frameworkPath = ToolLocationHelper.GetPathToDotNetFramework(version, arch);
        }

        private const int Fx40 = 262144;
        private const int Fx35 = 196613;
        private const int Fx30 = 196608;
        private const int Fx20 = 131072;

        private void SetGlobalVariables()
        {
            globals = this.project.DTE.Solution.Globals;

            string variablename = "CFUsername";
            if (globals.get_VariableExists(variablename))
            {
                UsernameTextBox.Text = globals[variablename] as string;
            }
            else
            {
                globals[variablename] = string.Empty;
                globals.set_VariablePersists(variablename, true);
            }

            variablename = "CFPassword";
            if (globals.get_VariableExists(variablename))
            {
                PasswordTextBox.Password = globals[variablename] as string;
            }
            else
            {
                globals[variablename] = string.Empty;
                globals.set_VariablePersists(variablename, true);
            }
        }

        private void ManageWindow_Closing(object sender, CancelEventArgs e)
        {
            string variablename = "CFUsername";
            globals[variablename] = UsernameTextBox.Text;
            globals.set_VariablePersists(variablename, true);
            variablename = "CFPassword";
            globals[variablename] = PasswordTextBox.Password;
            globals.set_VariablePersists(variablename, true);
        }

        private void PushApplicationButton_Click(object sender, RoutedEventArgs e)
        {
            PushApplicationButton.IsEnabled = false;
            CancelButton.IsEnabled = false;

            progressDialog = new ProgressDialog("Push Application...", "Saving project...");
            progressDialog.Cancel += progressDialog_PushApp_Cancel;
            var progressDialogDispatcher = progressDialog.Dispatcher;

            worker = new BackgroundWorker();
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += delegate(object s, DoWorkEventArgs args)
            {
                if (worker.CancellationPending) { args.Cancel = true; return; }
                var eventargs = args.Argument as string[];
                var url = eventargs[0];
                var username = eventargs[1];
                var appurl = eventargs[2];
                var appname = eventargs[3];
                var password = eventargs[4];

                var update = new UpdateProgressDelegate(UpdateProgressText);
                var updateResponse = new UpdateResponseDelegate(UpdateResponse);

                if (Directory.Exists(precompiledSitePath))
                    Directory.Delete(precompiledSitePath, true);

                try
                {
                    ProcessStartInfo processStart = new ProcessStartInfo();
                    processStart.FileName = frameworkPath + "\\aspnet_compiler.exe";                   
                    processStart.Arguments = string.Format("-v / -nologo -p \"{0}\" -u -c \"{1}\"", projectDirectory, precompiledSitePath);
                    processStart.CreateNoWindow = true;
                    processStart.ErrorDialog = false;
                    processStart.UseShellExecute = false;
                    processStart.RedirectStandardOutput = true;

                    System.Diagnostics.Process process = new System.Diagnostics.Process();
                    process.StartInfo = processStart;
                    process.Start();
                    var output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();

                    if (worker.CancellationPending) { args.Cancel = true; return; }

                    var cfm = new VmcManager();
                    cfm.URL = "http://" + url;
                    string token = cfm.LogIn(username, password);
                    JObject parse = JObject.Parse(token);
                    var obj = (string)parse["token"];
                    cfm.AccessToken = obj;
                    progressDialogDispatcher.BeginInvoke(update, string.Format("Pushing {0}", appurl), 65);
                    if (worker.CancellationPending) { args.Cancel = true; return; }

                    var response = cfm.Push(appname, appurl, precompiledSitePath, "aspdotnet", "64");
                    progressDialogDispatcher.BeginInvoke(update, "Complete.", 100);
                    progressDialogDispatcher.BeginInvoke(updateResponse, response);
                }
                catch (Exception ex)
                {
                    progressDialogDispatcher.BeginInvoke(updateResponse, ex.Message + ":" + ex.StackTrace);
                }
            };

            worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
            {
                progressDialog.OkButtonEnabled = true;
                progressDialog.CancelButtonEnabled = false;
            };

            worker.RunWorkerAsync(new string[] {CloudControllerUrl.Text,
                                                UsernameTextBox.Text,
                                                ApplicationUrlTextBox.Text,
                                                ApplicationNameTextBox.Text,
                                                PasswordTextBox.Password});
            progressDialog.ShowDialog();

            Close();
        }

        private void PublishProject(System.Windows.Threading.Dispatcher progressDialogDispatcher, UpdateProgressDelegate update)
        {
            
        }

        public delegate void UpdateProgressDelegate(string logtext, int percentage);
        private void UpdateProgressText(string logtext, int percentage)
        {
            progressDialog.LogInfo = logtext;
            progressDialog.ProgressValue = percentage;
        }

        public delegate void UpdateResponseDelegate(string response);
        private void UpdateResponse(string response)
        {
            progressDialog.Response = response;
        }

        private void progressDialog_PushApp_Cancel(object sender, EventArgs e)
        {
            worker.CancelAsync();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ApplicationNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplicationUrlTextBox.Text = ApplicationUrlTextBox.Text.ToLower();
            ApplicationUrlTextBox.Text = ApplicationNameTextBox.Text + ".vcap.me";
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
           
            this.Close();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


    }
}
