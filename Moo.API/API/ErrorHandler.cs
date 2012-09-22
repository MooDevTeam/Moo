using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Security;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;

namespace Moo.API.API
{
    public class ErrorHandler : IErrorHandler
    {
        public bool HandleError(Exception error)
        {
            return true;
        }

        public void ProvideFault(Exception error, MessageVersion version, ref Message fault)
        {
            if (!(error is WebFaultException))
            {
                fault = Message.CreateMessage(version, "", new CustomFault
                {
                    Type = error.GetType().Name,
                    Message = error.Message
                }, new DataContractJsonSerializer(typeof(CustomFault)));
                fault.Properties.Add(WebBodyFormatMessageProperty.Name, new WebBodyFormatMessageProperty(WebContentFormat.Json));

                fault.Properties.Add(HttpResponseMessageProperty.Name, new HttpResponseMessageProperty()
                {
                    StatusCode = HttpStatusCode.InternalServerError
                });
            }
        }
    }

    [DataContract]
    public class CustomFault
    {
        [DataMember]
        public string Type { get; set; }

        [DataMember]
        public string Message { get; set; }
    }
}