namespace IronFoundry.Dea.Services
{
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Web;

    [ServiceContract(Namespace=Constants.MonitoringServiceNamespace)]
    public interface IMonitoringService
    {
        [WebGet(UriTemplate = "/healthz")]
        Message GetHealthz();

        [WebGet(UriTemplate = "/varz")]
        Message GetVarz();
    }
}