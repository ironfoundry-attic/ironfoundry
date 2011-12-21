namespace IronFoundry.Dea.WcfInfrastructure
{
    using System;

    public class IocServiceHost : PerCallServiceHost
    {
        public IocServiceHost(Type serviceType) : base(serviceType) { }

        public IocServiceHost(Type serviceType, Uri baseAddress) : base(serviceType, baseAddress) { }

        protected override void OnOpening()
        {
            IocServiceBehaviorUtil.AddIoCServiceBehaviorTo(this);
            base.OnOpening();
        }
    }
}