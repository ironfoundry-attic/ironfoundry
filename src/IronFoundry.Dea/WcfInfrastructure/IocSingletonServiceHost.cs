namespace IronFoundry.Dea.WcfInfrastructure
{
    using System;

    public class IocSingletonServiceHost : SingletonServiceHost
    {
        public IocSingletonServiceHost(Type serviceType) : base(serviceType) { }

        protected override void OnOpening()
        {
            IocServiceBehaviorUtil.AddIoCServiceBehaviorTo(this);
            base.OnOpening();
        }
    }
}