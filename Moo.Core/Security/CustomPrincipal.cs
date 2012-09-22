using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;

namespace Moo.Core.Security
{
    /// <summary>
    /// 自定义的Principal
    /// </summary>
    public class CustomPrincipal : IPrincipal
    {
        public IIdentity Identity { get; set; }
        
        public bool IsInRole(string role)
        {
            throw new NotImplementedException();
        }
    }
}