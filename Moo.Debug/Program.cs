using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Data.Objects;
using Moo.Core.DB;
namespace Moo.Debug
{
    class Program
    {
        static string ROOT = "http://localhost:52590/JsonAPI.svc/";
        static string Auth;
        static string ReadResponse(WebRequest request)
        {
            WebResponse response;
            response = request.GetResponse();
            using (response)
            {
                using (Stream stream = response.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
        }
        static string Post(string uri, string data)
        {
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            WebRequest request = HttpWebRequest.Create(ROOT + uri);
            request.Method = "POST";
            request.ContentType = "text/json";
            request.ContentLength = dataBytes.Length;
            request.Headers.Add("Auth", Auth);
            request.Timeout = System.Threading.Timeout.Infinite;
            request.GetRequestStream().Write(dataBytes, 0, dataBytes.Length);
            return ReadResponse(request);
        }

        static string Put(string uri, string data)
        {
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            WebRequest request = HttpWebRequest.Create(ROOT + uri);
            request.Method = "PUT";
            request.ContentType = "text/json";
            request.ContentLength = dataBytes.Length;
            request.Headers.Add("Auth", Auth);
            request.Timeout = System.Threading.Timeout.Infinite;
            request.GetRequestStream().Write(dataBytes, 0, dataBytes.Length);
            return ReadResponse(request);
        }

        static string Get(string uri)
        {
            WebRequest request = HttpWebRequest.Create(ROOT + uri);
            request.Headers.Add("Auth", Auth);
            request.Timeout = System.Threading.Timeout.Infinite;
            return ReadResponse(request);
        }

        static string Delete(string uri)
        {
            WebRequest request = HttpWebRequest.Create(ROOT + uri);
            request.Method = "DELETE";
            request.Timeout = System.Threading.Timeout.Infinite;
            request.Headers.Add("Auth", Auth);
            return ReadResponse(request);
        }

        static void Main(string[] args)
        {
            var tool = new System.Web.Script.Serialization.JavaScriptSerializer();
            Console.Write(tool.Serialize(new
            {
                ID = 123,
                Name = "onetwogoo\"\\"
            }));
        }
    }
}
