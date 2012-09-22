using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;
using Moo.Core.DB;
using Moo.Core.Security;
using Moo.Core.Text;

namespace Moo.API.API
{
    [ServiceContract]
    public class JsonAPI
    {
        NameValueCollection QueryParameters
        {
            get
            {
                return WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters;
            }
        }

        #region Test
        [OperationContract]
        [WebGet(UriTemplate = "some/{text}/thing")]
        public string Echo(string text)
        {
            return text;
        }
        [OperationContract]
        public string Debug()
        {
            throw new ArgumentException("没事啊");
        }
        #endregion

        #region Misc
        [OperationContract]
        public string Wiki2HTML(string wiki)
        {
            return wiki;
        }

        [OperationContract]
        public string GenerateDiffHTML(string oldText, string newText)
        {
            return DiffGenerator.Generate(oldText, newText);
        }
        #endregion

        #region Security
        [OperationContract]
        public string Login(string userName, string password)
        {
            return Security.Login(userName, password);
        }

        [OperationContract]
        [WebGet(UriTemplate = "CurrentUser")]
        public BriefUser GetCurrentUser()
        {
            SiteUser currentUser = Security.CurrentUser;
            return new BriefUser()
            {
                ID = currentUser.ID,
                Name = currentUser.Name
            };
        }

        [OperationContract]
        public bool CheckPermission(Guid obj, string type, string permission)
        {
            return Security.CheckPermission(Security.CurrentUser.Subjects, obj, type, permission);
        }

        [OperationContract]
        public void Logout()
        {
            Security.Logout();
        }
        #endregion

        #region Problems
        [OperationContract]
        [WebInvoke(UriTemplate = "Problems", Method = "POST")]
        public Guid CreateProblem(FullProblem problem)
        {
            using (MooDB db = new MooDB())
            {
                Security.RequirePermission(db, Guid.Empty, null, "problem.create");
                if (!new[] { "Tranditional", "SpecialJudged", "Interactive", "AnswerOnly" }.Contains(problem.Type))
                {
                    throw new ArgumentException("不支持的题目类型：" + problem.Type);
                }
                Problem newProblem = new Problem()
                {
                    Name = problem.Name,
                    Type = problem.Type,
                };
                db.Problems.AddObject(newProblem);
                db.SaveChanges();
                return newProblem.ID;
            }
        }

        [OperationContract]
        [WebGet(UriTemplate = "Problems/Count")]
        public int CountProblems()
        {
            using (MooDB db = new MooDB())
            {
                return db.Problems.Count();
            }
        }

        [OperationContract]
        [WebGet(UriTemplate = "Problems")]
        public List<FullProblem> ListProblem()
        {
            int? start = QueryParameters["skip"] == null ? null : (int?)int.Parse(QueryParameters["skip"]);
            int? count = QueryParameters["top"] == null ? null : (int?)int.Parse(QueryParameters["top"]);
            string nameLike = QueryParameters["nameLike"] == null ? null : QueryParameters["nameLike"];
            using (MooDB db = new MooDB())
            {
                IQueryable<Problem> problems = from p in db.Problems
                                               orderby p.ID descending
                                               select p;
                if (start != null)
                {
                    problems = problems.Skip((int)start);
                }
                if (count != null)
                {
                    problems = problems.Take((int)count);
                }
                if (nameLike != null)
                {
                    problems = problems.Where(p => p.Name.Contains(nameLike));
                }
                return problems.ToList().Select(p => p.ToFullProblem(db)).Where(p => Security.CheckPermission(db, (Guid)p.ID, "Problem", "problem.read")).ToList();
            }
        }

        [OperationContract]
        [WebGet(UriTemplate = "Problems/{id}")]
        public FullProblem GetProblem(string id)
        {
            Guid gid = Guid.Parse(id);
            using (MooDB db = new MooDB())
            {
                Problem problem = (from p in db.Problems
                                   where p.ID == gid
                                   select p).SingleOrDefault<Problem>();
                if (problem == null) throw new ArgumentException("无此题目");

                Security.RequirePermission(db, problem.ID, "Problem", "problem.read");

                return problem.ToFullProblem(db);
            }
        }

        [OperationContract]
        [WebInvoke(UriTemplate = "Problems/{id}", Method = "DELETE")]
        public void DeleteProblem(string id)
        {
            Guid gid = Guid.Parse(id);
            using (MooDB db = new MooDB())
            {
                Problem problem = (from p in db.Problems
                                   where p.ID == gid
                                   select p).SingleOrDefault<Problem>();
                if (problem == null) throw new ArgumentException("无此题目");

                Security.RequirePermission(db, problem.ID, "Problem", "problem.delete");

                if (problem.Contest.Any()) throw new InvalidOperationException("尚有比赛使用此题目");

                db.Problems.DeleteObject(problem);
                db.SaveChanges();
            }
        }

