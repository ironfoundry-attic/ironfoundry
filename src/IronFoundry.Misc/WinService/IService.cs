namespace IronFoundry.Misc.WinService
{
    using System;

    public interface IService
    {
        string ServiceName { get; }
        ushort StartIndex { get; }
        StartServiceResult StartService(IntPtr serviceHandle);
        void StopService();
    }

    public class StartServiceResult
    {
        private bool _success = true;

        public bool Success
        {
            get { return _success; }
            set { _success = value; }
        }

        public string Message { get; set; }

        public ushort ExitCode { get; set; }
    }
}