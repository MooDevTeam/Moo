using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using Moo.Core.DB;
namespace Moo.Core.Security
{
    /// <summary>
    /// 请求期间的简略当前用户登录信息
    /// </summary>
    public class SiteUser : IIdentity
    {
        public string AuthenticationType { get { return "Custom"; } }
        public bool IsAuthenticated { get { return true; } }

        public Guid ID { get; set; }
        public int Token { get; set; }
        public string Name { get; set; }
        public SiteRole Role { get; set; }

        public User GetDBUser(MooDB db)
        {
            return (from u in db.Users
                    where u.ID == ID
                    select u).Single<User>();
        }

        public void Initialize(User user)
        {
            ID = user.ID;
            Name = user.Name;
            Role = (SiteRole)Enum.Parse(typeof(SiteRole), user.Role.Name);
        }
    }
}