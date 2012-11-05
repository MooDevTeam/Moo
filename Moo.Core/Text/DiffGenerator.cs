using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace Moo.Core.Text
{
    /// <summary>
    /// Diff 生成器
    /// </summary>
    public static class DiffGenerator
    {
        public static string Generate(string oldText, string newText)
        {
            List<VDifference.Result> results = VDifference.GetDifference(oldText, newText, new string[] { "\r", "\n" });

            StringBuilder sb = new StringBuilder();
            foreach (VDifference.Result result in results)
            {
                switch (result.Flag)
                {
                    case VDifference.ChangeFlag.Keep:
                        sb.Append(HttpUtility.HtmlEncode(result.ToString()));
                        break;
                    case VDifference.ChangeFlag.Insert:
                        sb.AppendFormat("<ins style='background:lightgreen;'>{0}</ins>", HttpUtility.HtmlEncode(result.ToString()));
                        break;
                    case VDifference.ChangeFlag.Delete:
                        sb.AppendFormat("<del style='background:pink;'>{0}</del>", HttpUtility.HtmlEncode(result.ToString()));
                        break;
                }
            }
            return sb.ToString();
        }
    }
}