using System.Collections.Generic;
namespace WikiPlex.Compilation.Macros
{
    /// <summary>
    /// This macro will render color.
    /// </summary>
    /// <example><code language="none">
    /// [color:Red|It's red!]
    /// </code></example>
    public class ColorMacro : IMacro
    {
        public string Id
        {
            get { return "Color"; }
        }

        public IList<MacroRule> Rules
        {
            get
            {
                return new List<MacroRule>(){
                    new MacroRule(
                       @"(?is)(<color:(?:[^;\r\n]*?)>).*?(</color>)",
                        new Dictionary<int,string>(){
                            {1,ScopeName.ColorBegin},
                            {2,ScopeName.ColorEnd}
                        })
                };
            }
        }
    }
}