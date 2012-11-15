using System.Collections.Generic;

namespace WikiPlex.Compilation.Macros
{
    /// <summary>
    /// Will output source code rendered as plain text or as syntax highighted for certain languages.
    /// </summary>
    /// <example><code language="none">
    /// {{this is a single-line example}}
    /// {{
    /// this is a multi-line example with no syntax highlighting
    /// }}
    /// {code:aspx c#} ASPX C# {code:aspx c#}
    /// {code:aspx vb.net} ASPX VB.Net {code:aspx vb.net}
    /// {code:ashx} ASHX {code:ashx}
    /// {code:c++} C++ {code:c++}
    /// {code:c#} C# {code:c#}
    /// {code:vb.net} VB.Net {code:vb.net}
    /// {code:html} HTML {code:html}
    /// {code:sql} SQL {code:sql}
    /// {code:java} Java {code:java}
    /// {code:javascript} Javascript {code:javascript}
    /// {code:xml} XML {code:xml}
    /// {code:php} PHP {code:php}
    /// {code:css} CSS {code:css}
    /// {code:pascal} Pascal {code:pascal}
    /// {code:powershell} Powershell {code:powershell}
    /// </code></example>
    public class SourceCodeMacro : IMacro
    {
        /// <summary>
        /// Gets the id of the macro.
        /// </summary>
        public string Id
        {
            get { return "SourceCode"; }
        }

        /// <summary>
        /// Gets the list of rules for the macro.
        /// </summary>
        public IList<MacroRule> Rules
        {
            get
            {
                return new List<MacroRule>
                           {
                               new MacroRule(@"(?s)(?:(?:{"").*?(?:""}))"),
                               new MacroRule(
                                   @"(?m)({{)(.*?)(}})",
                                   new Dictionary<int, string>
                                       {
                                           {1, ScopeName.Remove},
                                           {2, ScopeName.SingleLineCode},
                                           {3, ScopeName.Remove}
                                       }),
                               new MacroRule(
                                   @"(?s)({{(?:\s+\r?\n)?)((?>(?:(?!}}|{{).)*)(?>(?:{{(?>(?:(?!}}|{{).)*)}}(?>(?:(?!}}|{{).)*))*).*?)((?:\r?\n)?}})",
                                   new Dictionary<int, string>
                                       {
                                           {1, ScopeName.Remove},
                                           {2, ScopeName.MultiLineCode},
                                           {3, ScopeName.Remove}
                                       }),
                               new MacroRule(
                                   @"(?si)(\{code:\s*)((?<Language>[^\}]+))(\s*\}\r?\n)(.*?)(\r?\n\{code:\s*\k<Language>\s*\}(?:\r?\n)?)",
                                   new Dictionary<int, string>
                                       {
                                           {1, ScopeName.Remove},
                                           {2, ScopeName.SourceCodeLanguage},
                                           {3, ScopeName.Remove},
                                           {4, ScopeName.SourceCode},
                                           {5, ScopeName.Remove}
                                       })
                           };
            }
        }
    }
}