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

namespace CloudFoundry.Net.VsExtension
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(GuidList.guidCloudFoundryPkgString)]
    [ProvideBindingPath]
    public sealed class CloudFoundryPackage : Package
    {
        private IVsMonitorSelection _vsMonitorSelection;

        private CloudFoundryProvider provider;

        public CloudFoundryPackage()
        {            
        }

        protected override void Initialize()
        {
            _vsMonitorSelection = (IVsMonitorSelection)GetService(typeof(IVsMonitorSelection));         
            base.Initialize();

            Application.Current.Resources.MergedDictionaries.Add(Application.LoadComponent(new Uri("CloudFoundry.Net.VsExtension.Ui.Controls;component/Resources/Expander.xaml", UriKind.Relative)) as ResourceDictionary);
            Application.Current.Resources.MergedDictionaries.Add(Application.LoadComponent(new Uri("CloudFoundry.Net.VsExtension.Ui.Controls;component/Resources/Resources.xaml", UriKind.Relative)) as ResourceDictionary);

            if (provider == null)
            {
                PreferencesProvider preferences = new PreferencesProvider("VisualStudio2010");
                provider = new CloudFoundryProvider(preferences, new Vmc.VcapClient(), new Vmc.VcapCredentialManager());
            }

            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if ( null != mcs )
            {
                mcs.AddCommand(new MenuCommand(CloudFoundryExplorer,
                               new CommandID(GuidList.guidCloudFoundryCmdSet,
                               (int)PkgCmdIDList.cmdidCloudFoundryExplorer)));

                mcs.AddCommand(new MenuCommand(EditCloudFoundryProperties,
                               new CommandID(GuidList.guidCloudFoundryCmdSet,
                               (int)PkgCmdIDList.cmdidEditCloudFoundryProperties)));
            }
        }

        private void CloudFoundryExplorer(object sender, EventArgs e)
        {
            
            var window = new MainWindow();
            //var parentWindow = System.Windows.Window.GetWindow();
            //window.Owner = parentWindow;
            window.ShowDialog();
        }

        private void EditCloudFoundryProperties(object sender, EventArgs e)
        {
            Project project = _vsMonitorSelection.GetActiveProject();
            if (project != null)
            {
                var window = new ManageWindow(project);
                window.ShowModal();
            }
        }
    }
}
