using System.Collections.Generic;
namespace WikiPlex.Compilation.Macros
{
    /// <summary>
    /// This macro will render internal links.
    /// </summary>
    /// <example><code language="none">
    /// [Problem:1]
    /// </code></example>
    public class InternalLinkMacro : IMacro
    {
        public string Id
        {
            get { return "InternalLink"; }
        }

        public IList<MacroRule> Rules
        {
            get
            {
                return new List<MacroRule>(){
                    new MacroRule(
                        @"(?i)(\[problem\s*:\s*)(\d+)(\s*])",
                        new Dictionary<int, string>
                            {
                                {1, ScopeName.Remove},
                                {2, ScopeName.ProblemLink},
                                {3, ScopeName.Remove}
                            }),
                    new MacroRule(
                        @"(?i)(\[article\s*:\s*)(\d+)(\s*])",
                        new Dictionary<int, string>
                            {
                                {1, ScopeName.Remove},
                                {2, ScopeName.ArticleLink},
                                {3, ScopeName.Remove}
                            }),
                    new MacroRule(
                        @"(?i)(\[testcase\s*:\s*)(\d+)(\s*])",
                        new Dictionary<int, string>
                            {
                                {1, ScopeName.Remove},
                                {2, ScopeName.TestCaseLink},
                                {3, ScopeName.Remove}
                            }),
                    new MacroRule(
                        @"(?i)(\[user\s*:\s*)(\d+)(\s*])",
                        new Dictionary<int, string>
                            {
                                {1, ScopeName.Remove},
                                {2, ScopeName.UserLink},
                                {3, ScopeName.Remove}
                            }),
                    new MacroRule(
                        @"(?i)(\[record\s*:\s*)(\d+)(\s*])",
                        new Dictionary<int, string>
                            {
                                {1, ScopeName.Remove},
                                {2, ScopeName.RecordLink},
                                {3, ScopeName.Remove}
                            }),
                    new MacroRule(
                        @"(?i)(\[contest\s*:\s*)(\d+)(\s*])",
                        new Dictionary<int, string>
                            {
                                {1, ScopeName.Remove},
                                {2, ScopeName.ContestLink},
                                {3, ScopeName.Remove}
                            }),
                };
            }
        }
    }
}