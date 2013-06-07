namespace IronFoundry.Warden.Protocol
{
    public partial class StreamResponse : Response
    {
        public override Message.Type ResponseType
        {
            get { return Message.Type.Stream; }
        }
    }
}
