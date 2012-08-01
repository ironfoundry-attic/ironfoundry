namespace IronFoundry.Nats.Configuration
{
    using System;
    using System.Configuration;

    public class NatsSection : ConfigurationSection
    {
        public const string SectionName = "nats";

        [ConfigurationProperty("host", DefaultValue = "api.vcap.me", IsRequired = true)]
        public string Host
        {
            get
            {
                return (string)this["host"];
            }
            set
            {
                this["host"] = value;
            }
        }

        [ConfigurationProperty("port", DefaultValue = "4222", IsRequired = true)]
        public ushort Port
        {
            get
            {
                return Convert.ToUInt16(this["port"]);
            }
            set
            {
                this["port"] = value;
            }
        }

        [ConfigurationProperty("user", IsRequired = false)]
        public string User
        {
            get
            {
                return (string)this["user"];
            }
            set
            {
                this["user"] = value;
            }
        }

        [ConfigurationProperty("password", IsRequired = false)]
        public string Password
        {
            get
            {
                return (string)this["password"];
            }
            set
            {
                this["password"] = value;
            }
        }
    }
}