namespace IronFoundry.Nats.Client
{
    using System;

    public interface INatsSubscription : IEquatable<INatsSubscription>
    {
        int SubscriptionID { get; } // TODO this should be part of the NATS client!
        string ToString();
    }
}