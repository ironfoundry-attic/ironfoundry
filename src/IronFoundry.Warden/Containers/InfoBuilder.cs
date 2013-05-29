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
            Container c = containerManager.GetContainer(handle);
            var hostIp = Utility.GetLocalIPAddress().ToString();
            return new InfoResponse(hostIp, hostIp, c.Path);
        }
    }
}
