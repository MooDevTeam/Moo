using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Moo.Core.DB;
namespace Moo.Core.Tester
{
    public interface ITester
    {
        TestResult TestTraditional(string source, string language, IEnumerable<TraditionalTestCase> cases);
        TestResult TestSpecialJudged(string source, string language, IEnumerable<SpecialJudgedTestCase> cases);
        TestResult TestInteractive(string source, string language, IEnumerable<InteractiveTestCase> cases);
        TestResult TestAnswerOnly(IDictionary<int,string> answers, IEnumerable<AnswerOnlyTestCase> cases);
    }
}