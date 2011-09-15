namespace CloudFoundry.Net
{
    using System;

    public class FatalErrorEventArgs : EventArgs
    {
        public string Message { get; private set; }

        public FatalErrorEventArgs(string argMessage)
        {
            Message = argMessage ?? String.Empty;
        }
    }
}