using System.Collections.Generic;
namespace WikiPlex.Compilation.Macros
{
    /// <summary>
    /// This macro will render math.
    /// </summary>
    public class MathMarco : IMacro
    {
        public string Id
        {
            get { return "Math"; }
        }

        public IList<MacroRule> Rules
        {
            get
            {
                return new List<MacroRule>(){
                    new MacroRule(
                       @"(?is)(<math>)(.*?)(</math>)",
                        new Dictionary<int,string>(){
                            {1,ScopeName.Remove},
                            {2,ScopeName.Math},
                            {3,ScopeName.Remove}
                        })
                };
            }
        }
    }
}