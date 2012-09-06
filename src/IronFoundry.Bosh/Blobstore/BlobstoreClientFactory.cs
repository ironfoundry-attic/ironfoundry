namespace IronFoundry.Bosh.Blobstore
{
    using System;
    using IronFoundry.Bosh.Configuration;
    using IronFoundry.Bosh.Properties;
    using IronFoundry.Misc.Logging;

    public class BlobstoreClientFactory : IBlobstoreClientFactory
    {
        private readonly ILog log;
        private readonly IBoshConfig config;

        public BlobstoreClientFactory(ILog log, IBoshConfig config)
        {
            this.log = log;
            this.config = config;
        }

        public BlobstoreClient Create()
        {
            BlobstoreClient rv;

            var options = new BlobstoreOptions(config.BlobstoreEndpoint, config.BlobstoreUser, config.BlobstorePassword);

            switch (config.BlobstorePlugin)
            {
                case "simple" :
                    rv = new SimpleBlobstoreClient(options);
                    break;
                default :
                    string msg = String.Format(Resources.BlobstoreClientFactory_UnknownPlugin_Fmt, config.BlobstorePlugin);
                    @log.Error(msg);
                    throw new BlobstoreException(msg);
            }

            return rv;
        }
    }
}