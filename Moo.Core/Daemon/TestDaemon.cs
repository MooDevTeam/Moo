using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;
using System.Text.RegularExpressions;
using Moo.Core.DB;
using Moo.Core.Utility;
using Moo.Core.Tester;
namespace Moo.Core.Daemon
{
    /// <summary>
    ///Tester 后台进程
    /// </summary>
    public class TestDaemon : Daemon
    {
        ITester tester = new Moo.Core.Tester.MooTester.Tester();
        public static readonly TestDaemon Instance = new TestDaemon();

        private TestDaemon() { }

        protected override int Run()
        {
            using (MooDB db = new MooDB())
            {
                Record record = (from r in db.Records
                                 where r.JudgeInfo == null && r.Problem.EnableTesting
                                 select r).FirstOrDefault<Record>();
                var a = (from r in db.Records
                         where r.JudgeInfo == null
                         select r);
                if (record == null)
                {
                    return 5 * 1000;
                }
                else
                {
                    record.JudgeInfo = new JudgeInfo()
                    {
                        Record = record,
                        Score = -1,
                        Info = "<color:blue>*正在评测*</color>"
                    };
                    db.SaveChanges();
                    Test(db, record);
                    db.SaveChanges();
                    return 0;
                }
            }
        }

        void Test(MooDB db, Record record)
        {
            TestResult result;
            switch (record.Problem.Type)
            {
                case "Traditional":
                    result = TestTraditional(db, record);
                    break;
                case "SpecialJudged":
                    result = TestSpecialJudged(db, record);
                    break;
                case "Interactive":
                    result = TestInteractive(db, record);
                    break;
                case "AnswerOnly":
                    result = TestAnswerOnly(db, record);
                    break;
                default:
                    result = new TestResult()
                    {
                        Score = 0,
                        Info = "<color:red>*未知的题目类型*</color>"
                    };
                    break;
            }

            int oldScore = (from r in db.Records
                            where r.User.ID == record.User.ID && r.Problem.ID == record.Problem.ID
                                && r.JudgeInfo != null && r.JudgeInfo.Score >= 0
                            select r.JudgeInfo.Score).DefaultIfEmpty().Max();
            int currentScore = Math.Max(oldScore, result.Score);

            record.User.Score -= oldScore;
            record.Problem.ScoreSum -= oldScore;
            record.User.Score += currentScore;
            record.Problem.ScoreSum += currentScore;

            if (record.Problem.MaximumScore == null)
            {
                record.Problem.MaximumScore = result.Score;
            }
            else
            {
                record.Problem.MaximumScore = Math.Max(result.Score, (int)record.Problem.MaximumScore);
            }

            record.JudgeInfo.Score = result.Score;
            record.JudgeInfo.Info = result.Info;
        }

        TestResult TestTraditional(MooDB db, Record record)
        {
            IEnumerable<TraditionalTestCase> cases = from t in db.TestCases.OfType<TraditionalTestCase>()
                                                      where t.Problem.ID == record.Problem.ID
                                                      select t;
            return tester.TestTraditional(record.Code, record.Language, cases);
        }

        TestResult TestSpecialJudged(MooDB db, Record record)
        {
            IEnumerable<SpecialJudgedTestCase> cases = from t in db.TestCases.OfType<SpecialJudgedTestCase>()
                                                       where t.Problem.ID == record.Problem.ID
                                                       select t;
            return tester.TestSpecialJudged(record.Code, record.Language, cases);
        }

        TestResult TestInteractive(MooDB db, Record record)
        {
            IEnumerable<InteractiveTestCase> cases = from t in db.TestCases.OfType<InteractiveTestCase>()
                                                     where t.Problem.ID == record.Problem.ID
                                                     select t;
            return tester.TestInteractive(record.Code, record.Language, cases);
        }

        TestResult TestAnswerOnly(MooDB db, Record record)
        {
            IEnumerable<AnswerOnlyTestCase> cases = from t in db.TestCases.OfType<AnswerOnlyTestCase>()
                                                    where t.Problem.ID == record.Problem.ID
                                                    select t;
            Dictionary<int, string> answers = new Dictionary<int, string>();
            MatchCollection matches = Regex.Matches(record.Code, @"<Moo:Answer testCase='(\d+)'>(.*?)</Moo:Answer>", RegexOptions.Singleline);
            foreach (Match match in matches)
            {
                int testCaseID = int.Parse(match.Groups[1].Value);
                string answer = match.Groups[2].Value;
                answers.Add(testCaseID, answer);
            }
            return tester.TestAnswerOnly(answers, cases);
        }
    }
}