using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using System.Net.WebSockets;
using Microsoft.Web.WebSockets;
using Moo.Core.Security;
using Moo.Core.DB;
namespace Moo.API.WebSockets
{
    public class WebSocketsAPIHandler : WebSocketHandler
    {
        public static readonly Dictionary<int, WebSocketsAPIHandler> Clients = new Dictionary<int, WebSocketsAPIHandler>();

        SiteUser CurrentUser;

        static async Task Broadcast(ResponseMessage msg)
        {
            byte[] toSend = Encoding.UTF8.GetBytes(msg.ToString());
            List<Task> tasks = new List<Task>();
            foreach (WebSocketsAPIHandler handler in Clients.Values)
            {
                tasks.Add(handler.WebSocketContext.WebSocket.SendAsync(new ArraySegment<byte>(toSend), WebSocketMessageType.Text, true, CancellationToken.None));
            }

            await Task.WhenAll(tasks);
        }

        async Task Send(ResponseMessage msg)
        {
            byte[] toSend = Encoding.UTF8.GetBytes(msg.ToString());
            await WebSocketContext.WebSocket.SendAsync(new ArraySegment<byte>(toSend), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public static void NotifyNewMessage(User to)
        {
            if (to == null)
            {
                Broadcast(new ResponseMessage
                {
                    Type = ResponseMessage.MessageType.NewMessage,
                    ID = null
                });
            }
            else
            {
                lock (Clients)
                {
                    if (Clients.ContainsKey(to.ID))
                    {
                        Clients[to.ID].Send(new ResponseMessage
                        {
                            Type = ResponseMessage.MessageType.NewMessage,
                            ID = Security.CurrentUser.ID
                        });
                    }
                }
            }
        }

        public static void Kick(int userID)
        {
            lock (Clients)
            {
                if (Clients.ContainsKey(userID))
                {
                    WebSocketsAPIHandler conn = Clients[userID];
                    conn.Send(new ResponseMessage
                    {
                        Type = ResponseMessage.MessageType.Kicked
                    }).ContinueWith(_ => conn.Close());
                    Clients.Remove(userID);
                }
            }
        }

        public override void OnOpen()
        {
            if (!Security.Authenticate(WebSocketContext.QueryString["Auth"]))
            {
                Send(new ResponseMessage
                {
                    Type = ResponseMessage.MessageType.NotAuthenticated
                }).Wait();
                this.Close();
            }
            else
            {
                int userID = Security.CurrentUser.ID;

                Task toWait = null;
                bool loginSuccess = true;
                lock (Clients)
                {
                    if (Clients.ContainsKey(userID))
                    {
                        if (WebSocketContext.QueryString["forceLogin"] == "true")
                        {
                            WebSocketsAPIHandler oldConn = Clients[userID];
                            oldConn.CurrentUser = null;
                            toWait = oldConn.Send(new ResponseMessage
                            {
                                Type = ResponseMessage.MessageType.AnotherLogin
                            }).ContinueWith(_ => oldConn.Close());

                            Clients.Remove(userID);
                            Clients.Add(userID, this);
                        }
                        else
                        {
                            toWait = Send(new ResponseMessage
                            {
                                Type = ResponseMessage.MessageType.AlreadyLogin
                            }).ContinueWith(_ => Close());
                            loginSuccess = false;
                        }
                    }
                    else
                    {
                        Clients.Add(userID, this);
                    }
                }

                if (toWait != null)
                    toWait.Wait();
                if (loginSuccess)
                {
                    CurrentUser = Security.CurrentUser;
                    Broadcast(new ResponseMessage
                    {
                        Type = ResponseMessage.MessageType.Login,
                        ID = CurrentUser.ID
                    }).Wait();
                }
            }
        }

        public override void OnClose()
        {
            Task toWait = null;
            lock (Clients)
            {
                if (CurrentUser != null && Clients.ContainsKey(CurrentUser.ID))
                {
                    Clients.Remove(CurrentUser.ID);
                    toWait = Broadcast(new ResponseMessage()
                    {
                        Type = ResponseMessage.MessageType.Logout,
                        ID = CurrentUser.ID
                    });
                }
            }
            if (toWait != null) toWait.Wait();
        }

        public override void OnError()
        {
            base.OnError();
        }

        public override void OnMessage(string message)
        {
            RequestMessage request = RequestMessage.FromString(message);
            switch (request.Type)
            {
                case RequestMessage.MessageType.Logout:
                    Close();
                    break;
                case RequestMessage.MessageType.Echo:
                    Send(new ResponseMessage
                    {
                        Type = ResponseMessage.MessageType.Echo,
                        Content = request.Content
                    }).Wait();
                    break;
                case RequestMessage.MessageType.Whoami:
                    Send(new ResponseMessage
                    {
                        Type = ResponseMessage.MessageType.Whoami,
                        Content = "You are " + CurrentUser.Name
                    }).Wait();
                    break;
            }
        }
    }
}