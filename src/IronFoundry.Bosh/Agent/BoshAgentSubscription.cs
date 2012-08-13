namespace IronFoundry.Bosh.Agent
{
    using System;
    using IronFoundry.Nats.Client;

    public class BoshAgentSubscription : INatsSubscription
    {
        // TODO this should be part of NATS client as in ruby code
        private static readonly object sync = new object();
        private static int staticSubscriptionID = 0;

        private readonly int instanceSubscriptionID = 0;
        private readonly string subscription;

        public BoshAgentSubscription(string agentID)
        {
            lock (sync)
            {
                ++staticSubscriptionID;
                instanceSubscriptionID = staticSubscriptionID;
            }
            subscription = String.Format("agent.{0}", agentID);
        }

        public int SubscriptionID
        {
            get { return instanceSubscriptionID; }
        }

        public override string ToString()
        {
            return subscription;
        }

        public override int GetHashCode()
        {
            return subscription.GetHashCode();
        }

        public bool Equals(INatsSubscription other)
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
            return this.Equals(obj as INatsSubscription);
        }
    }
}