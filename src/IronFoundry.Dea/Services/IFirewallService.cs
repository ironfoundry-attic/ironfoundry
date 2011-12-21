namespace IronFoundry.Dea.Services
{
    using System;

    public interface IFirewallService
    {
        void Open(ushort port, string name);
        void Close(ushort port);
    }
}
