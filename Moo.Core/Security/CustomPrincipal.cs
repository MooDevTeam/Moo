using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;

namespace Moo.Core.Security
{
    /// <summary>
    /// 自定义的用户权限管理
    /// </summary>
    public class CustomPrincipal : IPrincipal
    {
        public IIdentity Identity { get; set; }
        
        public bool IsInRole(string role)
        {
            SiteUser user = Identity as SiteUser;
            return user.Role.Name == role;
        }

        public bool IsAllowed(string function)
        {
            SiteUser user=Identity as SiteUser;
            return user.Role.AllowedFunction.Contains(function);
        }
    }

}