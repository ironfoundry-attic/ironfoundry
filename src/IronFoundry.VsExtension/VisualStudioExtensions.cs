namespace CloudFoundry.Net.VsExtension
{
    using System;
    using System.Runtime.InteropServices;
    using EnvDTE;
    using Microsoft.Build.Utilities;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;

    public static class VisualStudioExtensions
    {
        private const int Fx40 = 262144;
        private const int Fx35 = 196613;
        private const int Fx30 = 196608;
        private const int Fx20 = 131072;

        public static Project GetActiveProject(this IVsMonitorSelection vsMonitorSelection)
        {
            IntPtr ptrHeirarchy = IntPtr.Zero;
            uint intPtrItemId;
            IVsMultiItemSelect ptrMultiItemSelect;
            IntPtr intPtrResult = IntPtr.Zero;

            try
            {
                vsMonitorSelection.GetCurrentSelection(out ptrHeirarchy, out intPtrItemId, out ptrMultiItemSelect, out intPtrResult);

                if (ptrHeirarchy == IntPtr.Zero)
                    return null;

                if (intPtrItemId == (uint)VSConstants.VSITEMID.Selection)
                    return null;

                IVsHierarchy hierarchy = Marshal.GetTypedObjectForIUnknown(ptrHeirarchy, typeof(IVsHierarchy)) as IVsHierarchy;
                if (hierarchy != null)
                {
                    object project = null;
                    if (hierarchy.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ExtObject, out project) >= 0)
                        return (Project)project;
                }

                return null;
            }
            finally
            {
                if (ptrHeirarchy != IntPtr.Zero)
                    Marshal.Release(ptrHeirarchy);
                if (intPtrResult != IntPtr.Zero)
                    Marshal.Release(intPtrResult);
            }
        }

        public static string GetFrameworkPath(this Project project)
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

            return ToolLocationHelper.GetPathToDotNetFramework(version, arch);
        }

        public static string GetGlobalVariable(this Project project, string key)
        {
            string returnValue = string.Empty;
            if (project.Globals.get_VariableExists(key))
                returnValue = project.Globals[key] as string;
            return returnValue;
        }

        public static void SetGlobalVariable(this Project project, string key, string value)
        {
            project.Globals[key] = value;
            project.Globals.set_VariablePersists(key, true);
        }
    }
}
