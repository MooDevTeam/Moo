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
        #region Util
        NameValueCollection QueryParameters
        {
            get
            {
                return WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters;
            }
        }

        void DeleteACEs(MooDB db, Guid obj)
        {
            (from a in db.ACL
             where a.Object == obj
             select a).ToList().ForEach(a => db.ACL.DeleteObject(a));
        }
        #endregion

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
                    throw new NotSupportedException("不支持的题目类型：" + problem.Type);
                }
                Problem newProblem = new Problem()
                {
                    Name = problem.Name,
                    Type = problem.Type,
                    CreateTime = DateTime.Now,
                    CreatedBy = Security.CurrentUser.GetDBUser(db)
                };
                db.Problems.AddObject(newProblem);
                db.SaveChanges();

                db.ACL.AddObject(new ACE()
                {
                    Subject = Security.CurrentUser.ID,
                    Object = newProblem.ID,
                    Function = Security.GetFunction(db, "problem.modify"),
                    Allowed = true
                });

                db.ACL.AddObject(new ACE()
                {
                    Subject = Security.CurrentUser.ID,
                    Object = newProblem.ID,
                    Function = Security.GetFunction(db, "problem.delete"),
                    Allowed = true
                });

                db.SaveChanges();

                return newProblem.ID;
            }
        }

        [OperationContract]
        [WebGet(UriTemplate = "Problems/Count")]
        public int CountProblem()
        {
            string nameContains = QueryParameters["nameContains"] == null ? null : QueryParameters["nameContains"];
            using (MooDB db = new MooDB())
            {
                IQueryable<Problem> problems = db.Problems;
                if (nameContains != null)
                {
                    problems = problems.Where(p => p.Name.Contains(nameContains));
                }

                return problems.Count();
            }
        }

        [OperationContract]
        [WebGet(UriTemplate = "Problems")]
        public List<FullProblem> ListProblem()
        {
            int? skip = QueryParameters["skip"] == null ? null : (int?)int.Parse(QueryParameters["skip"]);
            int? top = QueryParameters["top"] == null ? null : (int?)int.Parse(QueryParameters["top"]);
            string nameContains = QueryParameters["nameContains"] == null ? null : QueryParameters["nameContains"];
            using (MooDB db = new MooDB())
            {
                IQueryable<Problem> problems = db.Problems;

                if (nameContains != null)
                {
                    problems = problems.Where(p => p.Name.Contains(nameContains));
                }

                problems = problems.OrderByDescending(p => p.CreateTime);

                if (skip != null)
                {
                    problems = problems.Skip((int)skip);
                }
                if (top != null)
                {
                    problems = problems.Take((int)top);
                }

                return problems.ToList().Select(p => p.ToFullProblem(db)).ToList();
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

                (from r in db.ProblemRevisions
                 where r.Problem.ID == problem.ID
                 select r).ToList().ForEach(r => DeleteACEs(db, r.ID));

                (from p in db.Posts
                 where p.Problem.ID == problem.ID
                 select p).ToList().ForEach(post =>
                {
                    (from p in db.PostItems
                     where p.Post.ID == post.ID
                     select p).ToList().ForEach(item => DeleteACEs(db, item.ID));
                    DeleteACEs(db, post.ID);
                });

                (from r in db.Records
                 where r.Problem.ID == problem.ID
                 select r).ToList().ForEach(r => DeleteACEs(db, r.ID));

                (from t in db.TestCases
                 where t.Problem.ID == problem.ID
                 select t).ToList().ForEach(t => DeleteACEs(db, t.ID));

                DeleteACEs(db, problem.ID);

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
                        throw new NotSupportedException("不支持的题目类型：" + problem.Type);
                    }
                    theProblem.Type = problem.Type;
                }
                db.SaveChanges();
            }
        }
        #endregion

        #region ProblemRevisions
        [OperationContract]
        [WebGet(UriTemplate = "Problems/{problemID}/Revisions/Count")]
        public int CountProblemRevision(string problemID)
        {
            Guid gproblemID = Guid.Parse(problemID);
            using (MooDB db = new MooDB())
            {
                return (from r in db.ProblemRevisions
                        where r.Problem.ID == gproblemID
                        select r).Count();
            }
        }

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
                                                        orderby r.CreateTime descending
                                                        select r;
                if (skip != null)
                {
                    revisions = revisions.Skip((int)skip);
                }
                if (top != null)
                {
                    revisions = revisions.Take((int)top);
                }

                return revisions.ToList().Select(r => r.ToBriefProblemRevision()).ToList();
            }
        }

        [OperationContract]
        [WebGet(UriTemplate = "Problems/{problemID}/Revisions/{id}")]
        public FullProblemRevision GetProblemRevision(string problemID, string id)
        {
            Guid gid = Guid.Parse(id);
            using (MooDB db = new MooDB())
            {
                ProblemRevision revision = (from r in db.ProblemRevisions
                                            where r.ID == gid
                                            select r).SingleOrDefault<ProblemRevision>();

                if (revision == null) throw new ArgumentException("无此题目版本");

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
                    CreateTime = DateTime.Now
                };
                db.ProblemRevisions.AddObject(problemRevision);
                problem.LatestRevision = problemRevision;
                db.SaveChanges();

                db.ACL.AddObject(new ACE()
                {
                    Subject = Security.CurrentUser.ID,
                    Object = problemRevision.ID,
                    Function = Security.GetFunction(db, "problem.revision.delete"),
                    Allowed = true
                });
                db.SaveChanges();
                return problemRevision.ID;
            }
        }

        [OperationContract]
        [WebInvoke(UriTemplate = "Problems/{problemID}/Revisions/{id}", Method = "DELETE")]
        public void DeleteProblemRevision(string problemID, string id)
        {
            Guid gid = Guid.Parse(id);
            using (MooDB db = new MooDB())
            {
                ProblemRevision revision = (from r in db.ProblemRevisions
                                            where r.ID == gid
                                            select r).SingleOrDefault<ProblemRevision>();
                if (revision == null) throw new ArgumentException("无此题目版本");

                Security.RequirePermission(db, revision.ID, "ProblemRevision", "problem.revision.delete");

                Problem problem = revision.Problem;
                if (problem.LatestRevision.ID == revision.ID)
                {
                    var otherRevisions = from r in db.ProblemRevisions
                                         where r.ID != revision.ID && r.Problem.ID == problem.ID
                                         select r;
                    if (otherRevisions.Any())
                    {
                        DateTime latestTime = otherRevisions.Max(r => r.CreateTime);
                        problem.LatestRevision = otherRevisions.Where(r => r.CreateTime == latestTime).First();
                    }
                    else
                    {
                        problem.LatestRevision = null;
                    }
                }

                DeleteACEs(db, revision.ID);

                db.ProblemRevisions.DeleteObject(revision);
                db.SaveChanges();
            }
        }
        #endregion

        #region Records
        [OperationContract]
        [WebGet(UriTemplate = "Records/Count")]
        public int CountRecord()
        {
            Guid? problemID = QueryParameters["problemID"] == null ? null : (Guid?)Guid.Parse(QueryParameters["problemID"]);
            Guid? userID = QueryParameters["userID"] == null ? null : (Guid?)Guid.Parse(QueryParameters["userID"]);
            Guid? contestID = QueryParameters["contestID"] == null ? null : (Guid?)Guid.Parse(QueryParameters["contestID"]);

            using (MooDB db = new MooDB())
            {
                IQueryable<Record> records = db.Records;
                if (problemID != null)
                {
                    records = records.Where(r => r.Problem.ID == problemID);
                }
                if (userID != null)
                {
                    records = records.Where(r => r.User.ID == userID);
                }
                if (contestID != null)
                {
                    Contest contest = (from c in db.Contests
                                       where c.ID == contestID
                                       select c).SingleOrDefault<Contest>();
                    if (contest == null) throw new ArgumentException("无此比赛");
                    records = records.Where(r => contest.Problem.Contains(r.Problem));
                }
                return records.Count();
            }
        }

        [OperationContract]
        [WebGet(UriTemplate = "Records")]
        public List<BriefRecord> ListRecord()
        {
            int? skip = QueryParameters["skip"] == null ? null : (int?)int.Parse(QueryParameters["skip"]);
            int? top = QueryParameters["top"] == null ? null : (int?)int.Parse(QueryParameters["top"]);
            Guid? problemID = QueryParameters["problemID"] == null ? null : (Guid?)Guid.Parse(QueryParameters["problemID"]);
            Guid? userID = QueryParameters["userID"] == null ? null : (Guid?)Guid.Parse(QueryParameters["userID"]);
            Guid? contestID = QueryParameters["contestID"] == null ? null : (Guid?)Guid.Parse(QueryParameters["contestID"]);

            using (MooDB db = new MooDB())
            {
                IQueryable<Record> records = db.Records;
                if (problemID != null)
                {
                    records = records.Where(r => r.Problem.ID == problemID);
                }
                if (userID != null)
                {
                    records = records.Where(r => r.User.ID == userID);
                }
                if (contestID != null)
                {
                    Contest contest = (from c in db.Contests
                                       where c.ID == contestID
                                       select c).SingleOrDefault<Contest>();
                    if (contest == null) throw new ArgumentException("无此比赛");
                    records = records.Where(r => contest.Problem.Contains(r.Problem));
                }

                records = records.OrderByDescending(r => r.CreateTime);

                if (skip != null)
                {
                    records = records.Skip((int)skip);
                }
                if (top != null)
                {
                    records = records.Take((int)top);
                }

                return records.ToList().Select(r => r.ToBriefRecord(db)).ToList();
            }
        }

        [OperationContract]
        [WebGet(UriTemplate = "Records/{id}")]
        public FullRecord GetRecord(string id)
        {
            Guid gid = Guid.Parse(id);
            using (MooDB db = new MooDB())
            {
                Record record = (from r in db.Records
                                 where r.ID == gid
                                 select r).SingleOrDefault<Record>();
                if (record == null) throw new ArgumentException("无此记录");

                Security.RequirePermission(db, record.ID, "Record", "record.read");

                return record.ToFullRecord(db);
            }
        }

        [OperationContract]
        [WebInvoke(UriTemplate = "Records", Method = "POST")]
        public Guid CreateRecord(FullRecord record)
        {
            if (!new[] { "c", "c++", "pascal", "java", "plaintext" }.Contains(record.Language))
            {
                throw new ArgumentException("不支持的语言：" + record.Language);
            }
            using (MooDB db = new MooDB())
            {
                Problem problem = (from p in db.Problems
                                   where p.ID == record.Problem
                                   select p).SingleOrDefault<Problem>();

                if (problem == null) throw new ArgumentException("无此题目");

                Security.RequirePermission(db, problem.ID, "Problem", "record.create");

                Record newRecord = new Record()
                {
                    Code = record.Code,
                    CreateTime = DateTime.Now,
                    Language = record.Language,
                    Problem = problem,
                    User = Security.CurrentUser.GetDBUser(db),
                };

                problem.SubmissionCount++;
                if (!(from r in db.Records
                      where r.Problem.ID == problem.ID && r.User.ID == Security.CurrentUser.ID
                      select r).Any())
                {
                    problem.SubmissionUser++;
                }

                db.Records.AddObject(newRecord);
                db.SaveChanges();

                db.ACL.AddObject(new ACE()
                {
                    Subject = Security.CurrentUser.ID,
                    Object = newRecord.ID,
                    Function = Security.GetFunction(db, "record.modify"),
                    Allowed = true
                });

                db.ACL.AddObject(new ACE()
                {
                    Subject = Security.CurrentUser.ID,
                    Object = newRecord.ID,
                    Function = Security.GetFunction(db, "record.delete"),
                    Allowed = true
                });

                if (record.PublicCode != null && (bool)record.PublicCode)
                {
                    db.ACL.AddObject(new ACE()
                    {
                        Subject = new SiteRoles(db).Reader.ID,
                        Object = newRecord.ID,
                        Allowed = true,
                        Function = Security.GetFunction(db, "record.code.read"),
                    });
                }
                db.SaveChanges();

                return newRecord.ID;
            }
        }

        [OperationContract]
        [WebInvoke(UriTemplate = "Records/{id}", Method = "PUT")]
        public void ModifyRecord(string id, FullRecord record)
        {
            Guid gid = Guid.Parse(id);
            using (MooDB db = new MooDB())
            {
                Record theRecord = (from r in db.Records
                                    where r.ID == gid
                                    select r).SingleOrDefault<Record>();
                if (record == null) throw new ArgumentException("无此记录");

                Security.RequirePermission(db, theRecord.ID, "Record", "record.modify");

                if (record.PublicCode != null)
                {
                    if ((bool)record.PublicCode)
                    {
                        if ((bool)theRecord.ToBriefRecord(db).PublicCode) throw new ArgumentException("代码早已公开");
                        db.ACL.AddObject(new ACE()
                        {
                            Subject = new SiteRoles(db).Reader.ID,
                            Object = theRecord.ID,
                            Allowed = true,
                            Function = Security.GetFunction(db, "record.code.read")
                        });
                    }
                    else
                    {
                        if (!(bool)theRecord.ToBriefRecord(db).PublicCode) throw new ArgumentException("代码本就私有");
                        db.ACL.DeleteObject((from a in db.ACL
                                             where a.Function.ID == Security.GetFunctionID("record.code.read") && a.Allowed
                                                  && a.Object == theRecord.ID && a.Subject == new SiteRoles(db).Reader.ID
                                             select a).Single());
                    }
                }

                db.SaveChanges();
            }
        }

        [OperationContract]
        [WebInvoke(UriTemplate = "Records/{id}", Method = "DELETE")]
        public void DeleteRecord(string id)
        {
            Guid gid = Guid.Parse(id);

            using (MooDB db = new MooDB())
            {
                Record record = (from r in db.Records
                                 where r.ID == gid
                                 select r).SingleOrDefault<Record>();
                if (record == null) throw new ArgumentException("无此记录");

                Security.RequirePermission(db, record.ID, "Record", "record.delete");

                if (record.JudgeInfo != null)
                {
                    DeleteJudgeInfoScore(db, record, record.JudgeInfo);
                    DeleteACEs(db, record.JudgeInfo.ID);
                }

                DeleteACEs(db, record.ID);

                db.Records.DeleteObject(record);
                db.SaveChanges();
            }
        }
        #endregion

        #region JudgeInfos
        [OperationContract]
        [WebGet(UriTemplate = "Records/{recordID}/JudgeInfo")]
        public FullJudgeInfo GetJudgeInfo(string recordID)
        {
            Guid grecordID = Guid.Parse(recordID);
            using (MooDB db = new MooDB())
            {
                Record record = (from r in db.Records
                                 where r.ID == grecordID
                                 select r).SingleOrDefault<Record>();
                if (record == null) throw new ArgumentException("无此记录");
                if (record.JudgeInfo == null) throw new ArgumentException("记录无测评信息");

                Security.RequirePermission(db, record.JudgeInfo.ID, "JudgeInfo", "record.judgeinfo.read");

                return record.JudgeInfo.ToFullJudgeInfo();
            }
        }

        void DeleteJudgeInfoScore(MooDB db, Record record, JudgeInfo info)
        {
            if (info.Score >= 0)
            {
                var hisRecords = from r in db.Records
                                 where r.User.ID == record.User.ID && r.Problem.ID == record.Problem.ID
                                       && r.JudgeInfo != null && r.JudgeInfo.Score >= 0
                                 select r;
                int oldScore = hisRecords.Max(r => r.JudgeInfo.Score);
                int newScore = (from r in hisRecords
                                where r.ID != record.ID
                                select r.JudgeInfo.Score).DefaultIfEmpty().Max();
                record.User.Score -= oldScore;
                record.Problem.ScoreSum -= oldScore;
                record.User.Score += newScore;
                record.Problem.ScoreSum += newScore;

                if (record.Problem.MaximumScore == info.Score)
                {
                    var problemRecords = from r in db.Records
                                         where r.ID != record.ID
                                               && r.Problem.ID == record.Problem.ID
                                               && r.JudgeInfo != null && r.JudgeInfo.Score >= 0
                                         select r;
                    if (problemRecords.Any())
                    {
                        record.Problem.MaximumScore = problemRecords.Max(r => r.JudgeInfo.Score);
                    }
                    else
                    {
                        record.Problem.MaximumScore = null;
                    }
                }
            }
        }

        [OperationContract]
        [WebInvoke(UriTemplate = "Records/{recordID}/JudgeInfo", Method = "DELETE")]
        public void DeleteJudgeInfo(string recordID)
        {
            Guid grecordID = Guid.Parse(recordID);
            using (MooDB db = new MooDB())
            {
                Record record = (from r in db.Records
                                 where r.ID == grecordID
                                 select r).SingleOrDefault<Record>();
                if (record == null) throw new ArgumentException("无此记录");
                if (record.JudgeInfo == null) throw new ArgumentException("记录无测评信息");

                JudgeInfo info = record.JudgeInfo;

                Security.RequirePermission(db, record.JudgeInfo.ID, "JudgeInfo", "record.judgeinfo.delete");

                DeleteACEs(db, info.ID);
                record.JudgeInfo = null;
                db.JudgeInfos.DeleteObject(info);
                db.SaveChanges();
            }
        }
        #endregion

        #region TestCases
        [OperationContract]
        [WebGet(UriTemplate = "Problems/{problemID}/TestCases/Count")]
        public int CountTestCase(string problemID)
        {
            Guid gproblemID = Guid.Parse(problemID);
            using (MooDB db = new MooDB())
            {
                return (from t in db.TestCases
                        where t.Problem.ID == gproblemID
                        select t).Count();
            }
        }

        [OperationContract]
        [WebGet(UriTemplate = "Problems/{problemID}/TestCases")]
        public int ListTestCase(string problemID)
        {
            int? skip = QueryParameters["skip"] == null ? null : (int?)int.Parse(QueryParameters["skip"]);
            int? top = QueryParameters["top"] == null ? null : (int?)int.Parse(QueryParameters["top"]);
            Guid gproblemID = Guid.Parse(problemID);
            using (MooDB db = new MooDB())
            {
                IQueryable<TestCase> testCases = from t in db.TestCases
                                                 where t.Problem.ID == gproblemID
                                                 select t;
                if (skip != null)
                {
                    testCases = testCases.Skip((int)skip);
                }
                if (top != null)
                {
                    testCases = testCases.Take((int)top);
                }

                return testCases.ToList().Select(t => t.T);
            }
        }

        [OperationContract]
        [WebGet(UriTemplate = "Problems/{problemID}/TestCases/{id}")]
        public FullTestCase GetTestCase(string problemID, string id)
        {
            Guid gid = Guid.Parse(id);
            using (MooDB db = new MooDB())
            {
                TestCase testCase = (from t in db.TestCases
                                     where t.ID == gid
                                     select t).SingleOrDefault<TestCase>();
                if (testCase == null) throw new ArgumentException("无此测试数据");

                Security.RequirePermission(db, testCase.ID, "TestCase", "testcase.read");

                if (testCase is TranditionalTestCase)
                {
                    return ((TranditionalTestCase)testCase).ToFullTranditionalTestCase();
                }
                else if (testCase is SpecialJudgedTestCase)
                {
                    return ((SpecialJudgedTestCase)testCase).ToFullSpecialJudgedTestCase();
                }
                else if (testCase is InteractiveTestCase)
                {
                    return ((InteractiveTestCase)testCase).ToFullInteractiveTestCase();
                }
                else if (testCase is AnswerOnlyTestCase)
                {
                    return ((AnswerOnlyTestCase)testCase).ToFullAnswerOnlyTestCase();
                }
                else
                {
                    throw new NotSupportedException("不支持的测试数据类型");
                }
            }
        }
        #endregion
    }
}
