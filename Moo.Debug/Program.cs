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

        static string Delete(string uri)
        {
            WebRequest request = HttpWebRequest.Create(ROOT + uri);
            request.Method = "DELETE";
            request.Headers.Add("Auth", Auth);
            return ReadResponse(request);
        }

        static void Main(string[] args)
        {
            string base64 = "TXpLeU1qYmw1UUlB";
            byte[] compressed = Convert.FromBase64String(base64);
            Console.OpenStandardOutput().Write(compressed, 0, compressed.Length);
            using (MemoryStream mem = new MemoryStream(compressed))
            {
                using (DeflateStream deflate = new DeflateStream(mem, CompressionMode.Decompress))
                {
                    byte[] buffer = new byte[10*1024];
                    int len=deflate.Read(buffer, 0, buffer.Length);
                    Console.Write(Encoding.Default.GetString(buffer,0,len));
                }
            }
        }
    }
}
