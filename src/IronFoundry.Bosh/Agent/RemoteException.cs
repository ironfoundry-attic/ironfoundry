namespace IronFoundry.Bosh.Agent
{
    using System;

    public class RemoteException
    {
        private string message;
        private string backtrace;
        private string blob;

        public RemoteException(string message, string backtrace = null, string blob = null)
        {
            this.message = message;
            this.backtrace = backtrace;
            this.blob = blob;
        }

        public static RemoteException From(Exception ex)
        {
            string blob = null;

            var messageHandlerException = ex as MessageHandlerException;
            if (null != messageHandlerException)
            {
                blob = messageHandlerException.Blob;
            }

            return new RemoteException(ex.Message, ex.StackTrace, blob);
        }

        public string Message { get { return message; } }

        public string Backtrace { get { return message; } }

        public string Blob { get { return blob; } }
    }
}