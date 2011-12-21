using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.ServiceModel.Security;
using System.ServiceModel.Web;
using IronFoundry.Dea.Config;

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
        private readonly IConfig config;

        public FilesService(ILog log, IConfig config)
        {
            this.log = log;
            this.config = config;
        }

        public Message GetFile()
        {
            var context = WebOperationContext.Current;

            var uriTemplate = context.IncomingRequest.UriTemplateMatch;   
            var splitPath = uriTemplate.RequestUri.PathAndQuery.Split('/').ToList();
            splitPath.Remove(uriTemplate.BaseUri.LocalPath.Trim('/'));

            var uri = context.IncomingRequest.UriTemplateMatch.RequestUri.Segments.ToList();
            uri.RemoveAt(0);
            string fullPath = uri.Aggregate(config.AppDir, (x, y) => Path.Combine(x, y));

            if (Path.GetDirectoryName(fullPath + "\\").Equals(Path.GetDirectoryName(config.AppDir + "\\"), StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityAccessDeniedException();

            Message returnMessage = null;

            if (File.Exists(fullPath))
            {
                try
                {
                    using (var fileStream = File.OpenRead(fullPath))
                    {
                        var memoryStream = new MemoryStream((int) Math.Max(fileStream.Length, 10485760));
                        fileStream.CopyTo(memoryStream, memoryStream.Capacity);
                        memoryStream.Seek(0, SeekOrigin.Begin);
                        returnMessage = context.CreateStreamResponse(memoryStream, "application/octet-stream");                            
                    }                    
                }
                catch (IOException exception)
                {
                    context.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                    context.OutgoingResponse.StatusDescription = exception.ToString();
                }
            }
            else if (Directory.Exists(fullPath))
            {
                var memoryStream = new MemoryStream();
                var outputStream = new StreamWriter(memoryStream);
                var dirInfo = new DirectoryInfo(fullPath);

                foreach (var info in dirInfo.GetFileSystemInfos().OrderBy(i => i is FileInfo).ToList())
                {
                    var left = info.Name;
                    var right = info is DirectoryInfo ? "-" : GetReadableForm((info as FileInfo).Length);
                    outputStream.WriteLine(left + new string(' ', Math.Max(8, 46 - left.Length - right.Length)) + right);
                }
             
                outputStream.Flush();
                memoryStream.Seek(0, SeekOrigin.Begin);
                returnMessage = context.CreateStreamResponse(memoryStream, "text/plain");
            }
            return returnMessage;
        }

        private static string GetReadableForm(long size)
        {
            string[] sizes = { "B", "K", "M", "G" };

            int order = 0;
            while (size >= 1024 && order + 1 < sizes.Length)
            {
                order++;
                size = size / 1024;
            }

            string result = string.Format(CultureInfo.InvariantCulture, "{0:0.##}{1}", size, sizes[order]);

            return result;
        }       
    }
}