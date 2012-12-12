using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using Moo.Core.DB;
namespace Moo.API
{
    /// <summary>
    /// File 的摘要说明
    /// </summary>
    public class FileHandler : IHttpHandler
    {
        static readonly Regex URL_PATTERN = new Regex(@"^/file/(\d+)$", RegexOptions.IgnoreCase);
        public void ProcessRequest(HttpContext context)
        {
            Match match = URL_PATTERN.Match(context.Request.Path);
            if (match.Success)
            {
                int id = int.Parse(match.Groups[1].ToString());
                using (MooDB db = new MooDB())
                {
                    UploadedFile file = (from f in db.UploadedFiles
                                         where f.ID == id
                                         select f).SingleOrDefault<UploadedFile>();
                    if (file == null)
                    {
                        context.Response.StatusCode = 404;
                        return;
                    }

                    context.Server.TransferRequest("/upload/" + file.FileName + "?filename=" + HttpUtility.UrlEncode(file.Name));
                }
            }
            else
            {
                context.Response.StatusCode = 404;
            }
        }

        public bool IsReusable
        {
            get
            {
                return true;
            }
        }
    }
}