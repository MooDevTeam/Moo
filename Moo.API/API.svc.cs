using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Security;
using System.IO;
using Moo.Core.DB;
using Moo.Core.Security;
using Moo.Core.Utility;
namespace Moo.API
{
    [ServiceBehavior]
    public class API : IAPI
    {
        static object loginLock = new object();
        static ISet<SiteUser> trustedUsers = new HashSet<SiteUser>();
        static IDictionary<int, DateTimeOffset> lastAccess = new Dictionary<int, DateTimeOffset>();

        bool Authenticate(string sToken, bool throwIfFailure = true)
        {
            string[] splited = sToken.Split(',');
            int userID = int.Parse(splited[0]), token = int.Parse(splited[1]);
            if (!SiteUsers.ByID.ContainsKey(userID) || SiteUsers.ByID[userID].Token != token)
            {
                if (throwIfFailure)
                {
                    throw new SecurityException("身份验证失败");
                }
                else
                {
                    return false;
                }
            }

            if (SiteUsers.ByID[userID].Role.Type != RoleType.Organizer && !trustedUsers.Contains(SiteUsers.ByID[userID]))
            {
                lock (SiteUsers.ByID[userID])
                {
                    if (lastAccess.ContainsKey(userID))
                    {
                        if (lastAccess[userID] > DateTimeOffset.Now.AddSeconds(-10))
                        {
                            Thread.Sleep(lastAccess[userID].AddSeconds(10) - DateTimeOffset.Now);
                        }
                    }
                    else
                    {
                        lastAccess[userID] = DateTimeOffset.Now;
                    }

                    lastAccess[userID] = DateTimeOffset.Now;
                }
            }
            Thread.CurrentPrincipal = new CustomPrincipal() { Identity = SiteUsers.ByID[userID] };
            return true;
        }

        bool Authorize(string permission, bool throwIfFailure = true)
        {
            SiteUser siteUser = (SiteUser)((CustomPrincipal)Thread.CurrentPrincipal).Identity;
            if (siteUser.Role.AllowedFunction.Contains(permission))
            {
                return true;
            }
            else
            {
                if (throwIfFailure)
                {
                    throw new SecurityException("权限不足");
                }
                else
                {
                    return false;
                }
            }
        }

        SiteUser CurrentUser
        {
            get
            {
                return (SiteUser)((CustomPrincipal)Thread.CurrentPrincipal).Identity;
            }
        }

        public string Login(string userName, string password)
        {
            lock (loginLock)
            {
                password = Converter.ToSHA256Hash(password);
                using (MooDB db = new MooDB())
                {
                    User user = (from u in db.Users
                                 where u.Name == userName && u.Password == password
                                 select u).SingleOrDefault<User>();
                    if (user == null)
                    {
                        Thread.Sleep(5000);
                        return null;
                    }

                    int token = new Random().Next();
                    if (SiteUsers.ByID.ContainsKey(user.ID))
                    {
                        SiteUsers.ByID[user.ID].Initialize(user);
                        SiteUsers.ByID[user.ID].Token = token;
                    }
                    else
                    {
                        SiteUsers.ByID[user.ID] = new SiteUser(user) { Token = token };
                    }
                    return user.ID + "," + token;
                }
            }
        }

        public void AddTrustedUser(string sToken, int userID)
        {
            Authenticate(sToken);
            if (CurrentUser.Role.Type != RoleType.Organizer) Authorize("");

            if (!SiteUsers.ByID.ContainsKey(userID))
            {
                using (MooDB db = new MooDB())
                {
                    User user = (from u in db.Users
                                 where u.ID == userID
                                 select u).SingleOrDefault<User>();
                    if (user == null) throw new ArgumentException("无此用户");
                    SiteUsers.ByID[userID] = new SiteUser(user);
                }
            }

            if (!trustedUsers.Contains(SiteUsers.ByID[userID]))
            {
                trustedUsers.Add(SiteUsers.ByID[userID]);
            }
            else throw new ArgumentException("用户已在列表中");
        }

        public void RemoveTrustedUser(string sToken, int userID)
        {
            Authenticate(sToken);
            if (CurrentUser.Role.Type != RoleType.Organizer) Authorize("");
            if (!SiteUsers.ByID.ContainsKey(userID)) throw new ArgumentException("用户不在列表中");

            if (trustedUsers.Contains(SiteUsers.ByID[userID]))
            {
                trustedUsers.Remove(SiteUsers.ByID[userID]);
            }
        }

