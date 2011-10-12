using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using CloudFoundry.Net.Types;
using CloudFoundry.Net.Vmc;
using CloudFoundry.Net.VsExtension.Ui.Controls.Model;
using CloudFoundry.Net.VsExtension.Ui.Controls.Utilities;
using CloudFoundry.Net.VsExtension.Ui.Controls.ViewModel;
using CloudFoundry.Net.VsExtension.Ui.Controls.Views;
using EnvDTE;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.Collections.Generic;

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

        public CloudFoundryPackage()
        {            
        }

        protected override void Initialize()
        {              
            base.Initialize();

            vsMonitorSelection = (IVsMonitorSelection)GetService(typeof(IVsMonitorSelection));
            dte = GetService(typeof(SDTE)) as DTE;
            
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

                if (result.GetValueOrDefault())
                {
                    PushViewModel modelData = null;
                    Messenger.Default.Send(new NotificationMessageAction<PushViewModel>(Messages.GetPushAppData, model => modelData = model));
                    SetCurrentCloudGuid(project, modelData.SelectedCloud.ID);

                    List<string> services = new List<string>();
                    foreach (var provisionedService in modelData.ApplicationServices)
                        services.Add(provisionedService.Name);
                    PerformAction("Push Application", project, modelData.SelectedCloud, (c, d) => 
                        c.Push(modelData.Name, modelData.Url, modelData.Instances, d, Convert.ToUInt32(modelData.SelectedMemory), services.ToArray()));
                }
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

                if (result.GetValueOrDefault())
                {
                    UpdateViewModel modelData = null;
                    Messenger.Default.Send(new NotificationMessageAction<UpdateViewModel>(Messages.GetUpdateAppData, model => modelData = model));

                    SetCurrentCloudGuid(project, modelData.SelectedCloud.ID);
                    PerformAction("Update Application",project, modelData.SelectedCloud, (c, d) => 
                        c.Update(modelData.SelectedApplication.Name, d));
                }
            }
        }        

        private void PerformAction(string action, Project project, Cloud cloud, Func<VcapClient,DirectoryInfo,VcapClientResult> function)
        {    
            var worker = new BackgroundWorker();
            var progress = new ProgressDialog(action, "Saving project...");
            var dispatcher = progress.Dispatcher;
            var helper = new WindowInteropHelper(progress);
            helper.Owner = (IntPtr)(dte.MainWindow.HWnd);
            progress.Cancel += (s,e) => worker.CancelAsync();            
           
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += (s,args) =>
            {
                if (worker.CancellationPending) { args.Cancel = true; return; }

                var projectProperties = project.DTE.Properties["Environment", "ProjectsAndSolution"];
                var defaultProjectLocation = projectProperties.Item("ProjectsLocation").Value as string;
                var stagingPath = Path.Combine(defaultProjectLocation, "CloudFoundry_Staging");
                var projectDirectory = Path.GetDirectoryName(project.FullName);
                var projectName = Path.GetFileNameWithoutExtension(projectDirectory).ToLower();
                //var site = project.Object as VsWebSite.VSWebSite;
                var precompiledSitePath = Path.Combine(stagingPath, projectName);
                var frameworkPath = project.GetFrameworkPath();

                if (!Directory.Exists(stagingPath))
                    Directory.CreateDirectory(stagingPath);

                if (Directory.Exists(precompiledSitePath))
                    Directory.Delete(precompiledSitePath, true);

                Action<string,int> update = (logtext,percent) => { progress.LogInfo = logtext; progress.ProgressValue = percent; };
                Action<string> updateResponse = (response) => progress.Response = response;
                                                      
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
                    var client = new VcapClient(cloud);
                    var result = client.Login();
                    if (result.Success == false)
                        throw new Exception("Vcap Login Failure: " + result.Message);

                    dispatcher.BeginInvoke(update, string.Format("Sending to {0}", cloud.Url), 65);
                    if (worker.CancellationPending) { args.Cancel = true; return; }

                    var response = function(client,new DirectoryInfo(precompiledSitePath));
                    if (response.Success == false)
                        throw new Exception("Vcap Action Failure: " + response.Message);
                    
                    dispatcher.BeginInvoke(update, "Complete.", 100);
                    dispatcher.BeginInvoke(updateResponse, response.Message);
                }
                catch (Exception ex)
                {
                    dispatcher.BeginInvoke(updateResponse, ex.Message + ":" + ex.StackTrace);
                }
            };

            worker.RunWorkerCompleted += (s,args) =>
            {
                progress.OkButtonEnabled = true;
                progress.CancelButtonEnabled = false;
            };

            worker.RunWorkerAsync();
            progress.ShowDialog();
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
