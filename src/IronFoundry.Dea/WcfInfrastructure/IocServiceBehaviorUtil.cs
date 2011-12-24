namespace IronFoundry.Dea.WcfInfrastructure
{
    using System.Collections.Generic;
    using System.ServiceModel;

    public static class IocServiceBehaviorUtil
    {
        public static void AddIoCServiceBehaviorTo(ServiceHost argServiceHost)
        {
            var behaviors = argServiceHost.Description.Behaviors;

            if (false == behaviors.IsNullOrEmpty())
            {
                var iocServiceBehavior = behaviors.Find<IocServiceBehavior>();
                if (null == iocServiceBehavior)
                {
                    iocServiceBehavior = new IocServiceBehavior();
                    behaviors.Add(iocServiceBehavior);
                }
            }
        }
    }
}