using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security;
using System.ServiceModel;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;

namespace Moo.API.API
{
    public class ErrorHandler :  IErrorHandler
    {
        static FaultCode securityExceptionCode = new FaultCode("SecurityException");
        static FaultCode argumentExceptionCode = new FaultCode("ArgumentException");
        public bool HandleError(Exception error)
        {
            return false;
        }

        public void ProvideFault(Exception error, MessageVersion version, ref Message fault)
        {
            if (error is SecurityException)
            {
                Message.CreateMessage(version, new FaultException(error.Message, securityExceptionCode).CreateMessageFault(), "");
            }
            else if (error is ArgumentException)
            {
                Message.CreateMessage(version, new FaultException(error.Message, argumentExceptionCode).CreateMessageFault(), "");
            }
        }
    }
}