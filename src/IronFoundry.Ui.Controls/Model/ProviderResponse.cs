namespace IronFoundry.Ui.Controls.Model
{
    public class ProviderResponse<T>
    {
        public T Response { get; set; }
        public string Message { get; set; }
        
        public ProviderResponse()
        {
            Response = default(T);
            Message = string.Empty;
        }

        public ProviderResponse(T response, string message)
        {
            Response = response;
            Message = message;
        }
    }
}