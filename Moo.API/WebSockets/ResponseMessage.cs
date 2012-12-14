using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
namespace Moo.API.WebSockets
{
    public class ResponseMessage
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
            NotAuthenticated, AlreadyLogin, AnotherLogin,Kicked,
            Login, Logout,
            Echo, Whoami,
            NewMessage,TestComplete
        }

        [ScriptIgnore]
        public MessageType Type;

        public string type
        {
            get { return Type.ToString(); }
        }

        public int? ID;
        public string Content;
        public string Name;
        public string Email;

        public override string ToString()
        {
            return Serializer.Serialize(this);
        }
    }
}