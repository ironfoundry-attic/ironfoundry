namespace IronFoundry.Dea.Agent
{
    using System;
    using System.Collections.Generic;
    using IronFoundry.Dea.Types;

    public interface IDropletManager
    {
        void Add(uint dropletID, Instance instance);
        void Add(uint dropletID, IEnumerable<Instance> instances);

        void ForAllInstances(Action<Instance> instanceAction);
        void ForAllInstances(Action<uint> dropletAction, Action<Instance> instanceAction);
        void ForAllInstances(uint dropletID, Action<Instance> instanceAction);

        void FromSnapshot(Snapshot snapshot);
        Snapshot GetSnapshot();

        void InstanceStopped(Instance instance);

        bool IsEmpty { get; }

        void SetProcessInformationFrom(IDictionary<string, IList<int>> iisWorkerProcesses);
    }
}
