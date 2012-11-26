using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;
using Moo.Core.Utility;
using Moo.Core.DB;
namespace Moo.Core.Daemon
{
    /// <summary>
    /// 比赛服务进程
    /// </summary>
    public class ContestDaemon : Daemon
    {
        public static readonly ContestDaemon Instance = new ContestDaemon();
        protected override int Run()
        {
            using (MooDB db = new MooDB())
            {
                Contest contest = (from c in db.Contests
                                   where c.Status == "Before" && c.StartTime <= DateTime.Now
                                   select c).FirstOrDefault<Contest>();
                if (contest != null)
                {
                    contest.Status = "During";
                    foreach (Problem problem in contest.Problem)
                    {
                        problem.EnableTesting = contest.EnableTestingOnStart;
                        problem.TestCaseHidden = contest.HideTestCaseOnStart;
                        problem.PostLocked = contest.LockPostOnStart;
                        problem.TestCaseLocked = contest.LockTestCaseOnStart;
                        problem.Locked = contest.LockProblemOnStart;
                        problem.Hidden = contest.HideProblemOnStart;
                        problem.RecordLocked = contest.LockRecordOnStart;
                        problem.JudgeInfoHidden = contest.HideJudgeInfoOnStart;
                        problem.ArticleLocked = contest.LockArticleOnStart;
                    }
                    db.SaveChanges();
                    return 0;
                }

                contest = (from c in db.Contests
                           where c.Status == "During" && c.EndTime <= DateTime.Now
                           select c).FirstOrDefault<Contest>();
                if (contest != null)
                {
                    contest.Status = "After";
                    foreach (Problem problem in contest.Problem)
                    {
                        problem.EnableTesting = contest.EnableTestingOnEnd;
                        problem.TestCaseHidden = contest.HideTestCaseOnEnd;
                        problem.PostLocked = contest.LockPostOnEnd;
                        problem.TestCaseLocked = contest.LockTestCaseOnEnd;
                        problem.Locked = contest.LockProblemOnEnd;
                        problem.Hidden = contest.HideProblemOnEnd;
                        problem.RecordLocked = contest.LockRecordOnEnd;
                        problem.JudgeInfoHidden = contest.HideJudgeInfoOnEnd;
                        problem.ArticleLocked = contest.LockArticleOnEnd;
                    }
                    db.SaveChanges();
                    return 0;
                }

                return 30 * 1000;
            }
        }
    }
}