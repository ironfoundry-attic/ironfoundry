namespace IronFoundry.Bosh.Agent
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class MessageHandlerException : Exception, ISerializable
    {
        private readonly string blob;

        public MessageHandlerException() { }

        public MessageHandlerException(string message, string blob)
            : base(message)
        {
            this.blob = blob;
        }

        public MessageHandlerException(Exception inner)
            : base(null, inner) { }

        public MessageHandlerException(string message)
            : this(message, (Exception)null) { }

        public MessageHandlerException(string message, Exception inner)
            : base(message, inner) { }

        protected MessageHandlerException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        public string Blob { get { return blob; } }
    }
}