        public List<BriefUserInfo> ListTrustedUser(string sToken)
        {
            Authenticate(sToken);
            if (CurrentUser.Role.Type != RoleType.Organizer) Authorize("");
            return (from u in trustedUsers
                    select new BriefUserInfo()
                    {
                        ID = u.ID,
                        Name = u.Name
                    }).ToList();
        }

        public BriefUserInfo GetUserByName(string sToken, string userName)
        {
            Authenticate(sToken);
            using (MooDB db = new MooDB())
            {
                return (from u in db.Users
                        where u.Name == userName
                        select new BriefUserInfo()
                        {
                            ID = u.ID,
                            Name = u.Name
                        }).SingleOrDefault<BriefUserInfo>();
            }
        }

        public int CreateProblem(string sToken, string name, string type, string content)
        {
            Authenticate(sToken);
            Authorize("problem.create");
            using (MooDB db = new MooDB())
            {
                User currentUser = CurrentUser.GetDBUser(db);

                Problem problem = new Problem()
                {
                    Name = name,
                    Type = type,
                    AllowTesting = true,
                    Hidden = false,
                    Lock = false,
                    LockPost = false,
                    LockRecord = false,
                    LockSolution = false,
                    LockTestCase = false,
                    MaximumScore = null,
                    ScoreSum = 0,
                    SubmissionCount = 0,
                    SubmissionUser = 0,
                    TestCaseHidden = false
                };

                ProblemRevision revision = new ProblemRevision()
                {
                    Content = content,
                    CreatedBy = currentUser,
                    Reason = "创建题目",
                    Problem = problem
                };

                SolutionRevision solution = new SolutionRevision()
                {
                    Content = "暂无题解",
                    CreatedBy = currentUser,
                    Reason = "创建题解",
                    Problem = problem
                };

                problem.LatestRevision = revision;
                problem.LatestSolution = solution;
                db.SaveChanges();

                return problem.ID;
            }
        }

        public int CreateTranditionalTestCase(string sToken, int problemID, byte[] input, byte[] answer, int timeLimit, int memoryLimit, int score)
        {
            Authenticate(sToken);

            using (MooDB db = new MooDB())
            {
                Problem problem = (from p in db.Problems
                                   where p.ID == problemID
                                   select p).SingleOrDefault<Problem>();
                if (problem == null) throw new ArgumentException("无此题目");
                if (problem.Type != "Tranditional") throw new ArgumentException("不是传统题");

                if (problem.LockTestCase)
                {
                    Authorize("testcase.locked.create");
                }
                else
                {
                    Authorize("testcase.create");
                }

                User currentUser = CurrentUser.GetDBUser(db);

                TestCase testCase = new TranditionalTestCase()
                {
                    Answer = answer,
                    CreatedBy = currentUser,
                    Input = input,
                    MemoryLimit = memoryLimit,
                    TimeLimit = timeLimit,
                    Problem = problem,
                    Score = score
                };

                db.TestCases.AddObject(testCase);
                db.SaveChanges();

                return testCase.ID;
            }
        }

        public int CreateSpecialJudgedTestCase(string sToken, int problemID, int judgerID, byte[] input, byte[] answer, int timeLimit, int memoryLimit)
        {
            Authenticate(sToken);
            using (MooDB db = new MooDB())
            {
                Problem problem = (from p in db.Problems
                                   where p.ID == problemID
                                   select p).SingleOrDefault<Problem>();
                if (problem == null) throw new ArgumentException("无此题目");
                if (problem.Type != "SpecialJudged") throw new ArgumentException("不是自定义测评题");

                if (problem.LockTestCase)
                {
                    Authorize("testcase.locked.create");
                }
                else
                {
                    Authorize("testcase.create");
                }

                UploadedFile judger = (from f in db.UploadedFiles
                                       where f.ID == judgerID
                                       select f).SingleOrDefault<UploadedFile>();
                if (judger == null) throw new ArgumentException("无此文件");

                User currentUser = CurrentUser.GetDBUser(db);

                TestCase testCase = new SpecialJudgedTestCase()
                {
                    Answer = answer,
                    Input = input,
                    CreatedBy = currentUser,
                    Judger = judger,
                    MemoryLimit = memoryLimit,
                    Problem = problem,
                    TimeLimit = timeLimit,
                };

                db.TestCases.AddObject(testCase);
                db.SaveChanges();

                return testCase.ID;
            }
        }
    }
}