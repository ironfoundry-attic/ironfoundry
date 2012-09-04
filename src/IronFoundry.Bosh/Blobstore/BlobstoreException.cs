namespace IronFoundry.Bosh.Blobstore
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class BlobstoreException : Exception, ISerializable
    {
        public BlobstoreException() { }

        public BlobstoreException(string format, params object[] args)
            : this(String.Format(format, args)) { }

        public BlobstoreException(string message)
            : base(message) { }

        public BlobstoreException(string message, Exception inner)
            : base(message, inner) { }

        protected BlobstoreException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}