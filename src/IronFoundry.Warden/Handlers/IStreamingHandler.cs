namespace IronFoundry.Warden.Handlers
{
    using System.Threading;
    using IronFoundry.Warden.Protocol;

    public interface IStreamingHandler
    {
        StreamResponse Handle(MessageWriter messageWriter, CancellationToken cancellationToken);
    }
}
