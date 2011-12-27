namespace IronFoundry.Dea.Providers
{
    using System.Collections.Generic;
    using IronFoundry.Dea.Types;

    public interface IVarzProvider
    {
        string GetVarzJson();

        VcapComponentDiscover Discover { set; }

        ulong MaxMemoryMB { set; }
        ulong MemoryReservedMB { set; }
        ulong MemoryUsedMB { set; }
        uint MaxClients { set; }
        string State { set; }

        IEnumerable<string> RunningAppsJson { set; }
        IDictionary<string, Metric> RuntimeMetrics { set; }
        IDictionary<string, Metric> FrameworkMetrics { set; }
    }
}