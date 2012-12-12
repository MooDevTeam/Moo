using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
namespace Moo.API.WebSockets
{
    public class RequestMessage
    {
        [ThreadStatic]
        static JavaScriptSerializer serializer;

        static JavaScriptSerializer Serializer
        {
            get
            {
                if (serializer == null) serializer = new JavaScriptSerializer();
                return serializer;
            }
        }

        public enum MessageType
        {
            Echo,Whoami,Logout
        }

        [ScriptIgnore]
        public MessageType Type;

        public string type
        {
            set
            {
                Type = (MessageType)Enum.Parse(typeof(MessageType), value);
            }
        }

        public string Content;

        public static RequestMessage FromString(string message)
        {
            return Serializer.Deserialize<RequestMessage>(message);
        }
    }
}