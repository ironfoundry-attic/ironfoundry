namespace IronFoundry.Warden.Containers
{
    using System;
    using IronFoundry.Misc;
    using IronFoundry.Warden.Protocol;

    public class InfoBuilder
    {
        private readonly IContainerManager containerManager;

        public InfoBuilder(IContainerManager containerManager)
        {
            if (containerManager == null)
            {
                throw new ArgumentNullException("containerManager");
            }
            this.containerManager = containerManager;
        }

        public InfoResponse GetInfoResponseFor(string handle)
        {
            if (handle.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException("handle");
            }
            Container c = containerManager.GetContainer(handle);
            return GetInfoResponseFor(c);
        }

        private InfoResponse GetInfoResponseFor(Container container)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }
            var hostIp = Utility.GetLocalIPAddress().ToString();
            return new InfoResponse(hostIp, hostIp, container.ContainerPath);
        }
    }
}
