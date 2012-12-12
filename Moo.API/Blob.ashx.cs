using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Text;
using System.IO.Compression;
namespace Moo.API
{
    public class Blob : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            if (context.Request.HttpMethod == "GET")
            {
                Guid id;
                if (!Guid.TryParse(context.Request.QueryString["id"],out id))
                {
                    context.Response.Write("Required Parameter id");
                    return;
                }
                if (Binary.Has(id))
                {
                    string filename = Binary.GetName(id);
                    if (filename == null)
                    {
                        filename = "Blob" + id;
                    }
                    filename += ".bin";
                    
                    context.Server.TransferRequest("/temp/" + Binary.GetFileName(id)+"?filename="+HttpUtility.UrlEncode(filename));
                }
                else
                {
                    context.Response.StatusCode = 404;
                }
            }
            else if (context.Request.HttpMethod == "POST")
            {
                Stream inputStream = context.Request.InputStream;
                if (context.Request.Headers["Content-Encoding"] == "deflate")
                {
                    inputStream = new DeflateStream(inputStream, CompressionMode.Decompress);
                }

                byte[] byteArray;
                using (inputStream)
                {
                    using (MemoryStream mem = new MemoryStream())
                    {
                        inputStream.CopyTo(mem);
                        byteArray = mem.ToArray();
                    }
                }

                if (context.Response.IsClientConnected)
                {
                    context.Response.ContentType = "text/plain";
                    context.Response.Write(Binary.Add(byteArray));
                }
            }
            else
            {
                context.Response.StatusCode = 405;
            }
        }

        public bool IsReusable
        {
            get { return true; }
        }


    }
}