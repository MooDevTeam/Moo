using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moo.Core.DB;
/*
namespace Moo.Core.Logic
{
    public static class Problems
    {
        public static int Create(FullProblem problem)
        {
            Security.Authorize("problem.create", false);

            if (problem.Type != "Tranditional" && problem.Type != "SpecialJudged" && problem.Type != "Interactive" && problem.Type != "AnswerOnly")
            {
                throw new ArgumentException("不支持的题目类型：" + problem.Type);
            }

            using (MooDB db = new MooDB())
            {
                Problem newProblem = new Problem()
                {
                    AllowTesting = true,
                    Hidden = false,
                    LatestRevision = null,
                    LatestSolution = null,
                    Lock = false,
                    LockPost = false,
                    LockRecord = false,
                    LockSolution = false,
                    LockTestCase = false,
                    MaximumScore = null,
                    ScoreSum = 0,
                    SubmissionCount = 0,
                    SubmissionUser = 0,
                    TestCaseHidden = false,
                    Name = problem.Name,
                    Type = problem.Type
                };
                db.Problems.AddObject(newProblem);
                db.SaveChanges();
                return newProblem.ID;
            }
        }

        public static int Count()
        {
            using (MooDB db = new MooDB())
            {
                return db.Problems.Count();
            }
        }

        public static List<BriefProblem> List(int? start, int? count)
        {
            Security.Authorize("problem.list", true);
            using (MooDB db = new MooDB())
            {
                IQueryable<BriefProblem> problems = from p in db.Problems
                                                    orderby p.ID descending
                                                    select new BriefProblem()
                                                    {
                                                        ID = p.ID,
                                                        Name = p.Name
                                                    };
                if (start != null)
                {
                    problems = problems.Skip((int)start);
                }
                if (count != null)
                {
                    problems = problems.Take((int)count);
                }
                return problems.ToList();
            }
        }

        public static FullProblem Get(Guid id)
        {
            using (MooDB db = new MooDB())
            {
                Problem problem = (from p in db.Problems
                                   where p.ID == id
                                   select p).SingleOrDefault<Problem>();

                if (problem == null) throw new ArgumentException("无此题目");
                return new FullProblem()
                {
                    ID = problem.ID,
                    Name = problem.Name,
                    Type = problem.Type,
                    Lock = problem.Lock,
                    LockPost = problem.LockPost,
                    LockRecord = problem.LockRecord,
                    LockSolution = problem.LockSolution,
                    LockTestCase = problem.LockTestCase,
                    AllowTesting = problem.AllowTesting,
                    Hidden = problem.Hidden,
                    TestCaseHidden = problem.TestCaseHidden,
                    MaximumScore = problem.MaximumScore,
                    ScoreSum = problem.ScoreSum,
                    SubmissionCount = problem.SubmissionCount,
                    SubmissionUser = problem.SubmissionUser,
                    LatestRevision = problem.LatestRevision == null ? null : (int?)problem.LatestRevision.ID,
                    LatestSolution = problem.LatestSolution == null ? null : (int?)problem.LatestSolution.ID
                };
            }
        }

        public static void Update(int id, FullProblem problem)
        {
            Security.Authorize("problem.modify", false);
            if (problem.Type != null && problem.Type != "Tranditional" && problem.Type != "SpecialJudged" && problem.Type != "Interactive" && problem.Type != "AnswerOnly")
            {
                throw new ArgumentException("不支持的题目类型：" + problem.Type);
            }

            using (MooDB db = new MooDB())
            {
                Problem theProblem = (from p in db.Problems
                                      where p.ID == id
                                      select p).SingleOrDefault<Problem>();
                if (theProblem == null) throw new ArgumentException("无此题目");

                if (problem.AllowTesting != null)
                {
                    theProblem.AllowTesting = (bool)problem.AllowTesting;
                }
                if (problem.Hidden != null)
                {
                    theProblem.Hidden = (bool)problem.Hidden;
                }
                if (problem.Lock != null)
                {
                    theProblem.Lock = (bool)problem.Lock;
                }
                if (problem.LockPost != null)
                {
                    theProblem.LockPost = (bool)problem.LockPost;
                }
                if (problem.LockRecord != null)
                {
                    theProblem.LockRecord = (bool)problem.LockRecord;
                }
                if (problem.LockSolution != null)
                {
                    theProblem.LockSolution = (bool)problem.LockSolution;
                }
                if (problem.LockTestCase != null)
                {
                    theProblem.LockTestCase = (bool)problem.LockTestCase;
                }
                if (problem.TestCaseHidden != null)
                {
                    theProblem.TestCaseHidden = (bool)problem.TestCaseHidden;
                }
                if (problem.Name != null)
                {
                    theProblem.Name = problem.Name;
                }
                if (problem.Type != null)
                {
                    theProblem.Type = problem.Type;
                }
                db.SaveChanges();
            }
        }

        public static void Delete(int id)
        {
            Security.Authorize("problem.delete", false);
            using (MooDB db = new MooDB())
            {
                Problem problem = (from p in db.Problems
                                   where p.ID == id
                                   select p).SingleOrDefault<Problem>();
                if (problem == null) throw new ArgumentException("无此题目");

                if (problem.Contest.Any()) throw new InvalidOperationException("尚有比赛使用此题目");

                db.Problems.DeleteObject(problem);
                db.SaveChanges();
            }
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

*/