using System;
using System.Collections.Generic;
using System.Diagnostics;
using IronFoundry.Dea.Types;

namespace IronFoundry.Dea.Agent
{
    public class DropletManager : IDropletManager
    {
        private readonly IDictionary<Guid, IDictionary<Guid, Instance>> droplets = new Dictionary<Guid, IDictionary<Guid, Instance>>();

        public void Add(Guid dropletID, IEnumerable<Instance> instances)
        {
            lock (droplets)
            {
                foreach (Instance instance in instances)
                {
                    Add(dropletID, instance);
                }
            }
        }

        public void Add(Guid dropletID, Instance instance)
        {
            lock (droplets)
            {
                IDictionary<Guid, Instance> instances;
                if (droplets.TryGetValue(dropletID, out instances))
                {
                    instances.Add(instance.InstanceID, instance);
                }
                else
                {
                    instances = new Dictionary<Guid, Instance>
                                {
                                    {instance.InstanceID, instance}
                                };
                    droplets.Add(dropletID, instances);
                }
            }
        }

        public bool IsEmpty
        {
            get
            {
                lock (droplets)
                {
                    return droplets.IsNullOrEmpty();
                }
            }
        }

        public void ForAllInstances(Action<Instance> instanceAction)
        {
            ForAllInstances(null, instanceAction);
        }

        public void ForAllInstances(Guid dropletID, Action<Instance> instanceAction)
        {
            lock (droplets)
            {
                if (IsEmpty)
                {
                    return;
                }
                if (droplets.ContainsKey(dropletID))
                {
                    IDictionary<Guid, Instance> instanceDict = droplets[dropletID];
                    if (null != instanceDict)
                    {
                        IEnumerable<Instance> instances = instanceDict.Values.ToListOrNull(); // NB: copies list
                        if (null != instances)
                        {
                            foreach (Instance instance in instances)
                            {
                                instanceAction(instance);
                            }
                        }
                    }
                }
            }
        }

        public void FromSnapshot(Snapshot snapshot)
        {
            lock (droplets)
            {
                foreach (DropletEntry dropletEntry in snapshot.Entries)
                {
                    foreach (InstanceEntry instanceEntry in dropletEntry.Instances)
                    {
                        Add(dropletEntry.DropletID, instanceEntry.Instance);
                    }
                }
            }
        }

        public Snapshot GetSnapshot()
        {
            var dropletEntries = new List<DropletEntry>();

            lock (droplets)
            {
                if (false == IsEmpty)
                {
                    foreach (var droplet in droplets)
                    {
                        var instanceEntries = new List<InstanceEntry>();

                        foreach (var instance in droplet.Value)
                        {
                            var instanceEntry = new InstanceEntry
                                                {
                                                    InstanceID = instance.Key,
                                                    Instance = instance.Value
                                                };
                            instanceEntries.Add(instanceEntry);
                        }

                        var d = new DropletEntry
                                {
                                    DropletID = droplet.Key,
                                    Instances = instanceEntries.ToArray()
                                };

                        dropletEntries.Add(d);
                    }
                }
            }

            return new Snapshot
                   {
                       Entries = dropletEntries.ToArrayOrNull()
                   };
        }

        public void InstanceStopped(Instance instance)
        {
            Guid dropletID = instance.DropletID;

            lock (droplets)
            {
                if (droplets.ContainsKey(dropletID))
                {
                    IDictionary<Guid, Instance> instanceDict = droplets[dropletID];
                    if (null != instanceDict)
                    {
                        instanceDict.Remove(instance.InstanceID);
                        if (instanceDict.IsNullOrEmpty())
                        {
                            droplets.Remove(dropletID);
                        }
                    }
                }
            }
        }

        public void SetProcessInformationFrom(IDictionary<string, IList<int>> iisWorkerProcessData)
        {
            if (iisWorkerProcessData.IsNullOrEmpty())
            {
                return;
            }

            ForAllInstances(inst =>
                            {
                                string appPoolName = inst.Staged; // TODO: we have to "know" that this is the app pool name
                                IList<int> tmp;
                                if (iisWorkerProcessData.TryGetValue(appPoolName, out tmp))
                                {
                                    foreach (int pid in tmp)
                                    {
                                        try
                                        {
                                            inst.AddWorkerProcess(Process.GetProcessById(pid));
                                        }
                                        catch
                                        {
                                        }
                                    }
                                }
                            });
        }

        public void ForAllInstances(Action<Guid> dropletAction, Action<Instance> instanceAction)
        {
            lock (droplets)
            {
                if (IsEmpty)
                {
                    return;
                }
                var dropletList = new List<KeyValuePair<Guid, IDictionary<Guid, Instance>>>(droplets);
                foreach (KeyValuePair<Guid, IDictionary<Guid, Instance>> kvp in dropletList)
                {
                    if (droplets.ContainsKey(kvp.Key))
                    {
                        Guid dropletID = kvp.Key;
                        if (null != dropletAction)
                        {
                            dropletAction(dropletID);
                        }
                        var instances = new List<Instance>(kvp.Value.Values);
                        foreach (Instance instance in instances)
                        {
                            instanceAction(instance);
                        }
                    }
                }
            }
        }
    }
}