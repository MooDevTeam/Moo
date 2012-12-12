using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
namespace Moo.API
{
    public class CustomBehavior : BehaviorExtensionElement, IServiceBehavior
    {
        public override Type BehaviorType
        {
            get { return GetType(); }
        }

        protected override object CreateBehavior()
        {
            return this;
        }

        public void AddBindingParameters(ServiceDescription serviceDescription, System.ServiceModel.ServiceHostBase serviceHostBase, System.Collections.ObjectModel.Collection<ServiceEndpoint> endpoints, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, System.ServiceModel.ServiceHostBase serviceHostBase)
        {
            IErrorHandler handler = new ErrorHandler();
            foreach (ChannelDispatcher chDisp in serviceHostBase.ChannelDispatchers)
            {
                if (chDisp.BindingName.Contains("WebHttpBinding"))
                {
                    chDisp.ErrorHandlers.Add(handler);
                    foreach (EndpointDispatcher epDisp in chDisp.Endpoints)
                    {
                        epDisp.DispatchRuntime.MessageInspectors.Add(new Authenticator());
                    }
                }
            }

            foreach (ServiceEndpoint endpoint in serviceDescription.Endpoints)
            {
                if (endpoint.Binding.GetType() == typeof(WebHttpBinding))
                {
                    foreach (OperationDescription operation in endpoint.Contract.Operations)
                    {
                        operation.Behaviors.Add(new CustomMessageFormatter.Behavior());
                    }
                }
            }
        }

        public void Validate(ServiceDescription serviceDescription, System.ServiceModel.ServiceHostBase serviceHostBase)
        {
        }
    }
}