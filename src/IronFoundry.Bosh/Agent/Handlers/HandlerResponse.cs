namespace IronFoundry.Bosh.Agent.Handlers
{
    using System;
    using Newtonsoft.Json;

    public class HandlerResponse
    {
        private readonly object value;

        public HandlerResponse(object value)
        {
            if (null == value)
            {
                throw new ArgumentNullException("value");
            }
            this.value = value;
        }

        [JsonProperty("value")]
        public object Value { get { return value; } }
    }
}