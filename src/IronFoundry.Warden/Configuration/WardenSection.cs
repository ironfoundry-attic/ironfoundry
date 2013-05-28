namespace IronFoundry.Warden.Configuration
{
    using System.Configuration;

    public class WardenSection : ConfigurationSection
    {
        public const string SectionName = "warden-server";

        [ConfigurationProperty("container-basepath", DefaultValue = "C:\\IronFoundry\\warden\\containers", IsRequired = false)]
        public string ContainerBasePath
        {
            get
            {
                return (string)this["container-basepath"];
            }
            set
            {
                this["container-basepath"] = value;
            }
        }
    }
}
