using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
namespace Moo.Debug
{
    class Program
    {
        static string ROOT = "http://localhost:52590/API/JsonAPI.svc/";
        static string Auth;
        static string ReadResponse(WebRequest request)
        {
            WebResponse response;
            try
            {
                response = request.GetResponse();
            }
            catch(WebException e)
            {
                response = e.Response;
            }
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
            request.GetRequestStream().Write(dataBytes, 0, dataBytes.Length);
            return ReadResponse(request);
        }

        static string Get(string uri)
        {
            WebRequest request = HttpWebRequest.Create(ROOT + uri);
            request.Headers.Add("Auth", Auth);
            return ReadResponse(request);
        }

        static void Main(string[] args)
        {
            Auth = Post("Login", "{\"userName\":\"onetwogoo\",\"password\":\"123456\"}");
            Auth = Auth.Substring(1, Auth.Length - 2);
            Console.WriteLine("Got Token <{0}>",Auth);

            Console.WriteLine(Get("Problems/41dde870-2967-4824-8984-2b46dfdd7777"));
            Console.WriteLine(Get("Problems/41dde870-2967-4824-8984-2b46dfdd7777/Revisions"));
            
            //Console.WriteLine(Post("Problems", "{\"problem\":{\"Name\":\"This is A Test Problem\",\"Type\":\"SpecialJudged\"}}"));
            //Console.WriteLine(Get("Problems/"));
            //Console.WriteLine(Put("Problems/4a2f7b66-e27a-4a06-a6c3-93455946d859", "{\"problem\":{\"Name\":\"Blah Blah\"}}"));
            //Console.WriteLine(Get("Problems"));
        }
    }
}
