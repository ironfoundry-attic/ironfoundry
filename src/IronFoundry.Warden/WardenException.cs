namespace IronFoundry.Warden
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class WardenException : Exception, ISerializable
    {
        public WardenException() : base()
        {
        }

        public WardenException(string message) : base(message)
        {
        }

        public WardenException(string message, params object[] args) : this(String.Format(message, args))
        {
        }

        public WardenException(string message, Exception inner) : base(message, inner)
        {
        }

        public WardenException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
