using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security;
using System.Security.Principal;
using System.Threading;
using Moo.Core.Utility;
using Moo.Core.DB;
using Moo.Core.Security;
namespace Moo.Core.Logic
{
    public static class Security
    {
        public static SiteUser CurrentUser
        {
            get
            {
                return (SiteUser)((CustomPrincipal)Thread.CurrentPrincipal).Identity;
            }
        }

        public static bool Authenticated
        {
            get
            {
                return Thread.CurrentPrincipal.Identity.IsAuthenticated;
            }
        }

        public static bool Authenticate(string token, bool throwIfFailure = true)
        {
            if (token == null)
            {
                Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity(""), new string[0]);
                return true;
            }

            string[] splited = token.Split(',');
            int userID = int.Parse(splited[0]), iToken = int.Parse(splited[1]);
            if (!SiteUsers.ByID.ContainsKey(userID) || SiteUsers.ByID[userID].Token != iToken)
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
            Thread.CurrentPrincipal = new CustomPrincipal() { Identity = SiteUsers.ByID[userID] };
            return true;
        }

        public static bool Authorize(string permission,bool allowAnonymous, bool throwIfFailure = true)
        {
            if (!Authenticated && allowAnonymous || Authenticated && CurrentUser.Role.AllowedFunction.Contains(permission))
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

        public static string Login(string userName, string password)
        {
            password = Converter.ToSHA256Hash(password);
            using (MooDB db = new MooDB())
            {
                User user = (from u in db.Users
                             where u.Name == userName && u.Password == password
                             select u).SingleOrDefault<User>();
                if (user == null)
                {
                    return null;
                }

                int token = Rand.RAND.Next();
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

        public static void Logout()
        {
            if (!Authenticated) throw new SecurityException("先登录，然后才能登出");
            SiteUsers.ByID.Remove(CurrentUser.ID);
        }
    }
}
