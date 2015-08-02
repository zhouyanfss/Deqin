using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;
using ZD.Service.DAL.Domain;

namespace ZD.Service.DAL.Domain.Common
{
    public class MyCallContextInitializer : ICallContextInitializer
    {
        public void AfterInvoke(object correlationState)
        {
            RepositoryFactory.CloseCurrentSession();
            //Utils.Logger.Info("invoked end.");
        }

        public object BeforeInvoke(InstanceContext instanceContext,
                                   IClientChannel channel, Message message)
        {
            return null;
        }
    }

    public class MyServiceBehaviorAttribute : Attribute, IServiceBehavior
    {
        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters) { }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            foreach (ChannelDispatcher dispatcher in serviceHostBase.ChannelDispatchers)
            {
                foreach (EndpointDispatcher endpoint in dispatcher.Endpoints)
                {
                    foreach (DispatchOperation operation in endpoint.DispatchRuntime.Operations)
                    {
                        operation.CallContextInitializers.Add(new MyCallContextInitializer());
                    }
                }
            }
        }

        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        { }
    }

}
