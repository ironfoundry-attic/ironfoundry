namespace IronFoundry.Bosh.Monit
{
    using System;
    using Microsoft.Win32;

    public class IIsMonitor : IMonitor
    {
        public void Start()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public object Status()
        {
            throw new NotImplementedException();
        }

        private Version GetIIsVersion()
        {
            using (RegistryKey componentsKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\InetStp", false))
            {
                if (componentsKey != null)
                {
                    int majorVersion = (int)componentsKey.GetValue("MajorVersion", -1);
                    int minorVersion = (int)componentsKey.GetValue("MinorVersion", -1);
                    if (majorVersion != -1 && minorVersion != -1)
                    {
                        return new Version(majorVersion, minorVersion);
                    }
                }
                return new Version(0, 0);
            }
        }
    }
}