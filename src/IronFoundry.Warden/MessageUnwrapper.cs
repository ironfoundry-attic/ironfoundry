namespace IronFoundry.Warden
{
    using System;
using System.Collections.Generic;
using System.IO;
using IronFoundry.Warden.Containers;
using IronFoundry.Warden.Protocol;
using NLog;
using ProtoBuf;

    public class MessageUnwrapper
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger(); 
        private readonly Message message;

        public MessageUnwrapper(Message message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }
            this.message = message;
        }

        public Request GetRequest()
        {
            Request request = null;

            if (message.Payload.IsNullOrEmpty())
            {
                switch (message.MessageType)
                {
                    case Message.Type.Ping:
                        request = new PingRequest();
                        break;
                    case Message.Type.Create:
                        request = new CreateRequest { Rootfs = ContainerType.Console.ToString() }; // TODO: not Rootfs
                        break;
                    default:
                        throw new WardenException("Invalid message type '{0}' for message WITHOUT payload.", message.MessageType);
                }
            }
            else
            {
                using (var ms = new MemoryStream(message.Payload))
                {
                    switch (message.MessageType)
                    {
                        case Message.Type.Echo:
                            request = Serializer.Deserialize<EchoRequest>(ms);
                            break;
                        default:
                            throw new WardenException("Invalid message type '{0}' for message WITH payload.", message.MessageType);
                    }
                }
            }

            return request;
        }
    }
}
