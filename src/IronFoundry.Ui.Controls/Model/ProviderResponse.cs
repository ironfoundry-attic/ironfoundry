namespace IronFoundry.Ui.Controls.Model
{
    using System;

    public class ProviderResponse<T>
    {
        public T Response { get; set; }
        public string Message { get; set; }
        
        public ProviderResponse()
        {
            Response = default(T);
            Message = String.Empty;
        }

        public ProviderResponse(T response, string message)
        {
            Response = response;
            Message = message;
        }
    }
}