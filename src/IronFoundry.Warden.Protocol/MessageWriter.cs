namespace IronFoundry.Warden.Protocol
{
    using System;
    using System.IO;
    using System.Text;
    using ProtoBuf;

    public class MessageWriter
    {
        private readonly Stream destination;

        public MessageWriter(Stream destination)
        {
            if (destination == null)
            {
                throw new ArgumentNullException("destination");
            }
            else if (!destination.CanWrite)
            {
                throw new ArgumentException("destination stream is unwritable");
            }
            this.destination = destination;
        }

        public void Write(Response response)
        {
            if (response == null)
            {
                throw new ArgumentNullException("response");
            }
            var wrapper = new ResponseWrapper(response);
            Message message = wrapper.GetMessage();

            byte[] responsePayload = null;
            using (var ms = new MemoryStream())
            {
                Serializer.Serialize(ms, message);
                responsePayload = ms.ToArray();
            }

            int payloadLen = responsePayload.Length;
            var payloadLenBytes = Encoding.ASCII.GetBytes(payloadLen.ToString());

            lock (destination)
            {
                destination.Write(payloadLenBytes, 0, payloadLenBytes.Length);
                destination.WriteByte(Constants.CR);
                destination.WriteByte(Constants.LF);
                destination.Write(responsePayload, 0, responsePayload.Length);
                destination.WriteByte(Constants.CR);
                destination.WriteByte(Constants.LF);
            }
        }
    }
}
