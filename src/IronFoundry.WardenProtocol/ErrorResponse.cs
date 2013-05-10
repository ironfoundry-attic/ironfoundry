namespace IronFoundry.WardenProtocol
{
    public partial class ErrorResponse : Response
    {
        public override Message.Type ResponseType
        {
            get { return WardenProtocol.Message.Type.Error; }
        }
    }
}
