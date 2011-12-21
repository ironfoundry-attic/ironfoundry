namespace IronFoundry.Dea.WcfInfrastructure
{
    using System;
    using System.ServiceModel;

    public abstract class PerCallServiceHost : ServiceHost
    {
        public PerCallServiceHost(Type serviceType) : base(serviceType) { }

        public PerCallServiceHost(Type serviceType, Uri baseAddress) : base(serviceType, baseAddress) { }

        protected override void OnOpening()
        {
            var serviceBehavior = Description.Behaviors.Find<ServiceBehaviorAttribute>();
            if (null == serviceBehavior)
            {
                serviceBehavior = new ServiceBehaviorAttribute();
                Description.Behaviors.Add(serviceBehavior);
            }
            serviceBehavior.InstanceContextMode = InstanceContextMode.PerCall;

            base.OnOpening();
        }
    }
}