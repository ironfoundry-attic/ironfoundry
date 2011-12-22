namespace IronFoundry.Dea.Services
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using IronFoundry.Dea.Logging;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.ServiceModel.Security;
    using IronFoundry.Dea.Config;
    using IronFoundry.Dea.Providers;

    public class FilesService : IFilesService
    {
        private readonly ILog log;
        private readonly IConfig config;
        private readonly IWebOperationContextProvider webContext;

        public FilesService(ILog log, IConfig config, IWebOperationContextProvider webContext)
        {
            this.log = log;
            this.config = config;
            this.webContext = webContext;
        }

        public Message GetFile()
        {
            Message message = null;

            var uriSegments = webContext.GetRequestUri().Segments.ToList().Select(u => u.TrimEnd('/')).ToList();
            uriSegments.RemoveRange(0, 2);                        
            var path = uriSegments.Aggregate(config.AppDir, Path.Combine);

            if (config.AppDir.Equals(path,StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityAccessDeniedException();                

            try
            {
                if (File.Exists(path))
                {
                    using (var fileStream = File.OpenRead(path))
                    {
                        var memoryStream = new MemoryStream((int)Math.Max(fileStream.Length, 10485760));
                        fileStream.CopyTo(memoryStream, memoryStream.Capacity);
                        memoryStream.Seek(0, SeekOrigin.Begin);
                        message = webContext.CreateStreamResponse(memoryStream, "application/octet-stream");
                    }
                }

                else if (Directory.Exists(path))
                {
                    var memoryStream = new MemoryStream();
                    var outputStream = new StreamWriter(memoryStream);

                    var fileSystemInfos = new DirectoryInfo(path).GetFileSystemInfos().OrderBy(i => i is FileInfo).ToList();

                    fileSystemInfos.ForEach(info =>
                    {
                        var left = info.Name;
                        var right = info is DirectoryInfo ? "-" : Utility.GetFileSizeString((info as FileInfo).Length);
                        outputStream.WriteLine(left + new string(' ', Math.Max(8, 46 - left.Length - right.Length)) + right);
                    });

                    outputStream.Flush();
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    message = webContext.CreateStreamResponse(memoryStream, "text/plain");
                }
            }
            catch (Exception ex)
            {
                webContext.SetOutgoingResponse(HttpStatusCode.InternalServerError, ex.ToString());
            }
            return message;
        }
    }
}