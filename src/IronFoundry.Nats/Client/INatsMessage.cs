namespace IronFoundry.Nats.Client
{
    public interface INatsMessage
    {
        string PublishSubject { get; }
        string ToJson();
        bool IsReceiveOnly { get; }
        bool CanPublishWithSubject(string subject);
    }
}