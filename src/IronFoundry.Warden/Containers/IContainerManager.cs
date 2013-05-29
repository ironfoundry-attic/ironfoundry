namespace IronFoundry.Warden.Containers
{
    using System;
    using System.Collections.Generic;

    public interface IContainerManager : IDisposable
    {
        void DestroyContainer(ContainerHandle handle);
        void AddContainer(Container container);
        IEnumerable<ContainerHandle> Handles { get; }
        Container GetContainer(string handle);
    }
}
