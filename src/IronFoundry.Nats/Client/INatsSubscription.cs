namespace IronFoundry.Nats.Client
{
    using System;

    public interface INatsSubscription : IEquatable<INatsSubscription>
    {
        int SubscriptionID { get; }
        string ToString();
    }
}