namespace IronFoundry.Dea.WcfInfrastructure
{
    using System.Collections.Generic;
    using System.ServiceModel;

    public static class IocServiceBehaviorUtil
    {
        public static void AddNinjectServiceBehaviorTo(ServiceHost argServiceHost)
        {
            var behaviors = argServiceHost.Description.Behaviors;

            if (false == behaviors.IsNullOrEmpty())
            {
                var ninjectServiceBehavior = behaviors.Find<IocServiceBehavior>();
                if (null == ninjectServiceBehavior)
                {
                    ninjectServiceBehavior = new IocServiceBehavior();
                    behaviors.Add(ninjectServiceBehavior);
                }
            }
        }
    }
}