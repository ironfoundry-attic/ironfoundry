namespace CloudFoundry.Net.Vmc
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class VmcNotFoundException : Exception, ISerializable
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
    public class VmcTargetException : Exception, ISerializable
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
    public class VmcAuthException : Exception, ISerializable
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