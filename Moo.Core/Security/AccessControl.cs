using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moo.Core.DB;
namespace Moo.Core.Security
{
    public static class AccessControl
    {
        static SiteUser u;
        static MooDB db;
        static Dictionary<Function, List<Func<Problem, bool?>>> problemRules = new Dictionary<Function, List<Func<Problem, bool?>>>
        {
            {Function.CreateProblem,new List<Func<Problem,bool?>>(){
                p=>u.Role>=SiteRole.NormalUser?(bool?)true:null,
            }},
            {Function.ReadProblem,new List<Func<Problem,bool?>>(){
                
            }},
            {Function.ModifyProblem,new List<Func<Problem,bool?>>(){
                p=>u.Role<=SiteRole.Reader?(bool?)false:null,
                p=>u.Role>=SiteRole.Worker?(bool?)true:null,
                p=>p.CreatedBy.ID==u.ID?(bool?)true:null,
            }},
            {Function.DeleteProblem,new List<Func<Problem,bool?>>(){
                p=>u.Role<=SiteRole.Reader?(bool?)false:null,
                p=>u.Role>=SiteRole.Worker?(bool?)true:null,
                p=>p.CreatedBy.ID==u.ID?(bool?)true:null,
            }}, 
        };
        public static bool Check(object @object, Function function)
        {
            if (@object is Problem)
            {
                return CheckRules(@object as Problem, problemRules, function);
            }
            else if (@object is ProblemRevision)
            {
            }
            return false;
        }
        static bool CheckRules<T>(T @object, Dictionary<Function, List<Func<T, bool?>>> rules, Function function)
        {
            foreach (var func in rules[function])
            {
                bool? result = func(@object);
                if (result != null) return (bool)result;
            }
            return false;
        }
    }
}
