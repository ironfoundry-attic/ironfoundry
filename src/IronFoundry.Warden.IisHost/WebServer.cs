namespace IronFoundry.Warden.IisHost
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.IO;
    using System.Runtime.InteropServices;
    using Properties;

    internal class WebServer : IDisposable
    {
        private readonly string appHostConfigPath;
        private readonly string rootWebConfigPath;

        public WebServer(string physicalPath, uint port, uint siteId, string frameworkPath)
        {
            string appPoolName = "AppPool" + port.ToString();
            appHostConfigPath = Path.Combine(Path.GetTempPath(), Path.GetTempFileName() + ".config");
            rootWebConfigPath = Environment.ExpandEnvironmentVariables(Path.Combine(frameworkPath, @"config\web.config"));

            string appHostContent = Resources.AppHostAspNet;
            //string appHostContent = Resources.AppHostStaticFiles;

            File.WriteAllText(appHostConfigPath,
                String.Format(appHostContent,
                              port,
                              physicalPath,
                              frameworkPath,
                              siteId,
                              appPoolName));
        }

        public void Dispose()
        {
            Stop();
        }

        public void Start()
        {
            if (!HostableWebCore.IsActivated)
            {
                HostableWebCore.Activate(appHostConfigPath, rootWebConfigPath, Guid.NewGuid().ToString());
            }
        }

        public void Stop()
        {
            if (HostableWebCore.IsActivated)
            {
                HostableWebCore.Shutdown(false);
            }
        }

        #region Hostable WebCore
        internal static class HostableWebCore
        {
            const string HWCPath = @"%windir%\system32\inetsrv\hwebcore.dll";

            private static bool _isActivated;

            private delegate int FnWebCoreShutdown(bool immediate);
            private delegate int FnWebCoreActivate(
                [In, MarshalAs(UnmanagedType.LPWStr)]string appHostConfig,
                [In, MarshalAs(UnmanagedType.LPWStr)]string rootWebConfig,
                [In, MarshalAs(UnmanagedType.LPWStr)]string instanceName);

            private static readonly FnWebCoreActivate WebCoreActivate;
            private static readonly FnWebCoreShutdown WebCoreShutdown;

            static HostableWebCore()
            {
               // Load the library and get the function pointers for the WebCore entry points
                IntPtr hwc = NativeMethods.LoadLibrary(Environment.ExpandEnvironmentVariables(HWCPath));

                IntPtr procaddr = NativeMethods.GetProcAddress(hwc, "WebCoreActivate");
                WebCoreActivate = (FnWebCoreActivate) Marshal.GetDelegateForFunctionPointer(procaddr, typeof(FnWebCoreActivate));

                procaddr = NativeMethods.GetProcAddress(hwc, "WebCoreShutdown");
                WebCoreShutdown = (FnWebCoreShutdown) Marshal.GetDelegateForFunctionPointer(procaddr, typeof(FnWebCoreShutdown));
            }

            /// <summary>
            /// Specifies if Hostable WebCore ha been activated
            /// </summary>
            public static bool IsActivated
            {
                get { return _isActivated; }
            }

            /// <summary>
            /// Activate the HWC
            /// </summary>
            /// <param name="appHostConfig">Path to ApplicationHost.config to use</param>
            /// <param name="rootWebConfig">Path to the Root Web.config to use</param>
            /// <param name="instanceName">Name for this instance</param>
            public static void Activate(string appHostConfig, string rootWebConfig, string instanceName)
            {
                int result = WebCoreActivate(appHostConfig, rootWebConfig, instanceName);
                if (result != 0)
                {
                    Marshal.ThrowExceptionForHR(result);
                }

                _isActivated = true;
            }

            /// <summary>
            /// Shutdown HWC
            /// </summary>
            public static void Shutdown(bool immediate)
            {
                if (_isActivated)
                {
                    WebCoreShutdown(immediate);
                    _isActivated = false;
                }
            }

            private static class NativeMethods
            {
                [DllImport("kernel32.dll", SetLastError = true)]
                internal static extern IntPtr LoadLibrary(String dllname);

                [DllImport("kernel32.dll", SetLastError = true)]
                internal static extern IntPtr GetProcAddress(IntPtr hModule, String procname);
            }
        }
        #endregion
    }
}
