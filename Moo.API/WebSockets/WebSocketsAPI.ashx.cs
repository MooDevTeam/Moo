using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Microsoft.Web.WebSockets;
namespace Moo.API.WebSockets
{
    public class WebSocketsAPI : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            if (context.IsWebSocketRequest)
            {
                context.AcceptWebSocketRequest(new WebSocketsAPIHandler());
            }
            else
            {
                context.Response.ContentType = "text/plain";
                context.Response.Write("Not WebSockets");
            }
        }

        public bool IsReusable
        {
            get
            {
                return true;
            }
        }
    }
}