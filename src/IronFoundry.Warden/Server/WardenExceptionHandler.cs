namespace IronFoundry.Warden.Server
{
    using System;
    using System.Threading.Tasks;
    using Protocol;
    using Utilities;

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

        public async Task<bool> HandleAsync()
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
                await messageWriter.WriteAsync(response);
                handled = true;
            }

            return handled;
        }
    }
}
