namespace IronFoundry.Warden.Handlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using IronFoundry.Warden.Protocol;

    public interface IStreamingHandler
    {
        Task<StreamResponse> HandleAsync(MessageWriter messageWriter, CancellationToken cancellationToken);
    }
}
