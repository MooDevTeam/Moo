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
        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            if (request.Properties.ContainsKey("HttpOperationName"))
            {
                string operation = (string)request.Properties["HttpOperationName"];
                if (!new[] { "", "HelpPageInvoke", "Login", "Echo", "Debug" }.Contains(operation))
                {
                    try
                    {
                        string sToken = WebOperationContext.Current.IncomingRequest.Headers["Auth"];
                        string[] splited = sToken.Split(',');
                        int userID = int.Parse(splited[0]);
                        int iToken = int.Parse(splited[1]);

                        if (SiteUsers.ByID[userID].Token != iToken) throw new Exception();
                        Thread.CurrentPrincipal = new CustomPrincipal() { Identity = SiteUsers.ByID[userID] };
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