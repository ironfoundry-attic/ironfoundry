namespace IronFoundry.Warden.Containers
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using NLog;

    public class ContainerManager : IContainerManager
    {
        private readonly ConcurrentDictionary<ContainerHandle, Container> containers =
            new ConcurrentDictionary<ContainerHandle, Container>();

        private readonly Logger log = LogManager.GetCurrentClassLogger();

        public IEnumerable<ContainerHandle> Handles
        {
            get { return containers.Keys; }
        }

        public void AddContainer(Container container)
        {
            if (!containers.TryAdd(container.Handle, container))
            {
                throw new WardenException("Could not add container '{0}' to collection!", container);
            }
        }

        public void DestroyContainer(ContainerHandle handle)
        {
            if (handle == null)
            {
                throw new ArgumentNullException("handle");
            }

            Container removed;
            if (containers.TryRemove(handle, out removed))
            {
                try
                {
                    removed.Destroy();
                }
                catch
                {
                    log.Error("Error destroying container! Re-adding to collection.");
                    containers[handle] = removed;
                    throw;
                }
            }
            else
            {
                throw new WardenException("Could not remove container '{0}' from collection!", handle);
            }
        }

        public void Dispose()
        {
            // TODO - serialize, clear collection
        }
    }
}
