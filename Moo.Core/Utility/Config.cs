using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
namespace Moo.Core.Utility
{
    public static class Config
    {
        public static string TemporyFileDirectory
        {
            get { return ConfigurationManager.AppSettings["TemporyFileDirectory"]; }
        }
        public static string UploadFileDirectory
        {
            get { return ConfigurationManager.AppSettings["UploadFileDirectory"]; }
        }
        public static TimeSpan BlobExpires
        {
            get { return TimeSpan.Parse(ConfigurationManager.AppSettings["BlobExpires"]); }
        }
    }
}
