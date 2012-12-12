using System;
using System.Collections.Generic;
using System.Threading;

namespace WikiPlex.Formatting.Renderers
{
    /// <summary>
    /// Will render all source code scopes.
    /// </summary>
    public class SourceCodeRenderer : Renderer
    {
        static readonly Dictionary<string, string> LanguageMap = new Dictionary<string, string>
        {
            {"c++","cpp"},
            {"pascal","delphi"}
        };

        /// <summary>
        /// Gets the collection of scope names for this <see cref="IRenderer"/>.
        /// </summary>
        protected override ICollection<string> ScopeNames
        {
            get
            {
                return new[] {
                                ScopeName.SingleLineCode, ScopeName.MultiLineCode,
                                ScopeName.SourceCodeLanguage,ScopeName.SourceCode
                             };
            }
        }

        /// <summary>
        /// Will expand the input into the appropriate content based on scope.
        /// </summary>
        /// <param name="scopeName">The scope name.</param>
        /// <param name="input">The input to be expanded.</param>
        /// <param name="htmlEncode">Function that will html encode the output.</param>
        /// <param name="attributeEncode">Function that will html attribute encode the output.</param>
        /// <returns>The expanded content.</returns>
        protected override string PerformExpand(string scopeName, string input, Func<string, string> htmlEncode, Func<string, string> attributeEncode)
        {
            switch (scopeName)
            {
                case ScopeName.SingleLineCode:
                    return string.Format("<span class=\"codeInline\">{0}</span>", htmlEncode(input));
                case ScopeName.MultiLineCode:
                    return FormatSyntax(input, htmlEncode);
                case ScopeName.SourceCodeLanguage:
                    string transaltedName = LanguageMap.ContainsKey(input.ToLower()) ? LanguageMap[input.ToLower()] : input.ToLower();
                    return string.Format("<pre><code class=\"language-{0}\">",transaltedName);
                case ScopeName.SourceCode:
                    return string.Format("{0}</pre></code>",htmlEncode(input));
                default:
                    return null;
            }
        }

        private static string FormatSyntax(string input, Func<string, string> htmlEncode)
        {
            if (input.EndsWith(Environment.NewLine))
                input = input.Substring(0, input.Length - Environment.NewLine.Length);
            return string.Format("<pre>{0}</pre>", htmlEncode(input));
        }
    }
}