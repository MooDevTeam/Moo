using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.IO;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;
using System.Net.Security;
using Moo.API.DataContracts;
using Moo.Core.DB;
using Moo.Core.Security;
using Moo.Core.IndexAPI;
using Moo.Core.Text;
using Moo.Core.Utility;
namespace Moo.API
{
    [ServiceContract(SessionMode = SessionMode.NotAllowed)]
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.Single)]
    public class JsonAPI
    {
        #region Utility
        NameValueCollection QueryParameters
        {
            get
            {
                return WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters;
            }
        }
        int? OptionalIntParameter(string name)
        {
            return QueryParameters[name] == null ? null : (int?)int.Parse(QueryParameters[name]);
        }
        #endregion

        #region Test
        [OperationContract]
        [WebGet(UriTemplate = "Echo/{text}")]
        public string Echo(string text)
        {
            return text;
        }
        [OperationContract]
        [WebGet]
        public object Debug()
        {
            return Search();
        }
        #endregion

        #region Misc
        [OperationContract]
        public string ParseWiki(string wiki)
        {
            return Moo.Core.Text.WikiParser.Parse(wiki);
        }

        [OperationContract]
        [WebGet(UriTemplate = "MemoryUsage")]
        public long GetMemoryUsage()
        {
            return Process.GetCurrentProcess().WorkingSet64;
        }

        [OperationContract]
        public void GarbageCollect()
        {
            Access.Required(null, null, Function.GarbageCollect);
            lock (typeof(Binary))
            {
                Binary.AutoPop();
            }
            GC.Collect();
        }

        [OperationContract]
        [WebGet(UriTemplate = "Search")]
        public object Search()
        {
            lock (typeof(JsonAPI))
            {
                string text = QueryParameters["text"];
                string type = QueryParameters["type"];
                int? skip = OptionalIntParameter("skip");
                int top = int.Parse(QueryParameters["top"]);
                if (!new IndexInterface().Types.Contains(type))
                    throw new ArgumentException("类型无效");

                using (var search = new Search())
                {
                    var searchResult = search.DoSearch(text, type, top);
                    return searchResult;
                }
            }
        }

        [OperationContract]
        [WebGet(UriTemplate = "IndexStatistics")]
        public object IndexStatistics()
        {
            Access.Required(null, null, Function.ReadIndexStatistics);
            using (var search = new Search())
            {
                Dictionary<string, int> result = new Dictionary<string, int>();
                foreach (var type in new IndexInterface().Types)
                {
                    result.Add(type, search.IndexStatistics(type));
                }
                return result;
            }
        }
        #endregion

        #region Security

        [OperationContract]
        [WebGet(UriTemplate = "PublicKey")]
        public object GetPublicKey()
        {
            string format = QueryParameters["format"];
            if (format == "hex")
            {
                return new
                {
                    Modulus = Converter.ToHexString(RSAHelper.PublicKey.Modulus),
                    Exponent = Converter.ToHexString(RSAHelper.PublicKey.Exponent)
                };
            }
            else
            {
                return new
                {
                    Modulus = Convert.ToBase64String(RSAHelper.PublicKey.Modulus),
                    Exponent = Convert.ToBase64String(RSAHelper.PublicKey.Exponent)
                };
            }
        }

        [OperationContract]
        public string Login(int userID, string password)
        {
            password = Encoding.UTF8.GetString(RSAHelper.Decrypt(Convert.FromBase64String(password)));
            string result = Security.Login(userID, password);
            if (result != null)
            {
                WebSockets.WebSocketsAPIHandler.Kick(userID);
            }
            return result;
        }

        [OperationContract]
        [WebGet(UriTemplate = "CurrentUser")]
        public int GetCurrentUser()
        {
            return Security.CurrentUser.ID;
        }

        [OperationContract]
        public void Logout()
        {
            WebSockets.WebSocketsAPIHandler.Kick(Security.CurrentUser.ID);
            Security.Logout();
        }
        #endregion

        #region Problems
        [OperationContract]
        [WebGet(UriTemplate = "Problems/Count")]
        public int CountProblem()
        {
            string nameContains = QueryParameters["nameContains"];
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
        public object ListProblem()
        {
            int? id = OptionalIntParameter("id");
            int? skip = OptionalIntParameter("skip");
            int? top = OptionalIntParameter("top");
            int? tagID = OptionalIntParameter("tagID");
            string nameContains = QueryParameters["nameContains"];
            string order = QueryParameters["order"];
            string full = QueryParameters["full"];

            using (MooDB db = new MooDB())
            {
                IQueryable<Problem> problems = db.Problems;

                if (id != null)
                {
                    problems = problems.Where(p => p.ID == id);
                }
                if (tagID != null)
                {
                    problems = problems.Where(p => p.Tag.Where(t => t.ID == tagID).Any());
                }
                if (nameContains != null)
                {
                    problems = problems.Where(p => p.Name.Contains(nameContains));
                }

                if (order == "asc")
                {
                    problems = problems.OrderBy(p => p.ID);
                }
                else
                {
                    problems = problems.OrderByDescending(p => p.ID);
                }

                if (skip != null)
                {
                    problems = problems.Skip((int)skip);
                }
                if (top != null)
                {
                    problems = problems.Take((int)top);
                }

                if (full != null)
                {
                    return (from p in problems
                            let myRecords = from r in db.Records
                                            where r.Problem.ID == p.ID && r.User.ID == Security.CurrentUser.ID
                                                  && r.JudgeInfo != null && r.JudgeInfo.Score >= 0
                                            select r
                            let testCases = from t in db.TestCases.OfType<TraditionalTestCase>()
                                            where t.Problem.ID == p.ID
                                            select t.Score
                            let fullScore = testCases.Any() ? (int?)testCases.Sum() : null
                            select new
                            {
                                ID = p.ID,
                                Problem = new
                                {
                                    ID = p.ID,
                                    Name = p.Name,
                                },
                                Type = p.Type,
                                AverageScore = p.JudgeInfoHidden ? null : p.SubmissionUser != 0 ? (double?)(p.ScoreSum / (double)p.SubmissionUser) : null,
                                MyScore = p.JudgeInfoHidden ? null : myRecords.Any() ? (int?)myRecords.Max(r => r.JudgeInfo.Score) : null,
                                FullScore = fullScore,
                                SubmissionTimes = p.SubmissionTimes,
                                SubmissionUser = p.SubmissionUser,
                            }).ToList();
                }
                else
                {
                    return problems.Select(p => new
                    {
                        ID = p.ID,
                        Problem = new
                        {
                            ID = p.ID,
                            Name = p.Name
                        },
                        Type = p.Type
                    }).ToList();
                }
            }
        }

        #region CreateProblem
        [DataContract]
        public class CreateProblemData
        {
            [DataMember]
            public string Name;

            [DataMember]
            public string Type;

            [DataMember]
            public string Content;
        }

        [OperationContract]
        [WebInvoke(UriTemplate = "Problems", Method = "POST")]
        public int CreateProblem(CreateProblemData problem)
        {
            if (!new[] { "Traditional", "SpecialJudged", "Interactive", "AnswerOnly" }.Contains(problem.Type))
            {
                throw new NotSupportedException("不支持的题目类型：" + problem.Type);
            }

            using (MooDB db = new MooDB())
            {
                User currentUser = Security.CurrentUser.GetDBUser(db);
                Problem newProblem = new Problem()
                {
                    Name = problem.Name,
                    Type = problem.Type,
                    CreateTime = DateTime.Now,
                    CreatedBy = currentUser,
                    ArticleLocked = false,
                    EnableTesting = true,
                    Hidden = false,
                    Locked = false,
                    PostLocked = false,
                    RecordLocked = false,
                    TestCaseHidden = false,
                    TestCaseLocked = false,
                };
                Access.Required(db, newProblem, Function.CreateProblem);
                db.Problems.AddObject(newProblem);

                ProblemRevision revision = new ProblemRevision()
                {
                    Content = problem.Content,
                    CreatedBy = currentUser,
                    CreateTime = DateTime.Now,
                    Problem = newProblem,
                    Reason = "创建题目",
                };
                newProblem.LatestRevision = revision;
                Access.Required(db, revision, Function.CreateProblemRevision);
                db.ProblemRevisions.AddObject(revision);

                db.SaveChanges();
                return newProblem.ID;
            }
        }
        #endregion

        [OperationContract]
        [WebGet(UriTemplate = "Problems/{id}")]
        public object GetProblem(string id)
        {
            int iid = int.Parse(id);
            int? revisionID = OptionalIntParameter("revisionID");
            using (MooDB db = new MooDB())
            {
                Problem problem = (from p in db.Problems
                                   where p.ID == iid
                                   select p).SingleOrDefault<Problem>();
                if (problem == null) throw new ArgumentException("无此题目");

                Access.Required(db, problem, Function.ReadProblem);

                ProblemRevision revision;
                if (revisionID == null)
                {
                    revision = problem.LatestRevision;
                }
                else
                {
                    revision = (from r in db.ProblemRevisions
                                where r.ID == revisionID
                                select r).SingleOrDefault<ProblemRevision>();
                    if (revision == null) throw new ArgumentException("题目版本与题目不对应");
                }

                return new
                {
                    ID = problem.ID,
                    Name = problem.Name,
                    CreatedBy = new
                    {
                        ID = problem.CreatedBy.ID,
                        Name = problem.CreatedBy.Name
                    },
                    Content = Access.Check(db, revision, Function.ReadProblemRevision) ? revision.Content : "权限不足，无法查看内容。",
                    CreateTime = problem.CreateTime,
                    Revision = new
                    {
                        CreatedBy = new
                        {
                            ID = revision.CreatedBy.ID,
                            Name = revision.CreatedBy.Name
                        },
                        CreateTime = revision.CreateTime
                    },
                    ArticleLocked = problem.ArticleLocked,
                    EnableTesting = problem.EnableTesting,
                    Hidden = problem.Hidden,
                    Locked = problem.Locked,
                    PostLocked = problem.PostLocked,
                    RecordLocked = problem.RecordLocked,
                    TestCaseHidden = problem.TestCaseHidden,
                    TestCaseLocked = problem.TestCaseLocked,
                    JudgeInfoHidden = problem.JudgeInfoHidden,
                    Type = problem.Type,
                    Tag = problem.Tag.Select(t => new
                    {
                        ID = t.ID,
                        Name = t.Name
                    }).ToList()
                };
            }
        }

        [OperationContract]
        [WebInvoke(UriTemplate = "Problems/{id}", Method = "PUT")]
        public void ModifyProblem(string id, FullProblem problem)
        {
            int iid = int.Parse(id);
            using (MooDB db = new MooDB())
            {
                Problem theProblem = (from p in db.Problems
                                      where p.ID == iid
                                      select p).SingleOrDefault<Problem>();
                if (theProblem == null) throw new ArgumentException("无此题目");

                Access.Required(db, theProblem, Function.ModifyProblem);

                if (problem.Name != null)
                {
                    theProblem.Name = problem.Name;
                }
                if (problem.Type != null)
                {
                    if (!new[] { "Traditional", "SpecialJudged", "Interactive", "AnswerOnly" }.Contains(problem.Type))
                    {
                        throw new NotSupportedException("不支持的题目类型：" + problem.Type);
                    }
                    theProblem.Type = problem.Type;
                }
                if (problem.ArticleLocked != null)
                {
                    theProblem.ArticleLocked = (bool)problem.ArticleLocked;
                }
                if (problem.EnableTesting != null)
                {
                    theProblem.EnableTesting = (bool)problem.EnableTesting;
                }
                if (problem.Hidden != null)
                {
                    theProblem.Hidden = (bool)problem.Hidden;
                }
                if (problem.Locked != null)
                {
                    theProblem.Locked = (bool)problem.Locked;
                }
                if (problem.PostLocked != null)
                {
                    theProblem.PostLocked = (bool)problem.PostLocked;
                }
                if (problem.RecordLocked != null)
                {
                    theProblem.RecordLocked = (bool)problem.RecordLocked;
                }
                if (problem.TestCaseHidden != null)
                {
                    theProblem.TestCaseHidden = (bool)problem.TestCaseHidden;
                }
                if (problem.TestCaseLocked != null)
                {
                    theProblem.TestCaseLocked = (bool)problem.TestCaseLocked;
                }
                if (problem.JudgeInfoHidden != null)
                {
                    theProblem.JudgeInfoHidden = (bool)problem.JudgeInfoHidden;
                }
                db.SaveChanges();
            }
        }

        [OperationContract]
        [WebInvoke(UriTemplate = "Problems/{id}", Method = "DELETE")]
        public void DeleteProblem(string id)
        {
            int iid = int.Parse(id);
            using (MooDB db = new MooDB())
            {
                Problem problem = (from p in db.Problems
                                   where p.ID == iid
                                   select p).SingleOrDefault<Problem>();
                if (problem == null) throw new ArgumentException("无此题目");

                Access.Required(db, problem, Function.DeleteProblem);

                if (problem.Contest.Any()) throw new InvalidOperationException("尚有比赛使用此题目");
                problem.Tag.Clear();
                db.Problems.DeleteObject(problem);
                db.SaveChanges();
            }
        }
        #endregion

        #region Problem Tags
        [OperationContract]
        [WebInvoke(UriTemplate = "Problems/{id}/Tags", Method = "POST")]
        public void CreateProblemTag(string id, int tagID)
        {
            int iid = int.Parse(id);
            using (MooDB db = new MooDB())
            {
                Tag tag = (from t in db.Tags
                           where t.ID == tagID
                           select t).SingleOrDefault<Tag>();
                if (tag == null) throw new ArgumentException("无此标签");

                Problem problem = (from p in db.Problems
                                   where p.ID == iid
                                   select p).SingleOrDefault<Problem>();
                if (problem == null) throw new ArgumentException("无此题目");

                Access.Check(db, problem, Function.ModifyProblem);

                problem.Tag.Add(tag);
                db.SaveChanges();
            }
        }

        [OperationContract]
        [WebInvoke(UriTemplate = "Problems/{problemID}/Tags/{id}", Method = "DELETE")]
        public void DeleteProblemTag(string problemID, string id)
        {
            int iproblemID = int.Parse(problemID);
            int iid = int.Parse(id);
            using (MooDB db = new MooDB())
            {
                Problem problem = (from p in db.Problems
                                   where p.ID == iproblemID
                                   select p).SingleOrDefault<Problem>();
                if (problem == null) throw new ArgumentException("无此题目");

                Tag tag = (from t in problem.Tag
                           where t.ID == iid
                           select t).SingleOrDefault<Tag>();
                if (tag == null) throw new ArgumentException("无此标签");

                Access.Check(db, problem, Function.ModifyProblem);

                problem.Tag.Remove(tag);
                db.SaveChanges();
            }
        }
        #endregion

        #region ProblemRevisions
        [OperationContract]
        [WebGet(UriTemplate = "Problems/{problemID}/Revisions/Count")]
        public int CountProblemRevision(string problemID)
        {
            int iproblemID = int.Parse(problemID);
            using (MooDB db = new MooDB())
            {
                return (from r in db.ProblemRevisions
                        where r.Problem.ID == iproblemID
                        select r).Count();
            }
        }

        [OperationContract]
        [WebGet(UriTemplate = "Problems/{problemID}/Revisions")]
        public object ListProblemRevision(string problemID)
        {
            int? skip = QueryParameters["skip"] == null ? null : (int?)int.Parse(QueryParameters["skip"]);
            int? top = QueryParameters["top"] == null ? null : (int?)int.Parse(QueryParameters["top"]);
            int iproblemID = int.Parse(problemID);
            using (MooDB db = new MooDB())
            {
                IQueryable<ProblemRevision> revisions = from r in db.ProblemRevisions
                                                        where r.Problem.ID == iproblemID
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

                return revisions.Select(r => new
                {
                    ID = r.ID,
                    CreateTime = r.CreateTime,
                    CreatedBy = new
                    {
                        ID = r.CreatedBy.ID,
                        Name = r.CreatedBy.Name
                    },
                    Reason = r.Reason,
                }).ToList();
            }
        }

        [OperationContract]
        [WebInvoke(UriTemplate = "Problems/{problemID}/Revisions", Method = "POST")]
        public int CreateProblemRevision(string problemID, FullProblemRevision revision)
        {
            int iproblemID = int.Parse(problemID);
            using (MooDB db = new MooDB())
            {
                Problem problem = (from p in db.Problems
                                   where p.ID == iproblemID
                                   select p).SingleOrDefault<Problem>();
                if (problem == null) throw new ArgumentException("无此题目");

                ProblemRevision problemRevision = new ProblemRevision()
                {
                    CreatedBy = Security.CurrentUser.GetDBUser(db),
                    Content = revision.Content,
                    Problem = problem,
                    Reason = revision.Reason,
                    CreateTime = DateTime.Now
                };
                problem.LatestRevision = problemRevision;

                Access.Required(db, problemRevision, Function.CreateProblemRevision);

                db.ProblemRevisions.AddObject(problemRevision);
                db.SaveChanges();
                return problemRevision.ID;
            }
        }

        [OperationContract]
        [WebInvoke(UriTemplate = "Problems/{problemID}/Revisions/{id}", Method = "DELETE")]
        public void DeleteProblemRevision(string problemID, string id)
        {
            int iid = int.Parse(id);
            using (MooDB db = new MooDB())
            {
                ProblemRevision revision = (from r in db.ProblemRevisions
                                            where r.ID == iid
                                            select r).SingleOrDefault<ProblemRevision>();
                if (revision == null) throw new ArgumentException("无此题目版本");

                Access.Required(db, revision, Function.DeleteProblemRevision);

                Problem problem = revision.Problem;
                if (problem.LatestRevision.ID == revision.ID)
                {
                    throw new InvalidOperationException("不可删除最新版本");
                }

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
            int? problemID = QueryParameters["problemID"] == null ? null : (int?)int.Parse(QueryParameters["problemID"]);
            int? userID = QueryParameters["userID"] == null ? null : (int?)int.Parse(QueryParameters["userID"]);
            int? contestID = QueryParameters["contestID"] == null ? null : (int?)int.Parse(QueryParameters["contestID"]);

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
        public object ListRecord()
        {
            int? id = OptionalIntParameter("id");
            int? skip = QueryParameters["skip"] == null ? null : (int?)int.Parse(QueryParameters["skip"]);
            int? top = QueryParameters["top"] == null ? null : (int?)int.Parse(QueryParameters["top"]);
            int? problemID = QueryParameters["problemID"] == null ? null : (int?)int.Parse(QueryParameters["problemID"]);
            int? userID = QueryParameters["userID"] == null ? null : (int?)int.Parse(QueryParameters["userID"]);
            int? contestID = QueryParameters["contestID"] == null ? null : (int?)int.Parse(QueryParameters["contestID"]);

            using (MooDB db = new MooDB())
            {
                IQueryable<Record> records = db.Records;
                if (id != null)
                {
                    records = records.Where(r => r.ID == id);
                }
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
                    records = from r in records
                              where r.Problem.Contest.Where(c => c.ID == contest.ID).Any()
                                 && r.User.Contest.Where(c => c.ID == contest.ID).Any()
                                 && r.CreateTime >= contest.StartTime && r.CreateTime <= contest.EndTime
                              select r;
                }

                records = records.OrderByDescending(r => r.ID);

                if (skip != null)
                {
                    records = records.Skip((int)skip);
                }
                if (top != null)
                {
                    records = records.Take((int)top);
                }

                return records.ToList().Select(r => new
                {
                    ID = r.ID,
                    CreateTime = r.CreateTime,
                    Problem = new
                    {
                        ID = r.Problem.ID,
                        Name = r.Problem.Name
                    },
                    User = new
                    {
                        ID = r.User.ID,
                        Name = r.User.Name
                    },
                    Language = r.Language,
                    Score = r.JudgeInfo == null ? null : Access.Check(db, r.JudgeInfo, Function.ReadJudgeInfo) ? (int?)r.JudgeInfo.Score : (int?)-2
                }).ToList();
            }
        }

        [OperationContract]
        [WebGet(UriTemplate = "Records/{id}")]
        public object GetRecord(string id)
        {
            int iid = int.Parse(id);
            using (MooDB db = new MooDB())
            {
                Record record = (from r in db.Records
                                 where r.ID == iid
                                 select r).SingleOrDefault<Record>();
                if (record == null) throw new ArgumentException("无此记录");

                Access.Required(db, record, Function.ReadRecord);

                return new
                {
                    ID = record.ID,
                    Problem = new
                    {
                        ID = record.Problem.ID,
                        Name = record.Problem.Name,
                    },
                    User = new
                    {
                        ID = record.User.ID,
                        Name = record.User.Name
                    },
                    CreateTime = record.CreateTime,
                    Code = Access.Check(db, record, Function.ReadRecordCode) ? record.Code : null,
                    CodeLength = record.Code.Length,
                    Score = record.JudgeInfo == null ? null : Access.Check(db, record.JudgeInfo, Function.ReadJudgeInfo) ? (int?)record.JudgeInfo.Score : (int?)-2,
                    JudgeInfo = record.JudgeInfo == null || !Access.Check(db, record.JudgeInfo, Function.ReadJudgeInfo) ? null : record.JudgeInfo.Info,
                    Language = record.Language,
                    PublicCode = record.PublicCode
                };
            }
        }

        [OperationContract]
        [WebInvoke(UriTemplate = "Records", Method = "POST")]
        public int CreateRecord(FullRecord record)
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

                User currentUser = Security.CurrentUser.GetDBUser(db);
                Record newRecord = new Record()
                {
                    Code = record.Code,
                    CreateTime = DateTime.Now,
                    Language = record.Language,
                    Problem = problem,
                    PublicCode = (bool)record.PublicCode,
                    User = currentUser,
                };

                problem.SubmissionTimes++;
                if (!(from r in db.Records
                      where r.Problem.ID == problem.ID && r.User.ID == Security.CurrentUser.ID
                      select r).Any())
                {
                    problem.SubmissionUser++;
                }

                Access.Required(db, newRecord, Function.CreateRecord);

                db.Records.AddObject(newRecord);
                db.SaveChanges();
                return newRecord.ID;
            }
        }

        [OperationContract]
        [WebInvoke(UriTemplate = "Records/{id}", Method = "PUT")]
        public void ModifyRecord(string id, FullRecord record)
        {
            int iid = int.Parse(id);
            using (MooDB db = new MooDB())
            {
                Record theRecord = (from r in db.Records
                                    where r.ID == iid
                                    select r).SingleOrDefault<Record>();
                if (record == null) throw new ArgumentException("无此记录");

                Access.Required(db, theRecord, Function.ModifyRecord);

                if (record.PublicCode != null)
                {
                    theRecord.PublicCode = (bool)record.PublicCode;
                }

                db.SaveChanges();
            }
        }

        [OperationContract]
        [WebInvoke(UriTemplate = "Records/{id}", Method = "DELETE")]
        public void DeleteRecord(string id)
        {
            int iid = int.Parse(id);

            using (MooDB db = new MooDB())
            {
                Record record = (from r in db.Records
                                 where r.ID == iid
                                 select r).SingleOrDefault<Record>();
                if (record == null) throw new ArgumentException("无此记录");

                Access.Required(db, record, Function.DeleteRecord);

                if (record.JudgeInfo != null)
                {
                    DeleteJudgeInfoScore(db, record, record.JudgeInfo);
                }

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
            int grecordID = int.Parse(recordID);
            using (MooDB db = new MooDB())
            {
                Record record = (from r in db.Records
                                 where r.ID == grecordID
                                 select r).SingleOrDefault<Record>();
                if (record == null) throw new ArgumentException("无此记录");
                if (record.JudgeInfo == null) throw new ArgumentException("记录无测评信息");

                Access.Required(db, record, Function.ReadRecord);

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
            int irecordID = int.Parse(recordID);
            using (MooDB db = new MooDB())
            {
                Record record = (from r in db.Records
                                 where r.ID == irecordID
                                 select r).SingleOrDefault<Record>();
                if (record == null) throw new ArgumentException("无此记录");
                //Omit
                if (record.JudgeInfo == null) return;

                JudgeInfo info = record.JudgeInfo;

                Access.Required(db, info, Function.DeleteJudgeInfo);

                CreateMessage(new CreateMessageData
                {
                    ToID = record.User.ID,
                    Content = "我对您的[Record:" + record.ID + "]进行了重测。\r\n"
                    + "原始得分为*" + info.Score + "*。\r\n"
                    + "原始测评信息为\r\n" + info.Info
                });

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
            int iproblemID = int.Parse(problemID);
            using (MooDB db = new MooDB())
            {
                return (from t in db.TestCases
                        where t.Problem.ID == iproblemID
                        select t).Count();
            }
        }

        [OperationContract]
        [WebGet(UriTemplate = "Problems/{problemID}/TestCases")]
        public object ListTestCase(string problemID)
        {
            int? skip = QueryParameters["skip"] == null ? null : (int?)int.Parse(QueryParameters["skip"]);
            int? top = QueryParameters["top"] == null ? null : (int?)int.Parse(QueryParameters["top"]);
            int iproblemID = int.Parse(problemID);
            using (MooDB db = new MooDB())
            {
                IQueryable<TestCase> testCases = from t in db.TestCases
                                                 where t.Problem.ID == iproblemID
                                                 orderby t.ID
                                                 select t;
                if (skip != null)
                {
                    testCases = testCases.Skip((int)skip);
                }
                if (top != null)
                {
                    testCases = testCases.Take((int)top);
                }

                return testCases.ToList().Select(t => new
                {
                    ID = t.ID,
                    CreatedBy = new
                    {
                        ID = t.CreatedBy.ID,
                        Name = t.CreatedBy.Name
                    },
                    CreateTime = t.CreateTime,
                    Type = t.GetType().Name
                }).ToList();
            }
        }

        const int PREVIEW_BYTES = 100;
        [OperationContract]
        [WebGet(UriTemplate = "Problems/{problemID}/TestCases/{id}")]
        public object GetTestCase(string problemID, string id)
        {
            int iid = int.Parse(id);
            using (MooDB db = new MooDB())
            {
                TestCase testCase = (from t in db.TestCases
                                     where t.ID == iid
                                     select t).SingleOrDefault<TestCase>();
                if (testCase == null) throw new ArgumentException("无此测试数据");

                Access.Required(db, testCase, Function.ReadTestCase);

                if (testCase is TraditionalTestCase)
                {
                    TraditionalTestCase asTraditional = testCase as TraditionalTestCase;
                    return new
                    {
                        ID = asTraditional.ID,
                        Problem = new
                        {
                            ID = asTraditional.Problem.ID,
                            Name = asTraditional.Problem.Name
                        },
                        CreatedBy = new
                        {
                            ID = asTraditional.CreatedBy.ID,
                            Name = asTraditional.CreatedBy.Name
                        },
                        CreateTime = asTraditional.CreateTime,
                        Type = "TraditionalTestCase",
                        TimeLimit = asTraditional.TimeLimit,
                        MemoryLimit = asTraditional.MemoryLimit,
                        Score = asTraditional.Score,
                        Input = new
                        {
                            Preview = Encoding.Default.GetString(asTraditional.Input, 0, Math.Min(asTraditional.Input.Length, PREVIEW_BYTES)),
                            BlobID = Binary.Add(asTraditional.Input, "TestCase" + asTraditional.ID + "Input")
                        },
                        Answer = new
                        {
                            Preview = Encoding.Default.GetString(asTraditional.Answer, 0, Math.Min(asTraditional.Answer.Length, PREVIEW_BYTES)),
                            BlobID = Binary.Add(asTraditional.Answer, "TestCase" + asTraditional.ID + "Answer")
                        },
                    };
                }
                else if (testCase is SpecialJudgedTestCase)
                {
                    SpecialJudgedTestCase asSpecialJudged = testCase as SpecialJudgedTestCase;
                    return new
                    {
                        ID = asSpecialJudged.ID,
                        Problem = new
                        {
                            ID = asSpecialJudged.Problem.ID,
                            Name = asSpecialJudged.Problem.Name
                        },
                        CreatedBy = new
                        {
                            ID = asSpecialJudged.CreatedBy.ID,
                            Name = asSpecialJudged.CreatedBy.Name
                        },
                        CreateTime = asSpecialJudged.CreateTime,
                        Type = "SpecialJudgedTestCase",
                        TimeLimit = asSpecialJudged.TimeLimit,
                        MemoryLimit = asSpecialJudged.MemoryLimit,
                        Input = new
                        {
                            Preview = Encoding.Default.GetString(asSpecialJudged.Input, 0, Math.Min(asSpecialJudged.Input.Length, PREVIEW_BYTES)),
                            BlobID = Binary.Add(asSpecialJudged.Input, "TestCase" + asSpecialJudged.ID + "Input")
                        },
                        Answer = new
                        {
                            Preview = Encoding.Default.GetString(asSpecialJudged.Answer, 0, Math.Min(asSpecialJudged.Answer.Length, PREVIEW_BYTES)),
                            BlobID = Binary.Add(asSpecialJudged.Answer, "TestCase" + asSpecialJudged.ID + "Answer")
                        },
                        Judger = new
                        {
                            ID = asSpecialJudged.Judger.ID,
                            Name = asSpecialJudged.Judger.Name
                        }
                    };
                }
                else if (testCase is InteractiveTestCase)
                {
                    InteractiveTestCase asInteractive = testCase as InteractiveTestCase;
                    return new
                    {
                        ID = asInteractive.ID,
                        Problem = new
                        {
                            ID = asInteractive.Problem.ID,
                            Name = asInteractive.Problem.Name
                        },
                        CreatedBy = new
                        {
                            ID = asInteractive.CreatedBy.ID,
                            Name = asInteractive.CreatedBy.Name
                        },
                        CreateTime = asInteractive.CreateTime,
                        Type = "InteractiveTestCase",
                        TimeLimit = asInteractive.TimeLimit,
                        MemoryLimit = asInteractive.MemoryLimit,
                        TestData = new
                        {
                            Preview = Encoding.Default.GetString(asInteractive.TestData, 0, Math.Min(asInteractive.TestData.Length, PREVIEW_BYTES)),
                            BlobID = Binary.Add(asInteractive.TestData, "TestCase" + asInteractive.ID + "TestData")
                        },
                        Invoker = new
                        {
                            ID = asInteractive.Invoker.ID,
                            Name = asInteractive.Invoker.Name
                        }
                    };
                }
                else if (testCase is AnswerOnlyTestCase)
                {
                    AnswerOnlyTestCase asAnswerOnly = testCase as AnswerOnlyTestCase;
                    return new
                    {
                        ID = asAnswerOnly.ID,
                        Problem = new
                        {
                            ID = asAnswerOnly.Problem.ID,
                            Name = asAnswerOnly.Problem.Name
                        },
                        CreatedBy = new
                        {
                            ID = asAnswerOnly.CreatedBy.ID,
                            Name = asAnswerOnly.CreatedBy.Name
                        },
                        CreateTime = asAnswerOnly.CreateTime,
                        Type = "AnswerOnlyTestCase",
                        TestData = new
                        {
                            Preview = Encoding.Default.GetString(asAnswerOnly.TestData, 0, Math.Min(asAnswerOnly.TestData.Length, PREVIEW_BYTES)),
                            BlobID = Binary.Add(asAnswerOnly.TestData, "TestCase" + asAnswerOnly.ID + "TestData")
                        },
                        Judger = new
                        {
                            ID = asAnswerOnly.Judger.ID,
                            Name = asAnswerOnly.Judger.Name
                        }
                    };
                }
                else
                {
                    throw new NotSupportedException("不支持的测试数据类型");
                }
            }
        }

        #region CreateTestCase
        #region DataContracts
        [DataContract]
        [KnownType(typeof(CreateTraditionalTestCaseData))]
        [KnownType(typeof(CreateSpecialJudgedTestCaseData))]
        [KnownType(typeof(CreateInteractiveTestCaseData))]
        [KnownType(typeof(CreateAnswerOnlyTestCaseData))]
        public abstract class CreateTestCaseData
        {
            public abstract TestCase ToTestCase(MooDB db);
        }

        [DataContract]
        public class CreateTraditionalTestCaseData : CreateTestCaseData
        {
            [DataMember]
            public Guid InputBlobID;

            [DataMember]
            public Guid AnswerBlobID;

            [DataMember]
            public int Score;

            [DataMember]
            public int TimeLimit;

            [DataMember]
            public int MemoryLimit;

            public override TestCase ToTestCase(MooDB db)
            {
                if (!Binary.Has(InputBlobID))
                    throw new ArgumentException("输入编号无效");
                if (!Binary.Has(AnswerBlobID))
                    throw new ArgumentException("答案编号无效");
                return new TraditionalTestCase
                {
                    Answer = Binary.Get(AnswerBlobID, true),
                    Input = Binary.Get(InputBlobID, true),
                    MemoryLimit = MemoryLimit,
                    Score = Score,
                    TimeLimit = TimeLimit
                };
            }
        }

        [DataContract]
        public class CreateSpecialJudgedTestCaseData : CreateTestCaseData
        {
            [DataMember]
            public Guid AnswerBlobID;

            [DataMember]
            public Guid InputBlobID;

            [DataMember]
            public int MemoryLimit;

            [DataMember]
            public int TimeLimit;

            [DataMember]
            public int JudgerID;

            public override TestCase ToTestCase(MooDB db)
            {
                if (!Binary.Has(AnswerBlobID))
                    throw new ArgumentException("答案编号无效");
                if (!Binary.Has(InputBlobID))
                    throw new ArgumentException("输入编号无效");
                UploadedFile judger = (from f in db.UploadedFiles
                                       where f.ID == JudgerID
                                       select f).SingleOrDefault<UploadedFile>();
                if (judger == null) throw new ArgumentException("无此文件");

                return new SpecialJudgedTestCase
                {
                    Answer = Binary.Get(AnswerBlobID, true),
                    Input = Binary.Get(InputBlobID, true),
                    Judger = judger,
                    MemoryLimit = MemoryLimit,
                    TimeLimit = TimeLimit
                };
            }
        }

        [DataContract]
        public class CreateInteractiveTestCaseData : CreateTestCaseData
        {
            [DataMember]
            public Guid TestDataBlobID;

            [DataMember]
            public int TimeLimit;

            [DataMember]
            public int MemoryLimit;

            [DataMember]
            public int InvokerID;

            public override TestCase ToTestCase(MooDB db)
            {
                if (!Binary.Has(TestDataBlobID))
                    throw new ArgumentException("测试资料编号无效");
                UploadedFile invoker = (from f in db.UploadedFiles
                                        where f.ID == InvokerID
                                        select f).SingleOrDefault<UploadedFile>();
                if (invoker == null) throw new ArgumentException("无此文件");

                return new InteractiveTestCase
                {
                    Invoker = invoker,
                    MemoryLimit = MemoryLimit,
                    TestData = Binary.Get(TestDataBlobID, true),
                    TimeLimit = TimeLimit,
                };
            }
        }

        [DataContract]
        public class CreateAnswerOnlyTestCaseData : CreateTestCaseData
        {
            [DataMember]
            public Guid TestDataBlobID;

            [DataMember]
            public int JudgerID;

            public override TestCase ToTestCase(MooDB db)
            {
                if (!Binary.Has(TestDataBlobID))
                    throw new ArgumentException("测试资料编号无效");
                UploadedFile judger = (from f in db.UploadedFiles
                                       where f.ID == JudgerID
                                       select f).SingleOrDefault<UploadedFile>();
                if (judger == null) throw new ArgumentException("无此文件");

                return new AnswerOnlyTestCase
                {
                    Judger = judger,
                    TestData = Binary.Get(TestDataBlobID, true)
                };
            }
        }
        #endregion

        [OperationContract]
        [WebInvoke(UriTemplate = "Problems/{problemID}/TestCases", Method = "POST")]
        public int CreateTestCase(string problemID, CreateTestCaseData testCase)
        {
            int iproblemID = int.Parse(problemID);
            using (MooDB db = new MooDB())
            {
                Problem problem = (from p in db.Problems
                                   where p.ID == iproblemID
                                   select p).SingleOrDefault<Problem>();
                if (problem == null) throw new ArgumentException("无此题目");

                TestCase newTestCase = testCase.ToTestCase(db);
                newTestCase.CreatedBy = Security.CurrentUser.GetDBUser(db);
                newTestCase.Problem = problem;
                newTestCase.CreateTime = DateTime.Now;

                Access.Required(db, newTestCase, Function.CreateTestCase);

                db.TestCases.AddObject(newTestCase);
                db.SaveChanges();
                return newTestCase.ID;
            }
        }

        [OperationContract]
        [WebInvoke(UriTemplate = "Problems/{problemID}/TestCases/Traditional", Method = "POST")]
        public int CreateTraditionalTestCase(string problemID, CreateTraditionalTestCaseData testCase)
        {
            return CreateTestCase(problemID, testCase);
        }

        [OperationContract]
        [WebInvoke(UriTemplate = "Problems/{problemID}/TestCases/SpecialJudged", Method = "POST")]
        public int CreateSpecialJudgedTestCase(string problemID, CreateSpecialJudgedTestCaseData testCase)
        {
            return CreateTestCase(problemID, testCase);
        }

        [OperationContract]
        [WebInvoke(UriTemplate = "Problems/{problemID}/TestCases/Interactive", Method = "POST")]
        public int CreateInteractiveTestCase(string problemID, CreateInteractiveTestCaseData testCase)
        {
            return CreateTestCase(problemID, testCase);
        }

        [OperationContract]
        [WebInvoke(UriTemplate = "Problems/{problemID}/TestCases/AnswerOnly", Method = "POST")]
        public int CreateAnswerOnlyTestCase(string problemID, CreateAnswerOnlyTestCaseData testCase)
        {
            return CreateTestCase(problemID, testCase);
        }
        #endregion

        #region ModifyTestCase
        [OperationContract]
        [WebInvoke(UriTemplate = "Problems/{problemID}/TestCases/{id}", Method = "PUT")]
        public void ModifyTestCase(string problemID, string id, FullTestCase testCase)
        {
            int iid = int.Parse(id);
            using (MooDB db = new MooDB())
            {
                TestCase theTestCase = (from t in db.TestCases
                                        where t.ID == iid
                                        select t).SingleOrDefault<TestCase>();
                if (theTestCase == null) throw new ArgumentException("无此测试数据");

                Access.Required(db, theTestCase, Function.ModifyTestCase);

                if (theTestCase is TraditionalTestCase && testCase is FullTraditionalTestCase)
                {
                    TraditionalTestCase oldT = theTestCase as TraditionalTestCase;
                    FullTraditionalTestCase newT = testCase as FullTraditionalTestCase;
                    if (newT.Answer != null)
                    {
                        oldT.Answer = newT.Answer;
                    }
                    if (newT.Input != null)
                    {
                        oldT.Input = newT.Input;
                    }
                    if (newT.MemoryLimit != null)
                    {
                        oldT.MemoryLimit = (int)newT.MemoryLimit;
                    }
                    if (newT.TimeLimit != null)
                    {
                        oldT.TimeLimit = (int)newT.TimeLimit;
                    }
                    if (newT.Score != null)
                    {
                        oldT.Score = (int)newT.Score;
                    }
                }
                else if (theTestCase is SpecialJudgedTestCase && testCase is FullSpecialJudgedTestCase)
                {
                    SpecialJudgedTestCase oldT = theTestCase as SpecialJudgedTestCase;
                    FullSpecialJudgedTestCase newT = testCase as FullSpecialJudgedTestCase;
                    if (newT.Answer != null)
                    {
                        oldT.Answer = newT.Answer;
                    }
                    if (newT.Input != null)
                    {
                        oldT.Input = newT.Input;
                    }
                    if (newT.MemoryLimit != null)
                    {
                        oldT.MemoryLimit = (int)newT.MemoryLimit;
                    }
                    if (newT.TimeLimit != null)
                    {
                        oldT.TimeLimit = (int)newT.TimeLimit;
                    }
                    if (newT.Judger != null)
                    {
                        UploadedFile judger = (from f in db.UploadedFiles
                                               where f.ID == newT.Judger
                                               select f).SingleOrDefault<UploadedFile>();
                        if (judger == null) throw new ArgumentException("无此文件");
                        oldT.Judger = judger;
                    }
                }
                else if (theTestCase is InteractiveTestCase && testCase is FullInteractiveTestCase)
                {
                    InteractiveTestCase oldT = theTestCase as InteractiveTestCase;
                    FullInteractiveTestCase newT = testCase as FullInteractiveTestCase;
                    if (newT.TestData != null)
                    {
                        oldT.TestData = newT.TestData;
                    }
                    if (newT.MemoryLimit != null)
                    {
                        oldT.MemoryLimit = (int)newT.MemoryLimit;
                    }
                    if (newT.TimeLimit != null)
                    {
                        oldT.TimeLimit = (int)newT.TimeLimit;
                    }
                    if (newT.Invoker != null)
                    {
                        UploadedFile invoker = (from f in db.UploadedFiles
                                                where f.ID == newT.Invoker
                                                select f).SingleOrDefault<UploadedFile>();
                        if (invoker == null) throw new ArgumentException("无此文件");
                        oldT.Invoker = invoker;
                    }
                }
                else if (theTestCase is AnswerOnlyTestCase && testCase is FullAnswerOnlyTestCase)
                {
                    AnswerOnlyTestCase oldT = theTestCase as AnswerOnlyTestCase;
                    FullAnswerOnlyTestCase newT = testCase as FullAnswerOnlyTestCase;
                    if (newT.TestData != null)
                    {
                        oldT.TestData = newT.TestData;
                    }
                    if (newT.Judger != null)
                    {
                        UploadedFile judger = (from f in db.UploadedFiles
                                               where f.ID == newT.Judger
                                               select f).SingleOrDefault<UploadedFile>();
                        if (judger == null) throw new ArgumentException("无此文件");
                        oldT.Judger = judger;
                    }
                }
                else
                {
                    throw new InvalidOperationException("类型未知或不匹配");
                }
                db.SaveChanges();
            }
        }

        [OperationContract]
        [WebInvoke(UriTemplate = "Problems/{problemID}/TestCases/Traditional/{id}", Method = "PUT")]
        public void ModifyTraditionalTestCase(string problemID, string id, FullTraditionalTestCase testCase)
        {
            ModifyTestCase(problemID, id, testCase);
        }

        [OperationContract]
        [WebInvoke(UriTemplate = "Problems/{problemID}/TestCases/SpecialJudged/{id}", Method = "PUT")]
        public void ModifySpecialJudgedTestCase(string problemID, string id, FullSpecialJudgedTestCase testCase)
        {
            ModifyTestCase(problemID, id, testCase);
        }

        [OperationContract]
        [WebInvoke(UriTemplate = "Problems/{problemID}/TestCases/Interactive/{id}", Method = "PUT")]
        public void ModifyInteractiveTestCase(string problemID, string id, FullInteractiveTestCase testCase)
        {
            ModifyTestCase(problemID, id, testCase);
        }

        [OperationContract]
        [WebInvoke(UriTemplate = "Problems/{problemID}/TestCases/AnswerOnly/{id}", Method = "PUT")]
        public void ModifyAnswerOnlyTestCase(string problemID, string id, FullAnswerOnlyTestCase testCase)
        {
            ModifyTestCase(problemID, id, testCase);
        }
        #endregion

        [OperationContract]
        [WebInvoke(UriTemplate = "Problems/{problemID}/TestCases/{id}", Method = "DELETE")]
        public void DeleteTestCase(string problemID, string id)
        {
            int iid = int.Parse(id);
            using (MooDB db = new MooDB())
            {
                TestCase testCase = (from t in db.TestCases
                                     where t.ID == iid
                                     select t).SingleOrDefault<TestCase>();
                if (testCase == null) throw new ArgumentException("无此测试数据");

                Access.Required(db, testCase, Function.DeleteTestCase);

                db.TestCases.DeleteObject(testCase);
                db.SaveChanges();
            }
        }
        #endregion

        #region User
        [OperationContract]
        [WebGet(UriTemplate = "Users/Count")]
        public int CountUser()
        {
            string nameContains = QueryParameters["nameContains"];
            using (MooDB db = new MooDB())
            {
                IQueryable<User> users = db.Users;
                if (nameContains != null)
                {
                    users = users.Where(u => u.Name.Contains(nameContains));
                }
                return users.Count();
            }
        }

        [OperationContract]
        [WebGet(UriTemplate = "Users/ByName")]
        public int? GetUserByName()
        {
            string name = QueryParameters["name"];
            using (MooDB db = new MooDB())
            {
                User user = (from u in db.Users
                             where u.Name == name
                             select u).SingleOrDefault<User>();
                return user == null ? null : (int?)user.ID;
            }
        }

        [OperationContract]
        [WebGet(UriTemplate = "Users")]
        public object ListUser()
        {
            int? skip = QueryParameters["skip"] == null ? null : (int?)int.Parse(QueryParameters["skip"]);
            int? top = QueryParameters["top"] == null ? null : (int?)int.Parse(QueryParameters["top"]);
            string nameContains = QueryParameters["nameContains"];
            using (MooDB db = new MooDB())
            {
                IQueryable<User> users = db.Users;
                if (nameContains != null)
                {
                    users = users.Where(u => u.Name.Contains(nameContains));
                }
                users = users.OrderByDescending(u => u.Score);
                if (skip != null)
                {
                    users = users.Skip((int)skip);
                }
                if (top != null)
                {
                    users = users.Take((int)top);
                }
                return users.Select(u => new
                {
                    ID = u.ID,
                    User = new
                    {
                        ID = u.ID,
                        Name = u.Name,
                        Email = u.Email
                    },
                    Role = u.Role.Name,
                    BriefDescription = u.BriefDescription,

                }).ToList();
            }
        }

        [OperationContract]
        [WebGet(UriTemplate = "Users/{id}")]
        public object GetUser(string id)
        {
            int iid = int.Parse(id);
            using (MooDB db = new MooDB())
            {
                User user = (from u in db.Users
                             where u.ID == iid
                             select u).SingleOrDefault<User>();
                if (user == null) throw new ArgumentException("无此用户");

                Access.Required(db, user, Function.ReadUser);

                return new
                {
                    ID = user.ID,
                    Name = user.Name,
                    Email = user.Email,
                    BriefDescription = user.BriefDescription,
                    Description = user.Description,
                    Role = user.Role.Name,
                    CreateTime = user.CreateTime,
                };
            }
        }

        #region CreateUser
        public class CreateUserData
        {
            public string Name;
            public string Password;
            public string Email;
        }

        [OperationContract]
        [WebInvoke(UriTemplate = "Users", Method = "POST")]
        public int CreateUser(CreateUserData user)
        {
            using (MooDB db = new MooDB())
            {
                User newUser = new User()
                {
                    BriefDescription = "我很懒，什么都没留下~",
                    Description = "我真的很懒，真的什么也没留下。",
                    Email = user.Email,
                    Name = user.Name,
                    Password = Converter.ToSHA256Hash(user.Password),
                    CreateTime = DateTime.Now,
                    Role = new SiteRoles(db).NormalUser,
                    Score = 0,
                };

                if (Security.Authenticated)
                {
                    Access.Required(db, newUser, Function.CreateUser);
                }

                db.Users.AddObject(newUser);
                db.SaveChanges();

                return newUser.ID;
            }
        }
        #endregion

        #region ModifyUser
        [DataContract]
        public class ModifyUserData
        {
            [DataMember]
            public string Name;

            string passwordCheck;
            [DataMember]
            public string PasswordCheck
            {
                get { return passwordCheck; }
                set { passwordCheck = Encoding.UTF8.GetString(RSAHelper.Decrypt(Convert.FromBase64String(value))); }
            }

            string password;
            [DataMember]
            public string Password
            {
                get { return password; }
                set { password = Encoding.UTF8.GetString(RSAHelper.Decrypt(Convert.FromBase64String(value))); }
            }
            [DataMember]
            public string Role;
            [DataMember]
            public string BriefDescription;
            [DataMember]
            public string Description;
            [DataMember]
            public string Email;
        }

        [OperationContract]
        [WebInvoke(UriTemplate = "Users/{id}", Method = "PUT")]
        public void ModifyUser(string id, ModifyUserData user)
        {
            int iid = int.Parse(id);
            using (MooDB db = new MooDB())
            {
                User theUser = (from u in db.Users
                                where u.ID == iid
                                select u).SingleOrDefault<User>();
                if (theUser == null) throw new ArgumentException("无此用户");

                Access.Required(db, theUser, Function.ModifyUser);

                if (user.BriefDescription != null)
                {
                    theUser.BriefDescription = user.BriefDescription;
                }
                if (user.Description != null)
                {
                    theUser.Description = user.Description;
                }
                if (user.Email != null)
                {
                    theUser.Email = user.Email;
                }
                if (user.Name != null)
                {
                    theUser.Name = user.Name;
                }
                if (user.Password != null)
                {
                    if (Converter.ToSHA256Hash(user.PasswordCheck) != theUser.Password)
                        throw new InvalidOperationException("旧有密码不正确");
                    theUser.Password = Converter.ToSHA256Hash(user.Password);
                }
                if (user.Role != null)
                {
                    Access.Required(db, theUser, Function.ModifyUserRole);
                    SiteRoles roles = new SiteRoles(db);
                    switch (user.Role)
                    {
                        case "Organizer":
                            theUser.Role = roles.Organizer;
                            break;
                        case "Worker":
                            theUser.Role = roles.Worker;
                            break;
                        case "NormalUser":
                            theUser.Role = roles.NormalUser;
                            break;
                        case "Reader":
                            theUser.Role = roles.Reader;
                            break;
                        default:
                            throw new ArgumentException("无效的角色");
                    }
                }

                db.SaveChanges();

                if (SiteUsers.ByID.ContainsKey(theUser.ID))
                {
                    SiteUsers.ByID[theUser.ID].Initialize(theUser);
                }
            }
        }
        #endregion

        #endregion

        #region Post
        [OperationContract]
        [WebGet(UriTemplate = "Posts/Count")]
        public int CountPost()
        {
            int? problemID = QueryParameters["problemID"] == null ? null : (int?)int.Parse(QueryParameters["problemID"]);
            using (MooDB db = new MooDB())
            {
                IQueryable<Post> posts = db.Posts;
                if (problemID != null)
                {
                    posts = posts.Where(p => p.Problem.ID == problemID);
                }
                return posts.Count();
            }
        }

        [OperationContract]
        [WebGet(UriTemplate = "Posts")]
        public List<FullPost> ListPost()
        {
            int? skip = QueryParameters["skip"] == null ? null : (int?)int.Parse(QueryParameters["skip"]);
            int? top = QueryParameters["top"] == null ? null : (int?)int.Parse(QueryParameters["top"]);
            int? problemID = QueryParameters["problemID"] == null ? null : (int?)int.Parse(QueryParameters["problemID"]);
            using (MooDB db = new MooDB())
            {
                IQueryable<Post> posts = db.Posts;
                if (problemID != null)
                {
                    posts = posts.Where(p => p.Problem.ID == problemID);
                }
                posts.OrderByDescending(p => p.OnTop).ThenByDescending(p => p.ReplyTime);
                if (skip != null)
                {
                    posts = posts.Skip((int)skip);
                }
                if (top != null)
                {
                    posts = posts.Take((int)top);
                }
                return posts.ToList().Select(p => p.ToFullPost()).ToList();
            }
        }

        [OperationContract]
        [WebGet(UriTemplate = "Posts/{id}")]
        public FullPost GetPost(string id)
        {
            int iid = int.Parse(id);
            using (MooDB db = new MooDB())
            {
                Post post = (from p in db.Posts
                             where p.ID == iid
                             select p).SingleOrDefault<Post>();
                if (post == null) throw new ArgumentException("无此帖子");

                Access.Required(db, post, Function.ReadPost);

                return post.ToFullPost();
            }
        }

        [OperationContract]
        [WebInvoke(UriTemplate = "Posts", Method = "POST")]
        public int CreatePost(FullPost post)
        {
            using (MooDB db = new MooDB())
            {
                Problem problem;
                if (post.Problem == null)
                {
                    problem = null;
                }
                else
                {
                    problem = (from p in db.Problems
                               where p.ID == post.Problem
                               select p).SingleOrDefault<Problem>();
                    if (problem == null) throw new ArgumentException("无此题目");
                }

                Post newPost = new Post()
                {
                    Name = post.Name,
                    OnTop = false,
                    Problem = problem,
                    Locked = false,
                    ReplyTime = DateTime.Now
                };

                Access.Required(db, newPost, Function.CreatePost);

                db.Posts.AddObject(newPost);
                db.SaveChanges();
                return newPost.ID;
            }
        }

        [OperationContract]
        [WebInvoke(UriTemplate = "Posts/{id}", Method = "PUT")]
        public void ModifyPost(string id, FullPost post)
        {
            int iid = int.Parse(id);
            using (MooDB db = new MooDB())
            {
                Post thePost = (from p in db.Posts
                                where p.ID == iid
                                select p).SingleOrDefault<Post>();
                if (thePost == null) throw new ArgumentException("无此帖子");

                Access.Required(db, thePost, Function.ModifyPost);

                if (post.Name != null)
                {
                    thePost.Name = post.Name;
                }
                if (post.OnTop != null)
                {
                    thePost.OnTop = (bool)post.OnTop;
                }
                if (post.Locked != null)
                {
                    thePost.Locked = (bool)post.Locked;
                }

                db.SaveChanges();
            }
        }

        [OperationContract]
        [WebInvoke(UriTemplate = "Posts/{id}", Method = "DELETE")]
        public void DeletePost(string id)
        {
            int iid = int.Parse(id);
            using (MooDB db = new MooDB())
            {
                Post post = (from p in db.Posts
                             where p.ID == iid
                             select p).SingleOrDefault<Post>();
                if (post == null) throw new ArgumentException("无此帖子");

                Access.Required(db, post, Function.DeletePost);

                db.Posts.DeleteObject(post);
                db.SaveChanges();
            }
        }
        #endregion

        #region PostItem
        [OperationContract]
        [WebGet(UriTemplate = "Posts/{postID}/Items/Count")]
        public int CountPostItem(string postID)
        {
            int ipostID = int.Parse(postID);
            using (MooDB db = new MooDB())
            {
                IQueryable<PostItem> postItems = from i in db.PostItems
                                                 where i.Post.ID == ipostID
                                                 select i;
                return postItems.Count();
            }
        }

        [OperationContract]
        [WebGet(UriTemplate = "Posts/{postID}/Items")]
        public object ListPostItem(string postID)
        {
            int? skip = QueryParameters["skip"] == null ? null : (int?)int.Parse(QueryParameters["skip"]);
            int? top = QueryParameters["top"] == null ? null : (int?)int.Parse(QueryParameters["top"]);
            bool order = QueryParameters["order"] != "desc";
            int? idGreaterThan = OptionalIntParameter("idGT");
            int ipostID = int.Parse(postID);
            using (MooDB db = new MooDB())
            {
                IQueryable<PostItem> postItems = from i in db.PostItems
                                                 where i.Post.ID == ipostID
                                                 select i;
                if (idGreaterThan != null)
                {
                    postItems = postItems.Where(p => p.ID > idGreaterThan);
                }
                if (order)
                {
                    postItems = postItems.OrderBy(i => i.ID);
                }
                else
                {
                    postItems = postItems.OrderByDescending(i => i.ID);
                }

                if (skip != null)
                {
                    postItems = postItems.Skip((int)skip);
                }
                if (top != null)
                {
                    postItems = postItems.Take((int)top);
                }
                return postItems.Select(i => new
                {
                    ID = i.ID,
                    CreatedBy = new
                    {
                        ID = i.CreatedBy.ID,
                        Name = i.CreatedBy.Name
                    },
                    Content = i.Content,
                    CreateTime = i.CreateTime,
                }).ToList();
            }
        }

        [OperationContract]
        [WebGet(UriTemplate = "Posts/{postID}/Items/{id}")]
        public FullPostItem GetPostItem(string postID, string id)
        {
            int iid = int.Parse(id);
            using (MooDB db = new MooDB())
            {
                PostItem postItem = (from i in db.PostItems
                                     where i.ID == iid
                                     select i).Single<PostItem>();
                if (postItem == null) throw new ArgumentException("无此帖子楼层");

                Access.Required(db, postItem, Function.ReadPostItem);

                return postItem.ToFullPostItem();
            }
        }

        [OperationContract]
        [WebInvoke(UriTemplate = "Posts/{postID}/Items", Method = "POST")]
        public int CreatePostItem(string postID, FullPostItem postItem)
        {
            int ipostID = int.Parse(postID);
            using (MooDB db = new MooDB())
            {
                Post post = (from p in db.Posts
                             where p.ID == ipostID
                             select p).SingleOrDefault<Post>();
                if (post == null) throw new ArgumentException("无此帖子");

                PostItem newPostItem = new PostItem()
                {
                    Content = postItem.Content,
                    CreateTime = DateTime.Now,
                    CreatedBy = Security.CurrentUser.GetDBUser(db),
                    Post = post,
                };
                post.ReplyTime = DateTime.Now;

                Access.Required(db, newPostItem, Function.CreatePostItem);

                db.PostItems.AddObject(newPostItem);
                db.SaveChanges();
                return newPostItem.ID;
            }
        }

        [OperationContract]
        [WebInvoke(UriTemplate = "Posts/{postID}/Items/{id}", Method = "PUT")]
        public void ModifyPostItem(string postID, string id, FullPostItem postItem)
        {
            int iid = int.Parse(id);
            using (MooDB db = new MooDB())
            {
                PostItem thePostItem = (from i in db.PostItems
                                        where i.ID == iid
                                        select i).SingleOrDefault<PostItem>();
                if (thePostItem == null) throw new ArgumentException("无此帖子楼层");

                Access.Required(db, thePostItem, Function.ModifyPostItem);

                if (postItem.Content != null)
                {
                    thePostItem.Content = postItem.Content;
                }

                db.SaveChanges();
            }
        }

        [OperationContract]
        [WebInvoke(UriTemplate = "Posts/{postID}/Items/{id}", Method = "DELETE")]
        public void DeletePostItem(string postID, string id)
        {
            int iid = int.Parse(id);
            using (MooDB db = new MooDB())
            {
                PostItem postItem = (from i in db.PostItems
                                     where i.ID == iid
                                     select i).SingleOrDefault<PostItem>();
                if (postItem == null) throw new ArgumentException("无此帖子楼层");

                Access.Required(db, postItem, Function.DeletePostItem);

                db.PostItems.DeleteObject(postItem);
                db.SaveChanges();
            }
        }
        #endregion

        #region Article
        [OperationContract]
        [WebGet(UriTemplate = "Articles/Count")]
        public int CountArticle()
        {
            int? problemID = QueryParameters["problemID"] == null ? null : (int?)int.Parse(QueryParameters["problemID"]);
            string nameContains = QueryParameters["nameContains"];
            using (MooDB db = new MooDB())
            {
                IQueryable<Article> articles = db.Articles;
                if (problemID != null)
                {
                    articles = articles.Where(a => a.Problem.ID == problemID);
                }
                if (nameContains != null)
                {
                    articles = articles.Where(a => a.Name.Contains(nameContains));
                }
                return articles.Count();
            }
        }

        [OperationContract]
        [WebGet(UriTemplate = "Articles")]
        public object ListArticle()
        {
            int? problemID = OptionalIntParameter("problemID");
            string nameContains = QueryParameters["nameContains"];
            int? skip = OptionalIntParameter("skip");
            int? top = OptionalIntParameter("top");
            int? id = OptionalIntParameter("id");
            int? tagID = OptionalIntParameter("tagID");
            using (MooDB db = new MooDB())
            {
                IQueryable<Article> articles = db.Articles;
                if (id != null)
                {
                    articles = articles.Where(a => a.ID == id);
                }
                if (tagID != null)
                {
                    articles = from a in articles
                               where a.Tag.Any(t => t.ID == tagID)
                               select a;
                }
                if (problemID != null)
                {
                    articles = articles.Where(a => a.Problem.ID == problemID);
                }
                if (nameContains != null)
                {
                    articles = articles.Where(a => a.Name.Contains(nameContains));
                }

                articles = articles.OrderByDescending(a => a.ID);

                if (skip != null)
                {
                    articles = articles.Skip((int)skip);
                }
                if (top != null)
                {
                    articles = articles.Take((int)top);
                }
                return articles.Select(a => new
                {
                    ID = a.ID,
                    Article = new
                    {
                        ID = a.ID,
                        Name = a.Name
                    },
                    Problem = new
                    {
                        ID = a.Problem == null ? null : (int?)a.Problem.ID,
                        Name = a.Problem == null ? null : a.Problem.Name
                    }
                }).ToList();
            }
        }

        [OperationContract]
        [WebGet(UriTemplate = "Articles/{id}")]
        public object GetArticle(string id)
        {
            int iid = int.Parse(id);
            int? revisionID = OptionalIntParameter("revisionID");
            using (MooDB db = new MooDB())
            {
                Article article = (from a in db.Articles
                                   where a.ID == iid
                                   select a).SingleOrDefault<Article>();
                if (article == null) throw new ArgumentException("无此文章");

                Access.Required(db, article, Function.ReadArticle);

                ArticleRevision revision;
                if (revisionID == null)
                {
                    revision = article.LatestRevision;
                }
                else
                {
                    revision = (from r in db.ArticleRevisions
                                where r.ID == revisionID
                                select r).SingleOrDefault<ArticleRevision>();
                    if (revision == null) throw new ArgumentException("无此文章版本");
                }

                return new
                {
                    ID = article.ID,
                    Name = article.Name,
                    Content = Access.Check(db, revision, Function.ReadArticleRevision) ? revision.Content : "权限不足，无法查看内容。",
                    CreateTime = article.CreateTime,
                    CreatedBy = new
                    {
                        ID = article.CreatedBy.ID,
                        Name = article.CreatedBy.Name
                    },
                    Problem = new
                    {
                        ID = article.Problem == null ? null : (int?)article.Problem.ID,
                        Name = article.Problem == null ? null : article.Problem.Name
                    },
                    Revision = new
                    {
                        CreatedBy = new
                        {
                            ID = revision.CreatedBy.ID,
                            Name = revision.CreatedBy.Name
                        },
                        CreateTime = revision.CreateTime
                    },
                    Tag = article.Tag.Select(t => new
                                            {
                                                ID = t.ID,
                                                Name = t.Name
                                            }).ToList()
                };
            }
        }

        #region CreateArticle
        [DataContract]
        public class CreateArticleData
        {
            [DataMember]
            public int? ProblemID;
            [DataMember]
            public string Name;
            [DataMember]
            public string Content;
        }

        [OperationContract]
        [WebInvoke(UriTemplate = "Articles", Method = "POST")]
        public int CreateArticle(CreateArticleData article)
        {
            using (MooDB db = new MooDB())
            {
                Problem problem;
                if (article.ProblemID != null)
                {
                    problem = (from p in db.Problems
                               where p.ID == article.ProblemID
                               select p).SingleOrDefault<Problem>();
                    if (problem == null) throw new ArgumentException("无此题目");
                }
                else
                {
                    problem = null;
                }

                User currentUser = Security.CurrentUser.GetDBUser(db);
                Article newArticle = new Article()
                {
                    CreatedBy = currentUser,
                    CreateTime = DateTime.Now,
                    Name = article.Name,
                    Problem = problem,
                };
                Access.Required(db, newArticle, Function.CreateArticle);
                db.Articles.AddObject(newArticle);

                ArticleRevision revision = new ArticleRevision()
                {
                    Article = newArticle,
                    Content = article.Content,
                    CreatedBy = currentUser,
                    CreateTime = DateTime.Now,
                    Reason = "创建文章"
                };
                newArticle.LatestRevision = revision;
                Access.Required(db, revision, Function.CreateArticleRevision);
                db.ArticleRevisions.AddObject(revision);

                db.SaveChanges();
                return newArticle.ID;
            }
        }
        #endregion

        [OperationContract]
        [WebInvoke(UriTemplate = "Articles/{id}", Method = "PUT")]
        public void ModifyArticle(string id, FullArticle article)
        {
            int iid = int.Parse(id);
            using (MooDB db = new MooDB())
            {
                Article theArticle = (from a in db.Articles
                                      where a.ID == iid
                                      select a).SingleOrDefault<Article>();
                if (theArticle == null) throw new ArgumentException("无此文章");

                Access.Required(db, theArticle, Function.ModifyArticle);

                if (article.Name != null)
                {
                    theArticle.Name = article.Name;
                }
                if (article.Problem != null)
                {
                    Problem problem = (from p in db.Problems
                                       where p.ID == article.Problem
                                       select p).SingleOrDefault<Problem>();
                    if (problem == null) throw new ArgumentException("无此题目");
                    theArticle.Problem = problem;
                }
                db.SaveChanges();
            }
        }

        [OperationContract]
        [WebInvoke(UriTemplate = "Articles/{id}", Method = "DELETE")]
        public void DeleteArticle(string id)
        {
            int iid = int.Parse(id);
            using (MooDB db = new MooDB())
            {
                Article article = (from a in db.Articles
                                   where a.ID == iid
                                   select a).SingleOrDefault<Article>();
                if (article == null) throw new ArgumentException("无此文章");

                Access.Required(db, article, Function.DeleteArticle);

                article.Tag.Clear();
                db.Articles.DeleteObject(article);
                db.SaveChanges();
            }
        }
        #endregion

        #region ArticleTags

        [OperationContract]
        [WebInvoke(UriTemplate = "Articles/{articleID}/Tags", Method = "POST")]
        public void CreateArticleTag(string articleID, string tagID)
        {
            int iarticleID = int.Parse(articleID);
            int iid = int.Parse(tagID);
            using (MooDB db = new MooDB())
            {
                Article article = (from a in db.Articles
                                   where a.ID == iarticleID
                                   select a).SingleOrDefault<Article>();
                if (article == null) throw new ArgumentException("无此文章");

                Tag tag = (from t in db.Tags
                           where t.ID == iid
                           select t).SingleOrDefault<Tag>();
                if (tag == null) throw new ArgumentException("无此标签");

                Access.Required(db, article, Function.ModifyArticle);
                article.Tag.Add(tag);
                db.SaveChanges();
            }
        }

        [OperationContract]
        [WebInvoke(UriTemplate = "Articles/{articleID}/Tags/{id}", Method = "DELETE")]
        public void DeleteArticleTag(string articleID, string id)
        {
            int iarticleID = int.Parse(articleID);
            int iid = int.Parse(id);
            using (MooDB db = new MooDB())
            {
                Article article = (from a in db.Articles
                                   where a.ID == iarticleID
                                   select a).SingleOrDefault<Article>();
                if (article == null) throw new ArgumentException("无此文章");

                Tag tag = (from t in article.Tag
                           where t.ID == iid
                           select t).SingleOrDefault<Tag>();
                if (tag == null) throw new ArgumentException("无此标签");

                Access.Required(db, article, Function.ModifyArticle);
                article.Tag.Remove(tag);
                db.SaveChanges();
            }
        }
        #endregion

        #region ArticleRevisions
        [OperationContract]
        [WebGet(UriTemplate = "Articles/{articleID}/Revisions/Count")]
        public int CountArticleRevision(string articleID)
        {
            int iarticleID = int.Parse(articleID);
            using (MooDB db = new MooDB())
            {
                return (from r in db.ArticleRevisions
                        where r.Article.ID == iarticleID
                        select r).Count();
            }
        }

        [OperationContract]
        [WebGet(UriTemplate = "Articles/{articleID}/Revisions")]
        public object ListArticleRevision(string articleID)
        {
            int? skip = QueryParameters["skip"] == null ? null : (int?)int.Parse(QueryParameters["skip"]);
            int? top = QueryParameters["top"] == null ? null : (int?)int.Parse(QueryParameters["top"]);
            int iarticleID = int.Parse(articleID);
            using (MooDB db = new MooDB())
            {
                IQueryable<ArticleRevision> articleRevisions = from r in db.ArticleRevisions
                                                               where r.Article.ID == iarticleID
                                                               orderby r.ID descending
                                                               select r;
                if (skip != null)
                {
                    articleRevisions = articleRevisions.Skip((int)skip);
                }
                if (top != null)
                {
                    articleRevisions = articleRevisions.Take((int)top);
                }

                return articleRevisions.Select(r => new
                {
                    ID = r.ID,
                    CreatedBy = new
                    {
                        ID = r.CreatedBy.ID,
                        Name = r.CreatedBy.Name
                    },
                    CreateTime = r.CreateTime,
                    Reason = r.Reason
                }).ToList();
            }
        }

        #region CreateArticleRevision
        [DataContract]
        public class CreateArticleRevisionData
        {
            [DataMember]
            public string Content;
            [DataMember]
            public string Reason;
        }

        [OperationContract]
        [WebInvoke(UriTemplate = "Articles/{articleID}/Revisions", Method = "POST")]
        public int CreateArticleRevision(string articleID, CreateArticleRevisionData revision)
        {
            int iarticleID = int.Parse(articleID);
            using (MooDB db = new MooDB())
            {
                Article article = (from a in db.Articles
                                   where a.ID == iarticleID
                                   select a).SingleOrDefault<Article>();
                if (article == null) throw new ArgumentException("无此文章");

                ArticleRevision newRevision = new ArticleRevision
                {
                    Article = article,
                    Content = revision.Content,
                    CreatedBy = Security.CurrentUser.GetDBUser(db),
                    CreateTime = DateTime.Now,
                    Reason = revision.Reason
                };
                article.LatestRevision = newRevision;

                Access.Required(db, newRevision, Function.CreateArticleRevision);

                db.ArticleRevisions.AddObject(newRevision);
                db.SaveChanges();
                return newRevision.ID;
            }
        }
        #endregion

        [OperationContract]
        [WebInvoke(UriTemplate = "Articles/{articleID}/Revisions/{id}", Method = "DELETE")]
        public void DeleteArticleRevision(string articleID, string id)
        {
            int iid = int.Parse(id);
            using (MooDB db = new MooDB())
            {
                ArticleRevision revision = (from r in db.ArticleRevisions
                                            where r.ID == iid
                                            select r).SingleOrDefault<ArticleRevision>();
                if (revision == null) throw new ArgumentException("无此文章版本");

                Access.Required(db, revision, Function.DeleteArticleRevision);

                Article article = revision.Article;
                if (article.LatestRevision.ID == revision.ID)
                {
                    throw new InvalidOperationException("不可删除最新版本");
                }

                db.ArticleRevisions.DeleteObject(revision);
                db.SaveChanges();
            }
        }
        #endregion

        #region Tags
        [OperationContract]
        [WebGet(UriTemplate = "Tags")]
        public object ListTag()
        {
            int? skip = OptionalIntParameter("skip");
            int? top = OptionalIntParameter("top");
            int? id = OptionalIntParameter("id");
            string nameContains = QueryParameters["nameContains"];
            using (MooDB db = new MooDB())
            {
                IQueryable<Tag> tags = db.Tags;
                if (nameContains != null)
                {
                    tags = tags.Where(t => t.Name.Contains(nameContains));
                }
                if (id != null)
                {
                    tags = tags.Where(t => t.ID == id);
                }

                tags = tags.OrderByDescending(t => t.ID);

                if (skip != null)
                {
                    tags = tags.Skip((int)skip);
                }
                if (top != null)
                {
                    tags = tags.Take((int)top);
                }

                return tags.Select(t => new
                {
                    ID = t.ID,
                    Name = t.Name,
                    ProblemNumber = t.Problem.Count,
                    ArticleNumber = t.Article.Count
                }).ToList();
            }
        }

        #region CreateTag
        [DataContract]
        public class CreateTagData
        {
            [DataMember]
            public string Name;
        }
        [OperationContract]
        [WebInvoke(UriTemplate = "Tags", Method = "POST")]
        public int CreateTag(CreateTagData tag)
        {
            using (MooDB db = new MooDB())
            {
                Tag newTag = new Tag
                {
                    Name = tag.Name,
                };

                Access.Required(db, newTag, Function.CreateTag);
                db.Tags.AddObject(newTag);
                db.SaveChanges();
                return newTag.ID;
            }
        }
        #endregion

        #region ModifyTag
        [DataContract]
        public class ModifyTagData
        {
            [DataMember]
            public string Name;
        }
        [OperationContract]
        [WebInvoke(UriTemplate = "Tags/{id}", Method = "PUT")]
        public void ModifyTag(string id, ModifyTagData tag)
        {
            int iid = int.Parse(id);
            using (MooDB db = new MooDB())
            {
                Tag theTag = (from t in db.Tags
                              where t.ID == iid
                              select t).SingleOrDefault<Tag>();
                if (theTag == null) throw new ArgumentException("无此标签");

                Access.Required(db, theTag, Function.ModifyTag);

                if (tag.Name != null)
                {
                    theTag.Name = tag.Name;
                }

                db.SaveChanges();
            }
        }
        #endregion

        [OperationContract]
        [WebInvoke(UriTemplate = "Tags/{id}", Method = "DELETE")]
        public void DeleteTag(string id)
        {
            int iid = int.Parse(id);
            using (MooDB db = new MooDB())
            {
                Tag tag = (from t in db.Tags
                           where t.ID == iid
                           select t).SingleOrDefault<Tag>();
                if (tag == null) throw new ArgumentException("无此标签");

                Access.Required(db, tag, Function.DeleteTag);

                tag.Problem.Clear();
                tag.Article.Clear();

                db.Tags.DeleteObject(tag);
                db.SaveChanges();
            }
        }
        #endregion

        #region Contests
        [OperationContract]
        [WebGet(UriTemplate = "Contests/Count")]
        public int CountContest()
        {
            using (MooDB db = new MooDB())
            {
                IQueryable<Contest> contests = db.Contests;
                return contests.Count();
            }
        }

        [OperationContract]
        [WebGet(UriTemplate = "Contests")]
        public object ListContest()
        {
            int? skip = QueryParameters["skip"] == null ? null : (int?)int.Parse(QueryParameters["skip"]);
            int? top = QueryParameters["top"] == null ? null : (int?)int.Parse(QueryParameters["top"]);
            using (MooDB db = new MooDB())
            {
                IQueryable<Contest> contests = from c in db.Contests
                                               orderby c.ID descending
                                               select c;
                if (skip != null)
                {
                    contests = contests.Skip((int)skip);
                }
                if (top != null)
                {
                    contests = contests.Take((int)top);
                }

                return contests.Select(c => new
                {
                    ID = c.ID,
                    Contest = new
                    {
                        ID = c.ID,
                        Name = c.Name
                    },
                    Status = c.Status,
                    StartTime = c.StartTime,
                    EndTime = c.EndTime
                }).ToList();
            }
        }

        [OperationContract]
        [WebGet(UriTemplate = "Contests/{id}")]
        public object GetContest(string id)
        {
            int iid = int.Parse(id);
            using (MooDB db = new MooDB())
            {
                Contest contest = (from c in db.Contests
                                   where c.ID == iid
                                   select c).SingleOrDefault<Contest>();
                if (contest == null) throw new ArgumentException("无此比赛");

                Access.Required(db, contest, Function.ReadContest);

                return new
                {
                    ID = contest.ID,
                    Name = contest.Name,
                    Status = contest.Status,
                    Attended = contest.User.Contains(Security.CurrentUser.GetDBUser(db)),
                    StartTime = contest.StartTime,
                    EndTime = contest.EndTime,
                    Description = contest.Description,
                    UserNumber = contest.User.Count,
                    Problem = contest.Problem.Select(p => new
                                                    {
                                                        ID = p.ID,
                                                        Name = p.Name,
                                                        Type = p.Type
                                                    }).ToList(),
                    EnableTestingOnEnd = contest.EnableTestingOnEnd,
                    EnableTestingOnStart = contest.EnableTestingOnStart,
                    HideProblemOnEnd = contest.HideProblemOnEnd,
                    HideProblemOnStart = contest.HideProblemOnStart,
                    HideTestCaseOnEnd = contest.HideTestCaseOnEnd,
                    HideTestCaseOnStart = contest.HideTestCaseOnStart,
                    LockArticleOnEnd = contest.LockArticleOnEnd,
                    LockArticleOnStart = contest.LockArticleOnStart,
                    LockPostOnEnd = contest.LockPostOnEnd,
                    LockPostOnStart = contest.LockPostOnStart,
                    LockProblemOnEnd = contest.LockProblemOnEnd,
                    LockProblemOnStart = contest.LockProblemOnStart,
                    LockRecordOnEnd = contest.LockRecordOnEnd,
                    LockRecordOnStart = contest.LockRecordOnStart,
                    LockTestCaseOnEnd = contest.LockTestCaseOnEnd,
                    LockTestCaseOnStart = contest.LockTestCaseOnStart,
                    HideJudgeInfoOnStart = contest.HideJudgeInfoOnStart,
                    HideJudgeInfoOnEnd = contest.HideJudgeInfoOnEnd,
                    ViewResultAnyTime = contest.ViewResultAnyTime
                };
            }
        }

        #region GetContestResult

        [DataContract]
        public class ProblemAndScore
        {
            [DataMember]
            public int ID;
            [DataMember]
            public int Score;
        }

        [DataContract]
        public class ContestResult
        {
            [DataMember]
            public int ID;
            [DataMember]
            public string Name;
            [DataMember]
            public int Score;
            [DataMember]
            public List<ProblemAndScore> Scores;
        }

        [OperationContract]
        [WebGet(UriTemplate = "Contests/{id}/Result")]
        public List<ContestResult> GetContestResult(string id)
        {
            int iid = int.Parse(id);
            int? skip = OptionalIntParameter("skip");
            int? top = OptionalIntParameter("top");
            using (MooDB db = new MooDB())
            {
                Contest contest = (from c in db.Contests
                                   where c.ID == iid
                                   select c).SingleOrDefault<Contest>();
                if (contest == null) throw new ArgumentException("无此比赛");

                Access.Required(db, contest, Function.ReadContestResult);

                IQueryable<ContestResult> results = (from u in contest.User
                                                     let maxScores = from p in contest.Problem
                                                                     let records = from r in u.Record
                                                                                   where r.CreateTime > contest.StartTime && r.CreateTime < contest.EndTime
                                                                                      && r.Problem.ID == p.ID
                                                                                      && r.JudgeInfo != null && r.JudgeInfo.Score >= 0
                                                                                   select r.JudgeInfo.Score
                                                                     let maxScore = records.DefaultIfEmpty().Max()
                                                                     select new
                                                                     {
                                                                         Problem = p,
                                                                         Score = maxScore
                                                                     }
                                                     let score = maxScores.Any() ? maxScores.Sum(s => s.Score) : 0
                                                     orderby score descending
                                                     select new ContestResult
                                                     {

                                                         ID = u.ID,
                                                         Name = u.Name,
                                                         Score = score,
                                                         Scores = maxScores.Select(s => new ProblemAndScore
                                                         {

                                                             ID = s.Problem.ID,
                                                             Score = s.Score
                                                         }).ToList()
                                                     }).AsQueryable();

                if (skip != null)
                {
                    results = results.Skip((int)skip);
                }
                if (top != null)
                {
                    results = results.Take((int)top);
                }
                return results.ToList();
            }
        }
        #endregion

        #region CreateContest

        [DataContract]
        public class CreateContestData
        {
            [DataMember]
            public string Name;
            [DataMember]
            public string Description;
            [DataMember]
            public DateTime StartTime;
            [DataMember]
            public DateTime EndTime;
        }

        [OperationContract]
        [WebInvoke(UriTemplate = "Contests", Method = "POST")]
        public int CreateContest(CreateContestData contest)
        {
            using (MooDB db = new MooDB())
            {
                Contest newContest = new Contest
                {
                    Description = contest.Description,
                    Name = contest.Name,
                    StartTime = contest.StartTime,
                    EndTime = contest.EndTime,
                    Status = "Before",

                    EnableTestingOnEnd = true,
                    EnableTestingOnStart = true,
                    HideProblemOnEnd = false,
                    HideProblemOnStart = false,
                    HideTestCaseOnEnd = false,
                    HideTestCaseOnStart = true,
                    LockArticleOnEnd = false,
                    LockArticleOnStart = true,
                    LockPostOnEnd = false,
                    LockPostOnStart = true,
                    LockProblemOnEnd = false,
                    LockProblemOnStart = true,
                    LockRecordOnEnd = false,
                    LockRecordOnStart = false,
                    LockTestCaseOnEnd = false,
                    LockTestCaseOnStart = true,
                    HideJudgeInfoOnEnd = false,
                    HideJudgeInfoOnStart = true,
                    ViewResultAnyTime = false
                };

                Access.Required(db, newContest, Function.CreateContest);

                db.Contests.AddObject(newContest);
                db.SaveChanges();
                return newContest.ID;
            }
        }
        #endregion

        [OperationContract]
        [WebInvoke(UriTemplate = "Contests/{id}", Method = "PUT")]
        public void ModifyContest(string id, FullContest contest)
        {
            int iid = int.Parse(id);
            using (MooDB db = new MooDB())
            {
                Contest theContest = (from c in db.Contests
                                      where c.ID == iid
                                      select c).SingleOrDefault<Contest>();
                if (theContest == null) throw new ArgumentException("无此比赛");

                Access.Required(db, theContest, Function.ModifyContest);

                if (contest.Description != null)
                {
                    theContest.Description = contest.Description;
                }
                if (contest.EnableTestingOnEnd != null)
                {
                    theContest.EnableTestingOnEnd = (bool)contest.EnableTestingOnEnd;
                }
                if (contest.EnableTestingOnStart != null)
                {
                    theContest.EnableTestingOnStart = (bool)contest.EnableTestingOnStart;
                }
                if (contest.EndTime != null)
                {
                    theContest.EndTime = (DateTime)contest.EndTime;
                }
                if (contest.HideProblemOnEnd != null)
                {
                    theContest.HideProblemOnEnd = (bool)contest.HideProblemOnEnd;
                }
                if (contest.HideProblemOnStart != null)
                {
                    theContest.HideProblemOnStart = (bool)contest.HideProblemOnStart;
                }
                if (contest.HideTestCaseOnEnd != null)
                {
                    theContest.HideTestCaseOnEnd = (bool)contest.HideTestCaseOnEnd;
                }
                if (contest.HideTestCaseOnStart != null)
                {
                    theContest.HideTestCaseOnStart = (bool)contest.HideTestCaseOnStart;
                }
                if (contest.LockArticleOnEnd != null)
                {
                    theContest.LockArticleOnEnd = (bool)contest.LockArticleOnEnd;
                }
                if (contest.LockArticleOnStart != null)
                {
                    theContest.LockArticleOnStart = (bool)contest.LockArticleOnStart;
                }
                if (contest.LockPostOnEnd != null)
                {
                    theContest.LockPostOnEnd = (bool)contest.LockPostOnEnd;
                }
                if (contest.LockPostOnStart != null)
                {
                    theContest.LockPostOnStart = (bool)contest.LockPostOnStart;
                }
                if (contest.LockProblemOnEnd != null)
                {
                    theContest.LockProblemOnEnd = (bool)contest.LockProblemOnEnd;
                }
                if (contest.LockProblemOnStart != null)
                {
                    theContest.LockProblemOnStart = (bool)contest.LockProblemOnStart;
                }
                if (contest.LockRecordOnEnd != null)
                {
                    theContest.LockRecordOnEnd = (bool)contest.LockRecordOnEnd;
                }
                if (contest.LockRecordOnStart != null)
                {
                    theContest.LockRecordOnStart = (bool)contest.LockRecordOnStart;
                }
                if (contest.LockTestCaseOnEnd != null)
                {
                    theContest.LockTestCaseOnEnd = (bool)contest.LockTestCaseOnEnd;
                }
                if (contest.LockTestCaseOnStart != null)
                {
                    theContest.LockTestCaseOnStart = (bool)contest.LockTestCaseOnStart;
                }
                if (contest.HideJudgeInfoOnStart != null)
                {
                    theContest.HideJudgeInfoOnStart = (bool)contest.HideJudgeInfoOnStart;
                }
                if (contest.HideJudgeInfoOnEnd != null)
                {
                    theContest.HideJudgeInfoOnEnd = (bool)contest.HideJudgeInfoOnEnd;
                }
                if (contest.ViewResultAnyTime != null)
                {
                    theContest.ViewResultAnyTime = (bool)contest.ViewResultAnyTime;
                }
                if (contest.Name != null)
                {
                    theContest.Name = contest.Name;
                }
                if (contest.Problem != null)
                {
                    theContest.Problem.Clear();
                    foreach (int problemID in contest.Problem)
                    {
                        Problem problem = (from p in db.Problems
                                           where p.ID == problemID
                                           select p).SingleOrDefault<Problem>();
                        if (problem == null) throw new ArgumentException("无此题目");

                        theContest.Problem.Add(problem);
                    }
                }
                if (contest.StartTime != null)
                {
                    theContest.StartTime = (DateTime)contest.StartTime;
                }

                db.SaveChanges();
            }
        }

        [OperationContract]
        [WebInvoke(UriTemplate = "Contests/{id}", Method = "DELETE")]
        public void DeleteContest(string id)
        {
            int iid = int.Parse(id);
            using (MooDB db = new MooDB())
            {
                Contest contest = (from c in db.Contests
                                   where c.ID == iid
                                   select c).SingleOrDefault<Contest>();
                if (contest == null) throw new ArgumentException("无此比赛");

                Access.Required(db, contest, Function.DeleteContest);

                contest.Problem.Clear();
                contest.User.Clear();

                db.Contests.DeleteObject(contest);
                db.SaveChanges();
            }
        }

        [OperationContract]
        [WebInvoke(UriTemplate = "Contests/{id}/Attend", Method = "POST")]
        public void AttendContest(string id)
        {
            int iid = int.Parse(id);
            using (MooDB db = new MooDB())
            {
                Contest contest = (from c in db.Contests
                                   where c.ID == iid
                                   select c).SingleOrDefault<Contest>();
                if (contest == null) throw new ArgumentException("无此比赛");

                Access.Required(db, contest, Function.AttendContest);

                if (contest.EndTime < DateTime.Now)
                {
                    throw new InvalidOperationException("比赛已结束");
                }

                User currentUser = Security.CurrentUser.GetDBUser(db);
                if (contest.User.Contains(currentUser))
                {
                    throw new InvalidOperationException("早已参加比赛");
                }

                contest.User.Add(currentUser);
                db.SaveChanges();
            }
        }

        [OperationContract]
        [WebInvoke(UriTemplate = "Contests/{id}/Problems", Method = "POST")]
        public void AddContestProblem(string id, int problemID)
        {
            int iid = int.Parse(id);
            using (MooDB db = new MooDB())
            {
                Contest contest = (from c in db.Contests
                                   where c.ID == iid
                                   select c).SingleOrDefault<Contest>();
                if (contest == null) throw new ArgumentException("无此比赛");

                Problem problem = (from p in db.Problems
                                   where p.ID == problemID
                                   select p).SingleOrDefault<Problem>();
                if (problem == null) throw new ArgumentException("无此题目");

                Access.Required(db, contest, Function.ModifyContest);

                contest.Problem.Add(problem);
                db.SaveChanges();
            }
        }

        [OperationContract]
        [WebInvoke(UriTemplate = "Contests/{contestID}/Problems/{id}", Method = "DELETE")]
        public void DeleteContestProblem(string contestID, string id)
        {
            int icontestID = int.Parse(contestID);
            int iid = int.Parse(id);
            using (MooDB db = new MooDB())
            {
                Contest contest = (from c in db.Contests
                                   where c.ID == icontestID
                                   select c).SingleOrDefault<Contest>();
                if (contest == null) throw new ArgumentException("无此比赛");

                Problem problem = (from p in contest.Problem
                                   where p.ID == iid
                                   select p).SingleOrDefault<Problem>();
                if (problem == null) throw new ArgumentException("无此题目");

                Access.Required(db, contest, Function.ModifyContest);

                contest.Problem.Remove(problem);
                db.SaveChanges();
            }
        }
        #endregion

        #region Messages
        [OperationContract]
        [WebGet(UriTemplate = "Messages/Contacts")]
        public object GetContacts()
        {
            int? skip = OptionalIntParameter("skip");
            int? top = OptionalIntParameter("top");
            using (MooDB db = new MooDB())
            {/*
                var fromMe = from m in db.Messages
                             where m.To != null && m.From.ID == Security.CurrentUser.ID && !m.DeletedByFrom
                             group m by m.To into ms
                             select ms.Key;
                var toMe = from m in db.Messages
                           where m.To.ID == Security.CurrentUser.ID && !m.DeletedByTo
                           group m by m.From into ms
                           select ms.Key;
              */
                var contacts = from u in db.Users
                               let fromMe = from m in db.Messages
                                            where m.To.ID == u.ID && m.From.ID == Security.CurrentUser.ID
                                            && !m.DeletedByFrom
                                            select m
                               let toMe = from m in db.Messages
                                          where m.From.ID == u.ID && m.To.ID == Security.CurrentUser.ID
                                          && !m.DeletedByTo
                                          select m
                               where fromMe.Any() || toMe.Any()
                               select new
                               {
                                   User = u,
                                   UnRead = toMe.Where(m => !m.HasRead).Count()
                               };


                if (skip != null)
                {
                    contacts = contacts.Skip((int)skip);
                }
                if (top != null)
                {
                    contacts = contacts.Take((int)top);
                }

                return contacts.Select(u => new
                {
                    ID = u.User.ID,
                    Name = u.User.Name,
                    Email = u.User.Email,
                    UnRead = u.UnRead
                }).ToList();
            }
        }

        [OperationContract]
        [WebGet(UriTemplate = "Messages")]
        public object GetMessage()
        {
            int? with = OptionalIntParameter("with");
            int? skip = OptionalIntParameter("skip");
            int? top = OptionalIntParameter("top");
            int? idGT = OptionalIntParameter("idGT");
            int? idLT = OptionalIntParameter("idLT");
            string order = QueryParameters["order"];
            using (MooDB db = new MooDB())
            {
                IQueryable<Message> messages;
                if (with == null)
                {
                    messages = from m in db.Messages
                               where m.To == null
                               select m;
                }
                else
                {
                    messages = from m in db.Messages
                               where m.To.ID == with && m.From.ID == Security.CurrentUser.ID && !m.DeletedByFrom
                                  || m.From.ID == with && m.To.ID == Security.CurrentUser.ID && !m.DeletedByTo
                               select m;
                }

                if (idGT != null)
                {
                    messages = messages.Where(m => m.ID > idGT);
                }

                if (idLT != null)
                {
                    messages = messages.Where(m => m.ID < idLT);
                }

                if (order == "asc")
                {
                    messages = messages.OrderBy(m => m.ID);
                }
                else
                {
                    messages = messages.OrderByDescending(m => m.ID);
                }

                if (skip != null)
                {
                    messages = messages.Skip((int)skip);
                }
                if (top != null)
                {
                    messages = messages.Take((int)top);
                }

                foreach (Message msg in messages.Where(m => m.To.ID == Security.CurrentUser.ID))
                {
                    msg.HasRead = true;
                }
                db.SaveChanges();

                return messages.Select(m => new
                {
                    ID = m.ID,
                    From = new
                    {
                        ID = m.From.ID,
                        Name = m.From.Name,
                        Email = m.From.Email
                    },
                    To = new
                    {
                        ID = m.To == null ? null : (int?)m.To.ID,
                        Name = m.To == null ? null : m.To.Name,
                        Email = m.To == null ? null : m.To.Email
                    },
                    CreateTime = m.CreateTime,
                    Content = m.Content,
                }).ToList();
            }
        }

        #region CreateMessage
        [DataContract]
        public class CreateMessageData
        {
            [DataMember]
            public int? ToID;

            [DataMember]
            public string Content;
        }

        [OperationContract]
        [WebInvoke(UriTemplate = "Messages", Method = "POST")]
        public int CreateMessage(CreateMessageData message)
        {
            using (MooDB db = new MooDB())
            {
                User to = null;
                if (message.ToID != null)
                {
                    to = (from u in db.Users
                          where u.ID == message.ToID
                          select u).SingleOrDefault<User>();
                    if (to == null) throw new Exception("无此用户");
                }

                Message newMessage = new Message
                {
                    Content = message.Content,
                    CreateTime = DateTime.Now,
                    From = Security.CurrentUser.GetDBUser(db),
                    To = to,
                    HasRead = to == null ? true : false
                };

                Access.Check(db, newMessage, Function.CreateMessage);

                db.Messages.AddObject(newMessage);
                db.SaveChanges();

                WebSockets.WebSocketsAPIHandler.NotifyNewMessage(to);

                return newMessage.ID;
            }
        }
        #endregion

        [OperationContract]
        [WebInvoke(UriTemplate = "Messages", Method = "DELETE")]
        public void DeleteMessage()
        {
            int? with = OptionalIntParameter("with");
            using (MooDB db = new MooDB())
            {
                if (with == null)
                {
                    Access.Required(db, new Message(), Function.DeletePublicMessage);

                    var toDelete = from m in db.Messages
                                   where m.To == null
                                   select m;
                    foreach (Message msg in toDelete)
                    {
                        db.Messages.DeleteObject(msg);
                    }
                }
                else
                {
                    Access.Required(db, new Message(), Function.DeletePrivateMessage);

                    var fromMe = from m in db.Messages
                                 where m.From.ID == Security.CurrentUser.ID && m.To.ID == with
                                 select m;
                    foreach (Message msg in fromMe)
                    {
                        msg.DeletedByFrom = true;
                        if (msg.DeletedByTo)
                        {
                            db.Messages.DeleteObject(msg);
                        }
                    }

                    var toMe = from m in db.Messages
                               where m.To.ID == Security.CurrentUser.ID && m.From.ID == with
                               select m;
                    foreach (Message msg in toMe)
                    {
                        msg.DeletedByTo = true;
                        if (msg.DeletedByFrom)
                        {
                            db.Messages.DeleteObject(msg);
                        }
                    }
                }

                db.SaveChanges();
            }
        }
        #endregion

        #region Files
        [OperationContract]
        [WebGet(UriTemplate = "Files")]
        public object ListFiles()
        {
            int? skip = OptionalIntParameter("skip");
            int? top = OptionalIntParameter("top");
            string nameContains = QueryParameters["nameContains"];
            int? id = OptionalIntParameter("id");
            using (MooDB db = new MooDB())
            {
                IQueryable<UploadedFile> files = db.UploadedFiles;

                if (nameContains != null)
                {
                    files = files.Where(f => f.Name.Contains(nameContains));
                }

                if (id != null)
                {
                    files = files.Where(f => f.ID == id);
                }

                files = files.OrderByDescending(f => f.ID);

                if (skip != null)
                {
                    files = files.Skip((int)skip);
                }

                if (top != null)
                {
                    files = files.Take((int)top);
                }

                return files.Select(f => new
                {
                    ID = f.ID,
                    File = new
                    {
                        ID = f.ID,
                        Name = f.Name
                    },
                    CreatedBy = new
                    {
                        ID = f.CreatedBy.ID,
                        Name = f.CreatedBy.Name
                    }
                }).ToList();
            }
        }

        [OperationContract]
        [WebGet(UriTemplate = "Files/{id}")]
        public object GetFile(string id)
        {
            int iid = int.Parse(id);
            using (MooDB db = new MooDB())
            {
                UploadedFile file = (from f in db.UploadedFiles
                                     where f.ID == iid
                                     select f).SingleOrDefault<UploadedFile>();
                if (file == null) throw new ArgumentException("无此文件");

                Access.Required(db, file, Function.ReadFile);

                return new
                {
                    ID = file.ID,
                    Name = file.Name,
                    Extension = new FileInfo(Config.UploadFileDirectory + file.FileName).Extension,
                    CreatedBy = new
                    {
                        ID = file.CreatedBy.ID,
                        Name = file.CreatedBy.Name
                    },
                    Length = new FileInfo(Config.UploadFileDirectory + file.FileName).Length,
                    Description = file.Description,
                };
            }
        }

        #region CreateFile
        [DataContract]
        public class CreateFileData
        {
            [DataMember]
            public Guid BlobID;

            [DataMember]
            public string Name;

            [DataMember]
            public string Description;
        }

        [OperationContract]
        [WebInvoke(UriTemplate = "Files", Method = "POST")]
        public int CreateFile(CreateFileData file)
        {
            using (MooDB db = new MooDB())
            {
                string fileName = Path.GetFileName(Config.UploadFileDirectory + file.Name);
                if (File.Exists(Config.UploadFileDirectory + fileName))
                {
                    fileName = Rand.RAND.Next() + fileName;
                    if (File.Exists(Config.UploadFileDirectory + fileName))
                    {
                        throw new ArgumentException("文件名称冲突");
                    }
                }
                UploadedFile newFile = new UploadedFile
                {
                    CreatedBy = Security.CurrentUser.GetDBUser(db),
                    Description = file.Description,
                    FileName = fileName,
                    Name = file.Name
                };

                Access.Required(db, newFile, Function.CreateFile);

                File.WriteAllBytes(Config.UploadFileDirectory + fileName, Binary.Get(file.BlobID, true));

                db.SaveChanges();
                return newFile.ID;
            }
        }
        #endregion

        #region ModifyFile
        [DataContract]
        public class ModifyFileData
        {
            [DataMember]
            public string Name;
            [DataMember]
            public string Description;
        };

        [OperationContract]
        [WebInvoke(UriTemplate = "Files/{id}", Method = "PUT")]
        public void ModifyFile(string id, ModifyFileData file)
        {
            int iid = int.Parse(id);
            using (MooDB db = new MooDB())
            {
                UploadedFile theFile = (from f in db.UploadedFiles
                                        where f.ID == iid
                                        select f).SingleOrDefault<UploadedFile>();
                if (theFile == null) throw new ArgumentException("无此文件");

                Access.Required(db, theFile, Function.ModifyFile);

                if (file.Name != null)
                {
                    theFile.Name = file.Name;
                }
                if (file.Description != null)
                {
                    theFile.Description = file.Description;
                }

                db.SaveChanges();
            }
        }
        #endregion

        [OperationContract]
        [WebInvoke(UriTemplate = "Files/{id}", Method = "DELETE")]
        public void DeleteFile(string id)
        {
            int iid = int.Parse(id);
            using (MooDB db = new MooDB())
            {
                UploadedFile file = (from f in db.UploadedFiles
                                     where f.ID == iid
                                     select f).SingleOrDefault<UploadedFile>();
                if (file == null) throw new ArgumentException("无此文件");

                var spj = from t in db.TestCases.OfType<SpecialJudgedTestCase>()
                          where t.Judger.ID == file.ID
                          select t;
                var interactive = from t in db.TestCases.OfType<InteractiveTestCase>()
                                  where t.Invoker.ID == file.ID
                                  select t;
                var answerOnly = from t in db.TestCases.OfType<AnswerOnlyTestCase>()
                                 where t.Judger.ID == file.ID
                                 select t;
                if (spj.Any() || interactive.Any() || answerOnly.Any())
                {
                    throw new ArgumentException("尚有测试点使用此文件");
                }

                Access.Required(db, file, Function.DeleteFile);

                File.Delete(Config.UploadFileDirectory + file.FileName);

                db.UploadedFiles.DeleteObject(file);
                db.SaveChanges();
            }
        }
        #endregion
    }
}
