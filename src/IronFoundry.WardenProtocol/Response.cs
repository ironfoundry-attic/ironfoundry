namespace IronFoundry.WardenProtocol
{
    public abstract class Response
    {
        public abstract Message.Type ResponseType { get; }
    }
}
