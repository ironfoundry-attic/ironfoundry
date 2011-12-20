namespace IronFoundry.Dea.Services
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using IronFoundry.Dea.Logging;

    [ServiceBehavior(Namespace=Constants.FilesServiceNamespace)]
    public class FilesService : IFilesService
    {
        private readonly ILog log;

        public FilesService(ILog log)
        {
            this.log = log;
        }

        public Message GetFile()
        {
            throw new NotImplementedException();
        }
    }
}