namespace IronFoundry.Dea.WcfInfrastructure
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Web;

    public class IocSingletonWebServiceHost : WebServiceHost
    {
        public IocSingletonWebServiceHost(Type serviceType) : base(serviceType) { }

        public IocSingletonWebServiceHost(Type serviceType, Uri baseAddress)
            : base(serviceType, baseAddress) { }

        protected override void OnOpening()
        {
            IocServiceBehaviorUtil.AddIoCServiceBehaviorTo(this);

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