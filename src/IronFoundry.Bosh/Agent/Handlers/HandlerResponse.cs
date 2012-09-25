namespace IronFoundry.Bosh.Agent.Handlers
{
    using System;
    using IronFoundry.Nats.Client;
    using Newtonsoft.Json;

    public class HandlerResponse : NatsMessage
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

        [JsonProperty(PropertyName="value")]
        public object Value { get { return value; } }

        public override string PublishSubject
        {
            get { return String.Empty; } // TODO Unused and ugly
        }
    }
}