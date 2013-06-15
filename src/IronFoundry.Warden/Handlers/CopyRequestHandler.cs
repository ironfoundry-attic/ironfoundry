namespace IronFoundry.Warden.Handlers
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Containers;
    using NLog;
    using Protocol;

    public abstract class CopyRequestHandler : ContainerRequestHandler
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger();

        private readonly ICopyRequest request;
        private readonly Response response;

        public CopyRequestHandler(IContainerManager containerManager, Request request, Response response)
            : base(containerManager, request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }
            this.request = (ICopyRequest)request;

            if (response == null)
            {
                throw new ArgumentNullException("response");
            }
            this.response = response;
        }

        public override Task<Response> HandleAsync()
        {
            log.Trace("SrcPath: '{0}' DstPath: '{1}'", request.SrcPath, request.DstPath);

            Container container = GetContainer();
            string sourcePath = container.ConvertToPathWithin(request.SrcPath);
            string destinationPath = container.ConvertToPathWithin(request.DstPath);

            var destinationAttrs = File.GetAttributes(destinationPath);
            if (destinationAttrs.HasFlag(FileAttributes.Directory))
            {
                var fileName = Path.GetFileName(sourcePath);
                File.Copy(sourcePath, Path.Combine(destinationPath, fileName), true);
            }
            else
            {
                File.Copy(sourcePath, destinationPath, true);
            }

            return Task.FromResult<Response>(response);
        }
    }
}
