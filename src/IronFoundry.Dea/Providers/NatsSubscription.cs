using System;
namespace IronFoundry.Dea.Providers
{
    public abstract class NatsSubscription
    {
        private static class Subscriptions
        {
            public const string deaInstanceStart      = "dea.{0:N}.start"; // NB: argument is VCAP GUID
            public const string deaStop               = "dea.stop";
            public const string deaStatus             = "dea.status";
            public const string dropletStatus         = "droplet.status";
            public const string deaDiscover           = "dea.discover";
            public const string deaFindDroplet        = "dea.find.droplet";
            public const string deaUpdate             = "dea.update";
            public const string routerStart           = "router.start";
            public const string healthManagerStart    = "healthmanager.start";
            public const string vcapComponentDiscover = "vcap.component.discover";
        }

        public static readonly NatsSubscription DeaStop               = new DeaStopSubscription();
        public static readonly NatsSubscription DeaStatus             = new DeaStatusSubscription();
        public static readonly NatsSubscription DropletStatus         = new DropletStatusSubscription();
        public static readonly NatsSubscription DeaDiscover           = new DeaDiscoverSubscription();
        public static readonly NatsSubscription DeaFindDroplet        = new DeaFindDropletSubscription();
        public static readonly NatsSubscription DeaUpdate             = new DeaUpdateSubscription();
        public static readonly NatsSubscription RouterStart           = new RouterStartSubscription();
        public static readonly NatsSubscription HealthManagerStart    = new HealthManagerStartSubscription();
        public static readonly NatsSubscription VcapComponentDiscover = new VcapComponentDiscoverSubscription();

        public static NatsSubscription GetDeaInstanceStartFor(Guid argUuid)
        {
            return new DeaInstanceStartSubscription(String.Format(Subscriptions.deaInstanceStart, argUuid));
        }

        public abstract string Subscription { get; }

        public override string ToString()
        {
            return Subscription;
        }

        public override int GetHashCode()
        {
            return Subscription.GetHashCode();
        }

        public class DeaInstanceStartSubscription : NatsSubscription
        {
            private readonly string subscription;

            public DeaInstanceStartSubscription(string argSubscription)
            {
                subscription = argSubscription;
            }

            public override string Subscription
            {
                get { return subscription; }
            }
        }

        private class DeaStopSubscription : NatsSubscription
        {
            public override string Subscription
            {
                get { return Subscriptions.deaStop; }
            }
        }

        private class DeaStatusSubscription : NatsSubscription
        {
            public override string Subscription
            {
                get { return Subscriptions.deaStatus; }
            }
        }

        private class DropletStatusSubscription : NatsSubscription
        {
            public override string Subscription
            {
                get { return Subscriptions.dropletStatus; }
            }
        }

        private class DeaDiscoverSubscription : NatsSubscription
        {
            public override string Subscription
            {
                get { return Subscriptions.deaDiscover; }
            }
        }

        private class DeaFindDropletSubscription : NatsSubscription
        {
            public override string Subscription
            {
                get { return Subscriptions.deaFindDroplet; }
            }
        }

        private class DeaUpdateSubscription : NatsSubscription
        {
            public override string Subscription
            {
                get { return Subscriptions.deaUpdate; }
            }
        }

        private class RouterStartSubscription : NatsSubscription
        {
            public override string Subscription
            {
                get { return Subscriptions.routerStart; }
            }
        }

        private class HealthManagerStartSubscription : NatsSubscription
        {
            public override string Subscription
            {
                get { return Subscriptions.healthManagerStart; }
            }
        }

        private class VcapComponentDiscoverSubscription : NatsSubscription
        {
            public override string Subscription
            {
                get { return Subscriptions.vcapComponentDiscover; }
            }
        }
    }
}