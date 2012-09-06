namespace IronFoundry.Bosh.Agent
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class AgentException : Exception, ISerializable
    {
        public AgentException() { }

        public AgentException(string format, params object[] args)
            : this(String.Format(format, args)) { }

        public AgentException(string message)
            : base(message) { }

        public AgentException(string message, Exception inner)
            : base(message, inner) { }

        protected AgentException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}