using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Moo.Core.DB;
using Moo.Core.Utility;
namespace Moo.Core.Security
{
    public static class Security
    {
        static Dictionary<string, Guid> functionByName = new Dictionary<string, Guid>();
        static Dictionary<string, string> functionNames = new Dictionary<string, string>();

        public static Guid GetFunctionID(string name)
        {
            if (functionByName.ContainsKey(name))
            {
                return functionByName[name];
            }
            else
            {
                using (MooDB db = new MooDB())
                {
                    Function function = (from f in db.Functions
                                         where f.Name == name
                                         select f).SingleOrDefault<Function>();
                    if (function == null) throw new ArgumentException("不存在名为 " + name + " 的功能");
                    functionByName.Add(name, function.ID);
                    functionNames.Add(name, function.DisplayName);
                    return function.ID;
                }
            }
        }

        public static Function GetFunction(MooDB db,string name)
        {
            return (from f in db.Functions
                    where f.Name == name
                    select f).Single<Function>();
        }

        public static bool Authenticated
        {
            get { return Thread.CurrentPrincipal.Identity.IsAuthenticated; }
        }

        public static SiteUser CurrentUser
        {
            get { return Thread.CurrentPrincipal.Identity as SiteUser; }
        }

        public static string Login(string userName, string password)
        {
            password = Converter.ToSHA256Hash(password);
            using (MooDB db = new MooDB())
            {
                User user = (from u in db.Users
                             where u.Name == userName && u.Password == password
                             select u).SingleOrDefault<User>();
                if (user == null) return null;

                int token = Rand.RAND.Next();
                if (!SiteUsers.ByID.ContainsKey(user.ID))
                {
                    SiteUsers.ByID.Add(user.ID, new SiteUser());
                }
                SiteUsers.ByID[user.ID].Initialize(user);
                SiteUsers.ByID[user.ID].Token = token;
                return user.ID + "," + token;
            }
        }

        public static void Logout()
        {
            SiteUsers.ByID.Remove(CurrentUser.ID);
        }

        public static bool CheckPermission(List<Guid> subjects, Guid obj, string type, string permission)
        {
            using (MooDB db = new MooDB())
            {
                return CheckPermission(db, subjects, obj, type, permission);
            }
        }

        public static bool CheckPermission(MooDB db, List<Guid> subjects, Guid obj, string type, string permission)
        {
            return CheckPermission(db, subjects, obj, type, GetFunctionID(permission));
        }

        public static bool CheckPermission(MooDB db, Guid obj, string type, string permission)
        {
            return CheckPermission(db, CurrentUser.Subjects, obj, type, permission);
        }

        public static void RequirePermission(Guid obj, string type, string permission)
        {
            using (MooDB db = new MooDB())
            {
                RequirePermission(db, obj, type, permission);
            }
        }

        public static void RequirePermission(MooDB db, Guid obj, string type, string permission)
        {
            if (!CheckPermission(CurrentUser.Subjects, obj, type, permission))
            {
                throw new UnauthorizedAccessException("不具备 " + functionNames[permission] + " 权限");
            }
        }

        static bool CheckPermission(MooDB db, List<Guid> subjects, Guid obj, string type, Guid permission)
        {
            var aces = from a in db.ACL
                       where a.Function.ID == permission && subjects.Contains(a.Subject) && a.Object == obj
                       select a;
            bool? allowed = null;
            foreach (ACE ace in aces)
            {
                if (ace.Allowed)
                {
                    allowed = true;
                    break;
                }
                else
                {
                    allowed = false;
                }
            }

            if (allowed != null)
            {
                return (bool)allowed;
            }

            switch (type)
            {
                case "Problem":
                    return CheckPermission(db, subjects, Guid.Empty, null, permission);
                case "ProblemRevision":
                    ProblemRevision problemRevision = (from r in db.ProblemRevisions
                                                       where r.ID == obj
                                                       select r).Single<ProblemRevision>();
                    return CheckPermission(db, subjects, problemRevision.Problem.ID, "Problem", permission);
                case "User":
                    return CheckPermission(db, subjects, Guid.Empty, null, permission);
                case "Homepage":
                    return CheckPermission(db, subjects, Guid.Empty, null, permission);
                case "TestCase":
                    TestCase testCase = (from t in db.TestCases
                                         where t.ID == obj
                                         select t).Single<TestCase>();
                    return CheckPermission(db, subjects, testCase.Problem.ID, "Problem", permission);
                case "Record":
                    Record record = (from r in db.Records
                                     where r.ID == obj
                                     select r).Single<Record>();
                    return CheckPermission(db, subjects, record.Problem.ID, "Problem", permission);
                case "Post":
                    Post post = (from p in db.Posts
                                 where p.ID == obj
                                 select p).Single<Post>();
                    if (post.Problem != null)
                        return CheckPermission(db, subjects, post.Problem.ID, "Problem", permission);
                    else
                        return CheckPermission(db, subjects, Guid.Empty, null, permission);
                case "PostItem":
                    PostItem postItem = (from i in db.PostItems
                                         where i.ID == obj
                                         select i).Single<PostItem>();
                    return CheckPermission(db, subjects, postItem.Post.ID, "Post", permission);
                case "Mail":
                    return CheckPermission(db, subjects, Guid.Empty, null, permission);
                case "ACL":
                    return CheckPermission(db, subjects, Guid.Empty, null, permission);
                case "File":
                    return CheckPermission(db, subjects, Guid.Empty, null, permission);
                default:
                    return false;
            }
        }
    }
}
