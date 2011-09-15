namespace CloudFoundry.Net
{
    using System;
    using System.Threading;

    public static class TimerExtensionMethods
    {
        public static void Stop(this Timer argThis)
        {
            argThis.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public static void Restart(this Timer argThis, TimeSpan argInterval)
        {
            argThis.Change(argInterval, argInterval);
        }
    }
}