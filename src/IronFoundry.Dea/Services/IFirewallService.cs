namespace IronFoundry.Dea.Services
{
    public interface IFirewallService
    {
        void Open(ushort port, string name);
        void Close(string name);
    }
}