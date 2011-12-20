namespace IronFoundry.Dea
{
    using System;
    using System.Collections.Generic;
    using IronFoundry.Dea.Types;

    public class DropletManager
    {
        // TODO check this out: http://geekswithblogs.net/BlackRabbitCoder/archive/2011/02/17/c.net-little-wonders-the-concurrentdictionary.aspx
        private readonly IDictionary<uint, IDictionary<Guid, Instance>> droplets = new Dictionary<uint, IDictionary<Guid, Instance>>();

        public void Add(uint argDropletID, IEnumerable<Instance> argInstances)
        {
            lock (droplets)
            {
                foreach (Instance instance in argInstances)
                {
                    Add(argDropletID, instance);
                }
            }
        }

        public void Add(uint argDropletID, Instance argInstance)
        {
            lock (droplets)
            {
                IDictionary<Guid, Instance> instances;
                if (droplets.TryGetValue(argDropletID, out instances))
                {
                    instances.Add(argInstance.InstanceID, argInstance);
                }
                else
                {
                    instances = new Dictionary<Guid, Instance>
                    {
                        { argInstance.InstanceID, argInstance }
                    };
                    droplets.Add(argDropletID, instances);
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

        public void ForAllInstances(Action<Instance> argInstanceAction)
        {
            ForAllInstances(null, argInstanceAction);
        }

        public void ForAllInstances(uint argDropletID, Action<Instance> argInstanceAction)
        {
            lock (droplets)
            {
                if (this.IsEmpty)
                {
                    return;
                }

                IDictionary<Guid, Instance> instanceDict = droplets[argDropletID];
                if (null != instanceDict)
                {
                    IEnumerable<Instance> instances = instanceDict.Values.ToListOrNull();
                    foreach (Instance instance in instances)
                    {
                        argInstanceAction(instance);
                    }
                }
            }
        }

        public void ForAllInstances(Action<uint> argDropletAction, Action<Instance> argInstanceAction)
        {
            lock (droplets)
            {
                if (this.IsEmpty)
                {
                    return;
                }

                foreach (KeyValuePair<uint, IDictionary<Guid, Instance>> kvp in droplets)
                {
                    uint dropletID = kvp.Key;
                    IDictionary<Guid, Instance> instanceDict = kvp.Value;

                    if (null != argDropletAction)
                    {
                        argDropletAction(dropletID);
                    }

                    foreach (Instance instance in instanceDict.Values)
                    {
                        argInstanceAction(instance);
                    }
                }
            }
        }

        public void FromSnapshot(Snapshot argSnapshot)
        {
            lock (droplets)
            {
                foreach (DropletEntry dropletEntry in argSnapshot.Entries)
                {
                    foreach (InstanceEntry instanceEntry in dropletEntry.Instances)
                    {
                        this.Add(dropletEntry.DropletID, instanceEntry.Instance);
                    }
                }
            }
        }

        public Snapshot GetSnapshot()
        {
            var dropletEntries = new List<DropletEntry>();

            lock (droplets)
            {
                if (false == this.IsEmpty)
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

            return new Snapshot()
            {
                Entries = dropletEntries.ToArrayOrNull()
            };
        }

        public void InstanceStopped(uint argDropletID, Instance argInstance)
        {
            lock (droplets)
            {
                IDictionary<Guid, Instance> instanceDict = droplets[argDropletID];
                if (null != instanceDict)
                {
                    instanceDict.Remove(argInstance.InstanceID);
                    if (instanceDict.IsNullOrEmpty())
                    {
                        droplets.Remove(argDropletID);
                    }
                }
            }
        }
    }
}