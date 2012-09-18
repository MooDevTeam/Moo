using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moo.Core.DB;
namespace Moo.Core.Logic
{
    public static class Problems
    {
        public static void Create(Problem problem)
        {
            Security.Authorize("problem.create", false);
            problem.AllowTesting = true;
            problem.Hidden = false;
            problem.LatestRevision = null;
            problem.LatestSolution = null;
            problem.Lock = false;
            problem.LockPost = false;
            problem.LockRecord = false;
            problem.LockSolution = false;
            problem.LockTestCase = false;
            problem.MaximumScore = null;
            problem.ScoreSum = 0;
            problem.SubmissionCount = 0;
            problem.SubmissionUser = 0;
            problem.TestCaseHidden = false;
            if (problem.Type != "Tranditional" && problem.Type != "SpecialJudged" && problem.Type != "Interactive" && problem.Type != "AnswerOnly")
            {
                throw new ArgumentException("不支持的题目类型：" + problem.Type);
            }

            MooDB db = new MooDB();
            db.Problems.AddObject(problem);
            db.SaveChanges();
        }

        public static IEnumerable<Problem> List()
        {
            Security.Authorize("problem.list", true);
            return new MooDB().Problems;
        }

        public static Problem Get(int id)
        {
            MooDB db = new MooDB();
            Problem problem = (from p in db.Problems
                               where p.ID == id
                               select p).SingleOrDefault<Problem>();

            if (problem == null) throw new ArgumentException("无此题目");
            return problem;
        }

        public static void Update(Problem problem)
        {
            Security.Authorize("problem.modify", false);
            MooDB db = new MooDB();
            Problem theProblem = (from p in db.Problems
                                  where p.ID == problem.ID
                                  select p).SingleOrDefault<Problem>();
            if (theProblem == null) throw new ArgumentException("无此题目");
            theProblem.AllowTesting = problem.AllowTesting;
            theProblem.Hidden = problem.Hidden;
            theProblem.Lock = problem.Lock;
            theProblem.LockPost = problem.LockPost;
            theProblem.LockRecord = problem.LockRecord;
            theProblem.LockSolution = problem.LockSolution;
            theProblem.LockTestCase = problem.LockTestCase;
            theProblem.TestCaseHidden = problem.TestCaseHidden;
            theProblem.Name = problem.Name;
            if (problem.Type != "Tranditional" && problem.Type != "SpecialJudged" && problem.Type != "Interactive" && problem.Type != "AnswerOnly")
            {
                throw new ArgumentException("不支持的题目类型：" + problem.Type);
            }
            theProblem.Type = problem.Type;

            db.SaveChanges();
        }

        public static void Delete(int id)
        {
            Security.Authorize("problem.delete", false);
            MooDB db = new MooDB();
            Problem problem = (from p in db.Problems
                               where p.ID == id
                               select p).SingleOrDefault<Problem>();
            if (problem == null) throw new ArgumentException("无此题目");

            if (problem.Contest.Any()) throw new InvalidOperationException("尚有比赛使用此题目");

            db.Problems.DeleteObject(problem);
            db.SaveChanges();
        }

        public static ProblemRevision GetRevision(int id)
        {
            Security.Authorize("problem.read", true);
            MooDB db = new MooDB();
            ProblemRevision revision = (from r in db.ProblemRevisions
                                        where r.ID == id
                                        select r).SingleOrDefault<ProblemRevision>();

            if (revision == null) throw new ArgumentException("无此题目版本");
            if (revision.Problem.Hidden) Security.Authorize("problem.hidden.read", false);
            return revision;
        }

        public static void CreateRevision(ProblemRevision revision)
        {
            Security.Authorize("problem.update", false);
            MooDB db = new MooDB();

            revision.Problem = (from p in db.Problems
                                where p.ID == revision.Problem.ID
                                select p).SingleOrDefault<Problem>();
            if (revision.Problem == null) throw new ArgumentException("无此题目");
            if (revision.Problem.Lock) Security.Authorize("problem.locked.update", false);

            revision.CreatedBy = Security.CurrentUser.GetDBUser(db);
            revision.Problem.LatestRevision = revision;
            db.ProblemRevisions.AddObject(revision);
            db.SaveChanges();
        }

        public static void DeleteRevision(int id)
        {
            Security.Authorize("problem.history.delete", false);
            MooDB db = new MooDB();
            ProblemRevision revision = (from r in db.ProblemRevisions
                                        where r.ID == id
                                        select r).SingleOrDefault<ProblemRevision>();
            if (revision == null) throw new ArgumentException("无此题目版本");
            if (revision.Problem.LatestRevision.ID == revision.ID)
            {
                revision.Problem.LatestRevision = null;
            }
            db.ProblemRevisions.DeleteObject(revision);
            db.SaveChanges();
        }
    }
}
