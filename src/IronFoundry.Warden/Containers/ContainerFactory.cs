namespace IronFoundry.Warden.Containers
{
    using System;

    public class ContainerFactory
    {
        private readonly ContainerType containerType;

        public ContainerFactory(string containerType)
        {
            if (containerType.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException("containerType");
            }

            ContainerType ct;
            if (Enum.TryParse(containerType, out ct))
            {
                this.containerType = ct;
            }
            else
            {
                throw new ArgumentException("Invalid container type. Must be 'IIS' or 'Console'");
            }
        }

        public ContainerFactory(ContainerType containerType)
        {
            this.containerType = containerType;
        }

        public Container CreateContainer()
        {
            throw new NotImplementedException();
        }
    }
}
