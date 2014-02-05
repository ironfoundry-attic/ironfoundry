namespace IronFoundry.Dea.Agent
{
    using System;
    using System.Collections.Generic;
    using IronFoundry.Dea.Types;

    public interface IDropletManager
    {
        void Add(Guid dropletID, Instance instance);
        void Add(Guid dropletID, IEnumerable<Instance> instances);

        void ForAllInstances(Action<Instance> instanceAction);
        void ForAllInstances(Action<Guid> dropletAction, Action<Instance> instanceAction);
        void ForAllInstances(Guid dropletID, Action<Instance> instanceAction);

        void FromSnapshot(Snapshot snapshot);
        Snapshot GetSnapshot();

        void InstanceStopped(Instance instance);

        bool IsEmpty { get; }

        void SetProcessInformationFrom(IDictionary<string, IList<int>> iisWorkerProcesses);
    }
}
