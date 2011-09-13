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
using System.Windows.Forms;
using EnvDTE;

namespace CloudFoundry.Net.VsExtension
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(GuidList.guidCloudFoundryPkgString)]
    public sealed class CloudFoundryPackage : Package
    {
        private IVsMonitorSelection _vsMonitorSelection;

        public CloudFoundryPackage()
        {            
        }

        protected override void Initialize()
        {
            _vsMonitorSelection = (IVsMonitorSelection)GetService(typeof(IVsMonitorSelection));         
            base.Initialize();

            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if ( null != mcs )
            {
                mcs.AddCommand(new MenuCommand(ManageCloudFoundry,
                               new CommandID(GuidList.guidCloudFoundryCmdSet,
                               (int)PkgCmdIDList.cmdidManageCloudFoundry)));

                mcs.AddCommand(new MenuCommand(EditCloudFoundryProperties,
                               new CommandID(GuidList.guidCloudFoundryCmdSet,
                               (int)PkgCmdIDList.cmdidEditCloudFoundryProperties)));
            }
        }

        private void ManageCloudFoundry(object sender, EventArgs e)
        {
            Project project = _vsMonitorSelection.GetActiveProject();
            if (project != null)
            {
                var window = new ManageWindow(project);
                window.ShowModal();
            }
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
