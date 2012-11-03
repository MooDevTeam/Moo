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
        public static bool Authenticated
        {
            get { return Thread.CurrentPrincipal.Identity.IsAuthenticated; }
        }

        public static SiteUser CurrentUser
        {
            get { return Thread.CurrentPrincipal.Identity as SiteUser; }
        }

        public static void Authenticate(string sToken)
        {
            string[] splited = sToken.Split(',');
            int userID = int.Parse(splited[0]);
            int iToken = int.Parse(splited[1]);

            if (SiteUsers.ByID[userID].Token != iToken) throw new Exception();
            Thread.CurrentPrincipal = new CustomPrincipal() { Identity = SiteUsers.ByID[userID] };
        }

        public static string Login(int userID, string password)
        {
            password = Converter.ToSHA256Hash(password);
            using (MooDB db = new MooDB())
            {
                User user = (from u in db.Users
                             where u.ID==userID && u.Password == password
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
    }
}
