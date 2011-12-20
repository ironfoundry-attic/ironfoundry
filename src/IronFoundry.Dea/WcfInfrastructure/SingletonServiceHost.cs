namespace IronFoundry.Dea.WcfInfrastructure
{
    using System;
    using System.ServiceModel;

    public class SingletonServiceHost : ServiceHost
    {
        public SingletonServiceHost(Type serviceType) : base(serviceType) { }

        protected override void OnOpening()
        {
            var serviceBehavior = Description.Behaviors.Find<ServiceBehaviorAttribute>();
            if (null == serviceBehavior)
            {
                serviceBehavior = new ServiceBehaviorAttribute();
                Description.Behaviors.Add(serviceBehavior);
            }
            serviceBehavior.InstanceContextMode = InstanceContextMode.Single;

            base.OnOpening();
        }
    }
}