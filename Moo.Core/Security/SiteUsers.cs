using System;
using System.Collections.Generic;
using System.Linq;
namespace Moo.Core.Security
{
    /// <summary>
    ///SiteUser 管理
    /// </summary>
    public static class SiteUsers
    {
        public static Dictionary<Guid, SiteUser> ByID { get; set; }

        static SiteUsers(){
            ByID = new Dictionary<Guid, SiteUser>();
        }
    }
}