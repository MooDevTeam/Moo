using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Data.Objects;
using Moo.Core.DB;
using Moo.Core.IndexAPI;
namespace Moo.Debug
{
    class Program
    {
        static System.Web.Script.Serialization.JavaScriptSerializer tool = new System.Web.Script.Serialization.JavaScriptSerializer();
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
        class RSA
        {
            public string Modulus;
            public string Exponent;
        }

        static void Main(string[] args)
        {
            //Moo.Core.Daemon.FullIndexDaemon.Instance.Start();
            try
            {
                while (true)
                {
                    string toSearch = Console.ReadLine();
                    DateTime st = DateTime.Now;
                    using (var search = new Search())
                    {
                        foreach (Search.SearchResult result in search.DoSearch(toSearch, "Article", 5))
                        {
                            Console.WriteLine("ID:{0} Title:{1}", result.ID, result.Title);
                            foreach (Search.SearchResult.ContentSegment match in result.Content)
                            {
                                Console.Write(" {0} ", match.Text);
                            }
                            Console.WriteLine();
                        }
                    }
                    Console.WriteLine("{0} -------------------------------------------",DateTime.Now-st);
                }
            }
            catch (Exception)
            {
            }
            //Moo.Core.Daemon.FullIndexDaemon.Instance.Stop();
            /*
            string PublicKey = Get("PublicKey");
            RSA rsa=tool.Deserialize<RSA>(PublicKey);
            var rsacsp = new System.Security.Cryptography.RSACryptoServiceProvider();
            rsacsp.ImportParameters(new System.Security.Cryptography.RSAParameters()
            {
                Modulus=Convert.FromBase64String(rsa.Modulus),
                Exponent=Convert.FromBase64String(rsa.Exponent)
            });
            byte[] pwd = rsacsp.Encrypt(Encoding.UTF8.GetBytes("ShaBi"), false);
            Auth = Post("Login", tool.Serialize(new
            {
                userID=2,
                password=Convert.ToBase64String(pwd)
            }));
            Auth = Auth.Substring(1, Auth.Length - 2);
            Console.WriteLine(Auth);
            for (int i = 1000; i < 2000; i++)
            {
                string content = Encoding.Unicode.GetString(File.ReadAllBytes(@"D:\Prob\" + i.ToString()+".txt"));
                content=content.Replace('\'',' ');
                Console.WriteLine(Post("Problems", tool.Serialize(new
                {
                    problem = new
                        {
                            Name = i.ToString(),
                            Type = "Traditional",
                            Content = content
                        }
                })));
            }
             * */
        }
    }
}
