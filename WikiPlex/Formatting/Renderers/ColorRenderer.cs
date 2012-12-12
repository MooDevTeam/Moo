using System.Collections.Generic;
using WikiPlex.Common;

namespace WikiPlex.Formatting.Renderers
{
    /// <summary>
    /// render color
    /// </summary>
    public class ColorRenderer : Renderer
    {
        protected override ICollection<string> ScopeNames
        {
            get { return new[] { ScopeName.ColorBegin, ScopeName.ColorEnd }; }
        }

        protected override string InvalidMacroError
        {
            get
            {
                return "Cannot resolve color macro, invalid number of parameters.";
            }
        }

        protected override string PerformExpand(string scopeName, string input, System.Func<string, string> htmlEncode, System.Func<string, string> attributeEncode)
        {
            if (scopeName == ScopeName.ColorBegin)
            {
                input = input.Substring(7, input.Length - 7 - 1);
                return string.Format("<span style='color: {0};'>", attributeEncode(input));
            }
            else if (scopeName == ScopeName.ColorEnd)
            {
                return "</span>";
            }
            else
            {
                throw new RenderException();
            }
        }
    }
}