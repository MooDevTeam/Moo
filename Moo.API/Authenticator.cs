using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Channels;
using System.Threading;
using System.ServiceModel.Web;
using Moo.Core.Security;
namespace Moo.API
{
    public class Authenticator : IDispatchMessageInspector
    {
        static readonly string[] ALLOWED_OPERATIONS = new string[]{
            "", "HelpPageInvoke",
            "Echo", "Debug",
            "Login", "GetUserByName", "CreateUser","GetPublicKey",
            "ParseWiki","Search"
        };
        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            WebOperationContext.Current.OutgoingResponse.Headers.Add("Cache-Control", "no-cache");
            if (request.Properties.ContainsKey("HttpOperationName"))
            {
                string operation = (string)request.Properties["HttpOperationName"];
                if (!ALLOWED_OPERATIONS.Contains(operation))
                {
                    string sToken = WebOperationContext.Current.IncomingRequest.Headers["Auth"];
                    if (!Security.Authenticate(sToken))
                    {
                        throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
                    }
                }
            }
            return null;
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
        }
    }
}