namespace IronFoundry.Dea.WcfInfrastructure
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;
    using StructureMap;

    public class IocInstanceProvider : IInstanceProvider
    {
        private readonly Type _serviceType;

        public IocInstanceProvider(Type serviceType)
        {
            _serviceType = serviceType;
        }

        public object GetInstance(InstanceContext instanceContext)
        {
            return GetInstance(instanceContext, null);
        }

        public object GetInstance(InstanceContext instanceContext, Message message)
        {
            return ObjectFactory.GetInstance(_serviceType);
        }

        public void ReleaseInstance(InstanceContext instanceContext, object instance)
        {
            var disposable = instance as IDisposable;
            if (null != disposable)
            {
                disposable.Dispose();
            }
        }
    }
}