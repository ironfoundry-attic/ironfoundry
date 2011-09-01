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

namespace CloudFoundry.CloudFoundry_VS2k10
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(GuidList.guidCloudFoundry_VS2k10PkgString)]
    public sealed class CloudFoundry_VS2k10Package : Package
    {
        private DTE _dte;
        private IVsMonitorSelection _vsMonitorSelection;

        public CloudFoundry_VS2k10Package()
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
        }
        #region Package Members

        protected override void Initialize()
        {
            Trace.WriteLine (string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();
            _vsMonitorSelection = (IVsMonitorSelection)GetService(typeof(IVsMonitorSelection));

            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if ( null != mcs )
            {
                CommandID menuCommandID = new CommandID(GuidList.guidCloudFoundry_VS2k10CmdSet, (int)PkgCmdIDList.cmdidDeployToCloudFoundryCommand);
                MenuCommand menuItem = new MenuCommand(MenuItemCallback, menuCommandID );
                mcs.AddCommand( menuItem );
            }
        }
        #endregion

        private void MenuItemCallback(object sender, EventArgs e)
        {
            Project project = _vsMonitorSelection.GetActiveProject();
            if (project != null)
            {
                DialogWindow window = new ManageWindow(project);
                window.ShowModal();
            }
        }
    }
}
