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
        const int BUF_SIZE = 1024 * 400;

        public void ProcessRequest(HttpContext context)
        {
            if (context.Request.HttpMethod == "GET")
            {
                int id = int.Parse(context.Request.QueryString["id"]);
                if (Binary.Has(id))
                {
                    byte[] bytes = Binary.Get(id);

                    string acceptEncoding = context.Request.Headers["Accept-Encoding"];
                    if (acceptEncoding != null && acceptEncoding.Contains("deflate"))
                    {
                        context.Response.AppendHeader("Content-Encoding", "deflate");
                        using (MemoryStream mem = new MemoryStream())
                        {
                            using (DeflateStream deflate = new DeflateStream(mem, CompressionMode.Compress))
                            {
                                deflate.Write(bytes, 0, bytes.Length);
                            }
                            bytes = mem.ToArray();
                        }
                    }

                    string filename = Binary.GetName(id);
                    if (filename == null)
                    {
                        filename = "Blob" + id;
                    }
                    filename += ".bin";
                    Download(context, bytes, filename);
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
                    context.Response.ContentType = "application/json";
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
            get
            {
                return true;
            }
        }

        void Download(HttpContext context, byte[] bytes, string filename)
        {
            int start = 0;
            int length = bytes.Length;

            if (context.Request.Headers["Range"] != null)
            {
                start = int.Parse(context.Request.Headers["Range"].Replace("bytes=", "").Split('-')[0]);
                length -= start;

                context.Response.StatusCode = 206;
                context.Response.AddHeader("Content-Range", "bytes " + start + "-" + (bytes.Length - 1) + "/" + bytes.Length);
            }

            context.Response.AddHeader("Content-Length", length.ToString());
            context.Response.ContentType = "application/octet-stream";
            context.Response.AddHeader("Content-Disposition", "attachment; filename=" + filename);

            while (length > 0)
            {
                if (!context.Response.IsClientConnected) return;

                int toWrite = Math.Min(length, BUF_SIZE);
                context.Response.OutputStream.Write(bytes, start, toWrite);
                context.Response.Flush();

                start += toWrite;
                length -= toWrite;
            }
        }
    }
}