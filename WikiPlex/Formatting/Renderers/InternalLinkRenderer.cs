using System.Collections.Generic;
using WikiPlex.Common;

namespace WikiPlex.Formatting.Renderers
{
    /// <summary>
    /// render internal link
    /// </summary>
    public class InternalLinkRenderer : Renderer
    {
        protected override ICollection<string> ScopeNames
        {
            get
            {
                return new[] { 
                    ScopeName.ProblemLink,
                    ScopeName.ArticleLink,
                    ScopeName.UserLink,
                    ScopeName.TestCaseLink,
                    ScopeName.RecordLink,
                    ScopeName.ContestLink,
                };
            }
        }

        protected override string InvalidMacroError
        {
            get
            {
                return "Cannot resolve internal link macro, invalid number of parameters.";
            }
        }

        const string FORMAT = "<a href=\"?page={0}&id={2}\" onclick=\"Page.item.{0}.load({{id:{2}}}); return false;\">第{2}号{1}</a>";

        protected override string PerformExpand(string scopeName, string input, System.Func<string, string> htmlEncode, System.Func<string, string> attributeEncode)
        {
            if (scopeName == ScopeName.ProblemLink)
            {
                return string.Format(FORMAT, "problem", "题目", input);
            }
            else if (scopeName == ScopeName.ContestLink)
            {
                return string.Format(FORMAT, "contest", "比赛", input);
            }
            else if (scopeName == ScopeName.ArticleLink)
            {
                return string.Format(FORMAT, "article", "文章", input);
            }
            else if (scopeName == ScopeName.RecordLink)
            {
                return string.Format(FORMAT, "record", "记录", input);
            }
            else if (scopeName == ScopeName.TestCaseLink)
            {
                return string.Format(FORMAT, "testCase", "测试数据", input);
            }
            else if (scopeName == ScopeName.UserLink)
            {
                return string.Format(FORMAT, "user", "用户", input);
            }
            else
            {
                throw new RenderException();
            }
        }
    }
}