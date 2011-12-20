namespace IronFoundry.Dea.WcfInfrastructure
{
    using System;
    using System.ServiceModel;

    public class PerCallServiceHost : ServiceHost
    {
        public PerCallServiceHost(Type serviceType) : base(serviceType) { }

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