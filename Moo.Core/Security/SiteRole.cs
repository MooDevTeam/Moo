using System;
using System.Collections.Generic;
using System.Linq;
using Moo.Core.DB;
namespace Moo.Core.Security
{
    /// <summary>
    /// 长期驻留的用户角色信息
    /// </summary>
    public class SiteRole
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public RoleType Type { get; set; }
        public HashSet<string> AllowedFunction { get; set; }
        
        public SiteRole(Role role)
        {
            ID = role.ID;
            Name = role.Name;
            DisplayName = role.DisplayName;
            Type = (RoleType)Enum.Parse(typeof(RoleType), Name);
            AllowedFunction = new HashSet<string>();
            foreach (Function func in role.AllowedFunction)
            {
                AllowedFunction.Add(func.Name);
            }
        }

        public Role GetDBRole(MooDB db)
        {
            return (from r in db.Roles
                    where r.ID == ID
                    select r).Single<Role>();
        }
    }
}