        [OperationContract]
        [WebInvoke(UriTemplate = "Problems/{id}", Method = "PUT")]
        public void ModifyProblem(string id, FullProblem problem)
        {
            Guid gid = Guid.Parse(id);
            using (MooDB db = new MooDB())
            {
                Problem theProblem = (from p in db.Problems
                                      where p.ID == gid
                                      select p).SingleOrDefault<Problem>();
                if (theProblem == null) throw new ArgumentException("无此题目");
                Security.RequirePermission(db, theProblem.ID, "Problem", "problem.modify");
                if (problem.Name != null)
                {
                    theProblem.Name = problem.Name;
                }
                if (problem.Type != null)
                {
                    if (!new[] { "Tranditional", "SpecialJudged", "Interactive", "AnswerOnly" }.Contains(problem.Type))
                    {
                        throw new ArgumentException("不支持的题目类型：" + problem.Type);
                    }
                    theProblem.Type = problem.Type;
                }
                db.SaveChanges();
            }
        }
        #endregion

        #region ProblemRevisions
        [OperationContract]
        [WebGet(UriTemplate = "Problems/{problemID}/Revisions")]
        public List<BriefProblemRevision> ListProblemRevision(string problemID)
        {
            int? skip = QueryParameters["skip"] == null ? null : (int?)int.Parse(QueryParameters["skip"]);
            int? top = QueryParameters["top"] == null ? null : (int?)int.Parse(QueryParameters["top"]);
            Guid gproblemID = Guid.Parse(problemID);
            using (MooDB db = new MooDB())
            {
                IQueryable<ProblemRevision> revisions = from r in db.ProblemRevisions
                                                        where r.Problem.ID == gproblemID
                                                        orderby r.ID descending
                                                        select r;
                if (skip != null)
                {
                    revisions = revisions.Skip((int)skip);
                }
                if (top != null)
                {
                    revisions = revisions.Take((int)top);
                }

                return revisions.ToList().Select(r => r.ToBriefProblemRevision()).Where(r => Security.CheckPermission(db, (Guid)r.ID, "ProblemRevision", "problem.revision.read")).ToList();
            }
        }

        [OperationContract]
        [WebGet(UriTemplate = "Problems/{problemID}/Revisions/{id}")]
        public FullProblemRevision GetProblemRevision(string problemID, string id)
        {
            Guid gprolemID = Guid.Parse(problemID);
            Guid gid = Guid.Parse(id);
            using (MooDB db = new MooDB())
            {
                ProblemRevision revision = (from r in db.ProblemRevisions
                                            where r.ID == gid
                                            select r).SingleOrDefault<ProblemRevision>();

                if (revision == null) throw new ArgumentException("无此题目版本");
                if (revision.Problem.ID != gprolemID) throw new ArgumentException("题目与题目版本不匹配");

                Security.RequirePermission(db, revision.ID, "ProblemRevision", "problem.revision.read");

                return revision.ToFullProblemRevision();
            }
        }

        [OperationContract]
        [WebInvoke(UriTemplate = "Problems/{problemID}/Revisions", Method = "POST")]
        public Guid CreateProblemRevision(string problemID, FullProblemRevision revision)
        {
            Guid gproblemID = Guid.Parse(problemID);
            using (MooDB db = new MooDB())
            {
                Problem problem = (from p in db.Problems
                                   where p.ID == gproblemID
                                   select p).SingleOrDefault<Problem>();
                if (problem == null) throw new ArgumentException("无此题目");

                Security.RequirePermission(db, problem.ID, "Problem", "problem.revision.create");

                ProblemRevision problemRevision = new ProblemRevision()
                {
                    CreatedBy = Security.CurrentUser.GetDBUser(db),
                    Content = revision.Content,
                    Problem = problem,
                    Reason = revision.Reason,
                };
                db.ProblemRevisions.AddObject(problemRevision);
                problem.LatestRevision = problemRevision;
                db.SaveChanges();
                return problemRevision.ID;
            }
        }

        [OperationContract]
        [WebInvoke(UriTemplate = "Problem/{problemID}/Revisions/{id}", Method = "DELETE")]
        public void DeleteProblemRevision(string problemID, string id)
        {
            Guid gproblemID = Guid.Parse(problemID);
            Guid gid = Guid.Parse(id);
            using (MooDB db = new MooDB())
            {
                ProblemRevision revision = (from r in db.ProblemRevisions
                                            where r.ID == gproblemID
                                            select r).SingleOrDefault<ProblemRevision>();
                if (revision == null) throw new ArgumentException("无此题目版本");
                if (revision.Problem.ID != gproblemID) throw new ArgumentException("题目与题目版本不匹配");

                Security.RequirePermission(db, revision.ID, "ProblemRevision", "problem.revision.delete");

                if (revision.Problem.LatestRevision.ID == revision.ID)
                {
                    revision.Problem.LatestRevision = null;
                }
                db.ProblemRevisions.DeleteObject(revision);
                db.SaveChanges();
            }
        }
        #endregion

    }
}
