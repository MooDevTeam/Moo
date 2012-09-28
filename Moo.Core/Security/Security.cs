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
    }
}
