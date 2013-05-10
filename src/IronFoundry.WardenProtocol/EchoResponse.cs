namespace IronFoundry.WardenProtocol
{
    public partial class EchoResponse : Response
    {
        public override Message.Type ResponseType
        {
            get { return WardenProtocol.Message.Type.Echo; }
        }
    }
}
