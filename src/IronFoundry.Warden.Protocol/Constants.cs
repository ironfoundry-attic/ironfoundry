namespace IronFoundry.Warden.Protocol
{
    public static class Constants
    {
        public const byte CR = 0x0d;
        public const byte LF = 0x0a;
        public static readonly byte[] CRLF = new[] { CR, LF };
    }
}
