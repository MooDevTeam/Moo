using System.Collections.Generic;
using WikiPlex.Common;
using System.Web;
namespace WikiPlex.Formatting.Renderers
{
    /// <summary>
    /// render math
    /// </summary>
    public class MathRenderer : Renderer
    {
        protected override ICollection<string> ScopeNames
        {
            get { return new[] { ScopeName.Math }; }
        }

        protected override string InvalidMacroError
        {
            get
            {
                return "Cannot resolve math macro, invalid number of parameters.";
            }
        }

        protected override string PerformExpand(string scopeName, string input, System.Func<string, string> htmlEncode, System.Func<string, string> attributeEncode)
        {
            if (scopeName == ScopeName.Math)
            {
                return string.Format("<img src='https://chart.googleapis.com/chart?cht=tx&chf=bg,s,00000000&chl={0}' alt='{1}' />", HttpUtility.UrlEncode(input.Trim()), input.Trim());
            }
            else
            {
                throw new RenderException();
            }
        }
    }
}