using System;
using EnvDTE;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;
using System.Runtime.InteropServices;

namespace CloudFoundry.Net.VsExtension
{
    public static class VisualStudioExtensions
    {
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
    }
}
