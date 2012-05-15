namespace IronFoundry.Dea.Agent
{
    using System;
    using System.Collections.Generic;
    using IronFoundry.Dea.Types;

    public interface IDropletManager
    {
        void Add(uint argDropletID, Instance argInstance);
        void Add(uint argDropletID, IEnumerable<Instance> argInstances);

        void ForAllInstances(Action<Instance> argInstanceAction);
        void ForAllInstances(Action<uint> argDropletAction, Action<Instance> argInstanceAction);
        void ForAllInstances(uint argDropletID, Action<Instance> argInstanceAction);

        void FromSnapshot(Snapshot argSnapshot);
        Snapshot GetSnapshot();

        void InstanceStopped(Instance argInstance);

        bool IsEmpty { get; }

        void SetProcessInformationFrom(IDictionary<string, int> iisWorkerProcesses);
    }
}