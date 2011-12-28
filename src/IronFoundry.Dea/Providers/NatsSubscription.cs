namespace IronFoundry.Dea.Providers
{
    using System;

    public abstract class NatsSubscription : IEquatable<NatsSubscription>
    {
        private static object sync = new object();
        private static int staticSequence = 0;
        private int instanceSequence = 0;

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

        public NatsSubscription()
        {
            lock (sync)
            {
                ++staticSequence;
                instanceSequence = staticSequence;
            }
        }

        public int Sequence
        {
            get { return instanceSequence; }
        }

        protected abstract string Subscription { get; }

        public override string ToString()
        {
            return Subscription;
        }

        public override int GetHashCode()
        {
            return Subscription.GetHashCode();
        }

        public bool Equals(NatsSubscription other)
        {
            bool rv = false;

            if (null != other)
            {
                rv = this.GetHashCode() == other.GetHashCode();
            }

            return rv;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as NatsSubscription);
        }

        public class DeaInstanceStartSubscription : NatsSubscription
        {
            private readonly string subscription;

            public DeaInstanceStartSubscription(string argSubscription)
            {
                subscription = argSubscription;
            }

            protected override string Subscription
            {
                get { return subscription; }
            }
        }

        private class DeaStopSubscription : NatsSubscription
        {
            protected override string Subscription
            {
                get { return Subscriptions.deaStop; }
            }
        }

        private class DeaStatusSubscription : NatsSubscription
        {
            protected override string Subscription
            {
                get { return Subscriptions.deaStatus; }
            }
        }

        private class DropletStatusSubscription : NatsSubscription
        {
            protected override string Subscription
            {
                get { return Subscriptions.dropletStatus; }
            }
        }

        private class DeaDiscoverSubscription : NatsSubscription
        {
            protected override string Subscription
            {
                get { return Subscriptions.deaDiscover; }
            }
        }

        private class DeaFindDropletSubscription : NatsSubscription
        {
            protected override string Subscription
            {
                get { return Subscriptions.deaFindDroplet; }
            }
        }

        private class DeaUpdateSubscription : NatsSubscription
        {
            protected override string Subscription
            {
                get { return Subscriptions.deaUpdate; }
            }
        }

        private class RouterStartSubscription : NatsSubscription
        {
            protected override string Subscription
            {
                get { return Subscriptions.routerStart; }
            }
        }

        private class HealthManagerStartSubscription : NatsSubscription
        {
            protected override string Subscription
            {
                get { return Subscriptions.healthManagerStart; }
            }
        }

        private class VcapComponentDiscoverSubscription : NatsSubscription
        {
            protected override string Subscription
            {
                get { return Subscriptions.vcapComponentDiscover; }
            }
        }
    }
}