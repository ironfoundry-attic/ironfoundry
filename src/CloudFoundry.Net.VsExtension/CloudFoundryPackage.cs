using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
//using System.Windows.Forms;
using EnvDTE;
using CloudFoundry.Net.VsExtension.Ui.Controls.Model;
using System.Windows;
using System.Windows.Interop;
using CloudFoundry.Net.VsExtension.Ui.Controls.Views;
using System.IO;
using Microsoft.Build.Utilities;
using GalaSoft.MvvmLight.Messaging;
using CloudFoundry.Net.VsExtension.Ui.Controls.Utilities;
using CloudFoundry.Net.VsExtension.Ui.Controls.ViewModel;
using System.ComponentModel;
using CloudFoundry.Net.Types;
using CloudFoundry.Net.Vmc;

namespace CloudFoundry.Net.VsExtension
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(GuidList.guidCloudFoundryPkgString)]
    [ProvideBindingPath]
    public sealed class CloudFoundryPackage : Package
    {
        private IVsMonitorSelection vsMonitorSelection;
        private DTE dte;       
        private CloudFoundryProvider provider;
        private BackgroundWorker worker;
        private ProgressDialog progressDialog;
        public delegate void UpdateResponseDelegate(string response);
        public delegate void UpdateProgressDelegate(string logtext, int percentage);

        public CloudFoundryPackage()
        {            
        }

        protected override void Initialize()
        {              
            base.Initialize();

            vsMonitorSelection = (IVsMonitorSelection)GetService(typeof(IVsMonitorSelection));
            dte = GetService(typeof(SDTE)) as DTE;

            System.Windows.Application.Current.Resources.MergedDictionaries.Add(System.Windows.Application.LoadComponent(new Uri("CloudFoundry.Net.VsExtension.Ui.Controls;component/Resources/Expander.xaml", UriKind.Relative)) as ResourceDictionary);
            System.Windows.Application.Current.Resources.MergedDictionaries.Add(System.Windows.Application.LoadComponent(new Uri("CloudFoundry.Net.VsExtension.Ui.Controls;component/Resources/Resources.xaml", UriKind.Relative)) as ResourceDictionary);

            if (provider == null)
            {
                PreferencesProvider preferences = new PreferencesProvider("VisualStudio2010");
                provider = new CloudFoundryProvider(preferences);
            }

            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if ( null != mcs )
            {
                mcs.AddCommand(new MenuCommand(CloudFoundryExplorer,
                               new CommandID(GuidList.guidCloudFoundryCmdSet,
                               (int)PkgCmdIDList.cmdidCloudFoundryExplorer)));

                mcs.AddCommand(new MenuCommand(PushApplication,
                               new CommandID(GuidList.guidCloudFoundryCmdSet,
                               (int)PkgCmdIDList.cmdidPushCloudFoundryApplication)));

                mcs.AddCommand(new MenuCommand(UpdateApplication,
                               new CommandID(GuidList.guidCloudFoundryCmdSet,
                               (int)PkgCmdIDList.cmdidUpdateCloudFoundryApplication)));
                
            }
        }

        private void CloudFoundryExplorer(object sender, EventArgs e)
        {
            var window = new Explorer();
            WindowInteropHelper helper = new WindowInteropHelper(window);
            helper.Owner = (IntPtr)(dte.MainWindow.HWnd);
            window.ShowDialog();
        }

        private void PushApplication(object sender, EventArgs e)
        {
            Project project = vsMonitorSelection.GetActiveProject();
            if (project != null)
            {                
                Guid cloudGuid = GetCurrentCloudGuid(project);

                Messenger.Default.Register<NotificationMessageAction<Guid>>(this,
                    message =>
                    {
                        if (message.Notification.Equals(Messages.SetPushAppData))
                            message.Execute(cloudGuid);
                    });

                var window = new Push();
                WindowInteropHelper helper = new WindowInteropHelper(window);
                helper.Owner = (IntPtr)(dte.MainWindow.HWnd);
                var result = window.ShowDialog();

                PushViewModel modelData = null;
                Messenger.Default.Send(new NotificationMessageAction<PushViewModel>(Messages.GetPushAppData,
                    model => 
                    {
                        modelData = model;
                    }));

                SetCurrentCloudGuid(project, modelData.SelectedCloud.ID);
                PerformPush(project, modelData.SelectedCloud, modelData.Name, modelData.Url, modelData.SelectedMemory,modelData.Instances);
            }
        }

        private void UpdateApplication(object sender, EventArgs e)
        {

            Project project = vsMonitorSelection.GetActiveProject();
            if (project != null)
            {
                Guid cloudGuid = GetCurrentCloudGuid(project);

                Messenger.Default.Register<NotificationMessageAction<Guid>>(this,
                    message =>
                    {
                        if (message.Notification.Equals(Messages.SetUpdateAppData))
                            message.Execute(cloudGuid);
                    });

                var window = new Update();
                WindowInteropHelper helper = new WindowInteropHelper(window);
                helper.Owner = (IntPtr)(dte.MainWindow.HWnd);
                var result = window.ShowDialog();

                UpdateViewModel modelData = null;
                Messenger.Default.Send(new NotificationMessageAction<UpdateViewModel>(Messages.GetUpdateAppData,
                    model =>
                    {
                        modelData = model;
                    }));

                SetCurrentCloudGuid(project, modelData.SelectedCloud.ID);
                // Update app
            }
        }

        private void PerformPush(Project project, 
                                 Cloud cloud, 
                                 string name, 
                                 string url,
                                 int memory,
                                 int instances)
        {            
            progressDialog = new ProgressDialog("Push Application...", "Saving project...");
            progressDialog.Cancel += (s,e) => worker.CancelAsync();
            var progressDialogDispatcher = progressDialog.Dispatcher;

            worker = new BackgroundWorker();
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += (s,args) =>
            {
                if (worker.CancellationPending) { args.Cancel = true; return; }

                var projectProperties = project.DTE.Properties["Environment", "ProjectsAndSolution"];
                var defaultProjectLocation = projectProperties.Item("ProjectsLocation").Value as string;
                var stagingPath = Path.Combine(defaultProjectLocation, "CloudFoundry_Staging");
                var projectDirectory = Path.GetDirectoryName(project.FullName);
                var projectName = Path.GetFileNameWithoutExtension(projectDirectory).ToLower();
                var site = project.Object as VsWebSite.VSWebSite;
                var precompiledSitePath = Path.Combine(stagingPath, projectName);
                var frameworkPath = project.GetFrameworkPath();

                if (!Directory.Exists(stagingPath))
                    Directory.CreateDirectory(stagingPath);

                if (Directory.Exists(precompiledSitePath))
                    Directory.Delete(precompiledSitePath, true);

                var update = new UpdateProgressDelegate(UpdateProgressText);
                var updateResponse = new UpdateResponseDelegate(UpdateResponse);
                                                      
                try
                {
                    var process = new System.Diagnostics.Process()
                    {
                        StartInfo = new ProcessStartInfo()
                        {
                            FileName = frameworkPath + "\\aspnet_compiler.exe",
                            Arguments = string.Format("-v / -nologo -p \"{0}\" -u -c \"{1}\"", projectDirectory, precompiledSitePath),
                            CreateNoWindow = true,
                            ErrorDialog = false,
                            UseShellExecute = false,
                            RedirectStandardOutput = true
                        }
                    };                    
                    process.Start();
                    var output = process.StandardOutput.ReadToEnd();                    
                    process.WaitForExit();
                    if (!string.IsNullOrEmpty(output))
                        throw new Exception("Asp Compile Error: " + output);

                    if (worker.CancellationPending) { args.Cancel = true; return; }

                    var cfm = new VcapClient(cloud);
                    VcapClientResult result = cfm.Login();

                    progressDialogDispatcher.BeginInvoke(update, string.Format("Pushing {0}", cloud.Url), 65);
                    if (worker.CancellationPending) { args.Cancel = true; return; }

                    VcapClientResult response = cfm.Push(name, url, new DirectoryInfo(precompiledSitePath),Convert.ToUInt32(memory));
                    progressDialogDispatcher.BeginInvoke(update, "Complete.", 100);
                    progressDialogDispatcher.BeginInvoke(updateResponse, response.Message);
                }
                catch (Exception ex)
                {
                    progressDialogDispatcher.BeginInvoke(updateResponse, ex.Message + ":" + ex.StackTrace);
                }
            };

            worker.RunWorkerCompleted += (s,args) =>
            {
                progressDialog.OkButtonEnabled = true;
                progressDialog.CancelButtonEnabled = false;
            };

            worker.RunWorkerAsync();
            progressDialog.ShowDialog();
        }

        private void UpdateProgressText(string logtext, int percentage)
        {
            progressDialog.LogInfo = logtext;
            progressDialog.ProgressValue = percentage;
        }

        private void UpdateResponse(string response)
        {
            progressDialog.Response = response;
        }               

        private static Guid GetCurrentCloudGuid(Project project)
        {
            Guid cloudGuid = Guid.Empty;
            var cloudId = project.GetGlobalVariable("CloudId");
            if (!Guid.TryParse(cloudId, out cloudGuid))
                cloudGuid = Guid.Empty;
            return cloudGuid;
        }

        private static void SetCurrentCloudGuid(Project project, Guid guid)
        {            
            project.SetGlobalVariable("CloudId", guid.ToString());
        }

        
    }
}
