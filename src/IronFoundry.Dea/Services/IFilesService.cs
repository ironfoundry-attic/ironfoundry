namespace IronFoundry.Dea.Services
{
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Web;

    [ServiceContract(Namespace=Constants.FilesServiceNamespace)]
    public interface IFilesService
    {
        [WebGet(UriTemplate = @"/*")]
        Message GetFile();
    }
}
