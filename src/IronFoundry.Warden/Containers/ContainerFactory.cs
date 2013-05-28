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
            Container created = null;
            
            switch (containerType)
            {
                case ContainerType.Console :
                    created = new ConsoleContainer();
                    break;
                case ContainerType.IIS :
                    created = new IISContainer();
                    break;
                default:
                    throw new WardenException("Unknown container type: '{0}'", containerType);
            }
            
            return created;
        }
    }
}
