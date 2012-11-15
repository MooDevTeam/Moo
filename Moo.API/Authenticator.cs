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
            "Login", "GetUserByName", "CreateUser",
            "ListPostItem","ParseWiki"
        };
        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            if (request.Properties.ContainsKey("HttpOperationName"))
            {
                string operation = (string)request.Properties["HttpOperationName"];
                if (!ALLOWED_OPERATIONS.Contains(operation))
                {
                    try
                    {
                        string sToken = WebOperationContext.Current.IncomingRequest.Headers["Auth"];
                        Security.Authenticate(sToken);
                    }
                    catch
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