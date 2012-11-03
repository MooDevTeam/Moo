using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.ServiceModel;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Description;
using System.ServiceModel.Channels;
using System.Net;
using System.Xml;
using System.Text;
namespace Moo.API
{
    public class CustomMessageFormatter : IDispatchMessageFormatter
    {
        IDispatchMessageFormatter oldFormatter;
        public CustomMessageFormatter(IDispatchMessageFormatter oldFormatter)
        {
            this.oldFormatter = oldFormatter;
        }

        public void DeserializeRequest(Message message, object[] parameters)
        {
            oldFormatter.DeserializeRequest(message, parameters);
        }

        public Message SerializeReply(MessageVersion messageVersion, object[] parameters, object result)
        {
            Message message = Message.CreateMessage(messageVersion, null, new MyJSSBodyWriter(result));
            HttpResponseMessageProperty httpProp = new HttpResponseMessageProperty();
            message.Properties.Add(HttpResponseMessageProperty.Name, httpProp);
            httpProp.Headers[HttpResponseHeader.ContentType] = "application/json";
            WebBodyFormatMessageProperty bodyFormat = new WebBodyFormatMessageProperty(WebContentFormat.Raw);
            message.Properties.Add(WebBodyFormatMessageProperty.Name, bodyFormat);
            return message;
        }

        class MyJSSBodyWriter : BodyWriter
        {
            object result;
            public MyJSSBodyWriter(object result)
                : base(true)
            {
                this.result = result;
            }
            protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
            {
                writer.WriteStartElement("Binary");
                JavaScriptSerializer jss = new JavaScriptSerializer();
                string serialized = jss.Serialize(this.result);
                byte[] bytes = Encoding.UTF8.GetBytes(serialized);
                writer.WriteBase64(bytes, 0, bytes.Length);
                writer.WriteEndElement();
            }
        }

        public class Behavior : IOperationBehavior
        {

            public void AddBindingParameters(OperationDescription operationDescription, BindingParameterCollection bindingParameters)
            {
            }

            public void ApplyClientBehavior(OperationDescription operationDescription, ClientOperation clientOperation)
            {
            }

            public void ApplyDispatchBehavior(OperationDescription operationDescription, DispatchOperation dispatchOperation)
            {
                dispatchOperation.Formatter = new CustomMessageFormatter(dispatchOperation.Formatter);
            }

            public void Validate(OperationDescription operationDescription)
            {
            }
        }
    }
}