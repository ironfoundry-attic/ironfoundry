namespace IronFoundry.Vcap
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class VmcException : Exception, ISerializable
    {
        public VmcException() { }

        public VmcException(string message)
            : base(message) { }

        public VmcException(string message, Exception inner)
            : base(message, inner) { }

        protected VmcException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }

    [Serializable]
    public class VmcNotFoundException : VmcException
    {
        public VmcNotFoundException() { }

        public VmcNotFoundException(string message)
            : base(message) { }

        public VmcNotFoundException(string message, Exception inner)
            : base(message, inner) { }

        protected VmcNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }

    [Serializable]
    public class VmcTargetException : VmcException
    {
        public VmcTargetException() { }

        public VmcTargetException(string message)
            : base(message) { }

        public VmcTargetException(string message, Exception inner)
            : base(message, inner) { }

        protected VmcTargetException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }

    [Serializable]
    public class VmcAuthException : VmcException
    {
        public VmcAuthException() { }

        public VmcAuthException(string message)
            : base(message) { }

        public VmcAuthException(string message, Exception inner)
            : base(message, inner) { }

        protected VmcAuthException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}