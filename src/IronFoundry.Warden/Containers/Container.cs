namespace IronFoundry.Warden.Containers
{
    public abstract class Container
    {
        private readonly ContainerHandle handle;

        public Container(string handle)
        {
            this.handle = new ContainerHandle(handle);
        }

        public Container()
        {
            this.handle = new ContainerHandle();
        }

        public string Handle
        {
            get { return handle; }
        }
    }
}
