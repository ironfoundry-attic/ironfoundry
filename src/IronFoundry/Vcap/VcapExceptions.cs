namespace IronFoundry.Vcap
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class VcapException : Exception, ISerializable
    {
        public VcapException() { }

        public VcapException(string message)
            : base(message) { }

        public VcapException(string message, Exception inner)
            : base(message, inner) { }

        protected VcapException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }

    [Serializable]
    public class VcapNotFoundException : VcapException
    {
        public VcapNotFoundException() { }

        public VcapNotFoundException(string message)
            : base(message) { }

        public VcapNotFoundException(string message, Exception inner)
            : base(message, inner) { }

        protected VcapNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }

    [Serializable]
    public class VcapTargetException : VcapException
    {
        public VcapTargetException() { }

        public VcapTargetException(string message)
            : base(message) { }

        public VcapTargetException(string message, Exception inner)
            : base(message, inner) { }

        protected VcapTargetException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }

    [Serializable]
    public class VcapAuthException : VcapException
    {
        public VcapAuthException() { }

        public VcapAuthException(string message)
            : base(message) { }

        public VcapAuthException(string message, Exception inner)
            : base(message, inner) { }

        protected VcapAuthException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}