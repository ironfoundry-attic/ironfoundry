namespace IronFoundry.Dea.Config
{
    using System;
    using System.Configuration;

    public class DeaSection : ConfigurationSection
    {
        public const string SectionName = "ironFoundryDea";

        [ConfigurationProperty("natsHost", DefaultValue = "api.vcap.me", IsRequired = true)]
        public string NatsHost
        {
            get
            {
                return (string)this["natsHost"];
            }
            set
            {
                this["natsHost"] = value;
            }
        }

        [ConfigurationProperty("natsPort", DefaultValue = "4222", IsRequired = true)]
        public ushort NatsPort
        {
            get
            {
                return Convert.ToUInt16(this["natsPort"]);
            }
            set
            {
                this["natsPort"] = value;
            }
        }

        [ConfigurationProperty("localRoute", DefaultValue = "127.0.0.1", IsRequired = false)]
        public string LocalRoute
        {
            get
            {
                return (string)this["localRoute"];
            }
            set
            {
                this["localRoute"] = value;
            }
        }


        [ConfigurationProperty("appDir", DefaultValue = @"C:\IronFoundry\apps", IsRequired = false)]
        public string AppDir
        {
            get
            {
                return (string)this["appDir"];
            }
            set
            {
                this["appDir"] = value;
            }
        }

        [ConfigurationProperty("stagingDir", DefaultValue = @"C:\IronFoundry\staging", IsRequired = false)]
        public string StagingDir
        {
            get
            {
                return (string)this["stagingDir"];
            }
            set
            {
                this["stagingDir"] = value;
            }
        }

        [ConfigurationProperty("dropletDir", DefaultValue = @"C:\IronFoundry\droplet", IsRequired = false)]
        public string DropletDir
        {
            get
            {
                return (string)this["dropletDir"];
            }
            set
            {
                this["dropletDir"] = value;
            }
        }

        [ConfigurationProperty("disableDirCleanup", DefaultValue = false, IsRequired = false)]
        public bool DisableDirCleanup
        {
            get
            {
                return Convert.ToBoolean(this["disableDirCleanup"]);
            }
            set
            {
                this["disableDirCleanup"] = value;
            }
        }

        [ConfigurationProperty("filesServicePort", DefaultValue = "12345", IsRequired = false)]
        public ushort FilesServicePort
        {
            get
            {
                return Convert.ToUInt16(this["filesServicePort"]);
            }
            set
            {
                this["filesServicePort"] = value;
            }
        }
    }
}