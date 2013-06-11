namespace IronFoundry.Warden.Server
{
    using System;
    using IronFoundry.Warden.Protocol;

    public class WardenExceptionHandler
    {
        private readonly Exception exception;
        private readonly MessageWriter messageWriter;

        public WardenExceptionHandler(Exception exception, MessageWriter messageWriter)
        {
            if (exception == null)
            {
                throw new ArgumentNullException("exception");
            }
            this.exception = exception;

            if (messageWriter == null)
            {
                throw new ArgumentNullException("messageWriter");
            }
            this.messageWriter = messageWriter;
        }

        public bool Handle()
        {
            bool handled = false;

            var wardenException = exception as WardenException;
            if (wardenException != null)
            {
                var response = new ErrorResponse
                {
                    Message = wardenException.ResponseMessage + "\n",
                    Data = wardenException.StackTrace
                };
                messageWriter.Write(response);
                handled = true;
            }

            return handled;
        }
    }
}
