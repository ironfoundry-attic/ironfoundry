namespace IronFoundry.Warden.Configuration
{
    using System.Configuration;

    public class WardenSection : ConfigurationSection
    {
        public const string SectionName = "warden-server";

        private const string ContainerBasePathPropName = "container-basepath";
        private const string TcpPortPropName = "tcp-port";

        [ConfigurationProperty(ContainerBasePathPropName, DefaultValue = "C:\\IronFoundry\\warden\\containers", IsRequired = false)]
        public string ContainerBasePath
        {
            get
            {
                return (string)this[ContainerBasePathPropName];
            }
            set
            {
                this[ContainerBasePathPropName] = value;
            }
        }

        [ConfigurationProperty(TcpPortPropName, DefaultValue = 4444, IsRequired = false)]
        public uint TcpPort
        {
            get
            {
                return (uint)this[TcpPortPropName];
            }
            set
            {
                this[TcpPortPropName] = value;
            }
        }
    }
}
