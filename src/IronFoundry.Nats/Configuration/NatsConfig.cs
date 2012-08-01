namespace IronFoundry.Nats.Configuration
{
    using System.Configuration;

    public class NatsConfig : INatsConfig
    {
        private readonly NatsSection natsSection;

        public NatsConfig()
        {
            this.natsSection = (NatsSection)ConfigurationManager.GetSection(NatsSection.SectionName);
        }

        public string Host
        {
            get { return natsSection.Host; }
        }

        public ushort Port
        {
            get { return natsSection.Port; }
        }

        public string User
        {
            get { return natsSection.User; }
        }

        public string Password
        {
            get { return natsSection.Password; }
        }
    }
}