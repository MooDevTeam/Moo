using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moo.Core.DB;
namespace Moo.Core.Security
{
    public class SiteRoles
    {
        MooDB db;

        Role aclManager, roleManager, siteManager, contributor, reader;

        public Role ACLManger
        {
            get
            {
                if (aclManager == null)
                {
                    aclManager = (from r in db.Roles
                                  where r.Name == "ACLManager"
                                  select r).Single<Role>();
                }
                return aclManager;
            }
        }

        public Role RoleManger
        {
            get
            {
                if (roleManager == null)
                {
                    roleManager = (from r in db.Roles
                                   where r.Name == "RoleManager"
                                   select r).Single<Role>();
                }
                return roleManager;
            }
        }

        public Role SiteManger
        {
            get
            {
                if (siteManager == null)
                {
                    siteManager = (from r in db.Roles
                                   where r.Name == "SiteManager"
                                   select r).Single<Role>();
                }
                return siteManager;
            }
        }

        public Role Contributor
        {
            get
            {
                if (contributor == null)
                {
                    contributor = (from r in db.Roles
                                   where r.Name == "Contributor"
                                   select r).Single<Role>();
                }
                return contributor;
            }
        }

        public Role Reader
        {
            get
            {
                if (reader == null)
                {
                    reader = (from r in db.Roles
                              where r.Name == "Reader"
                              select r).Single<Role>();
                }
                return reader;
            }
        }

        public SiteRoles(MooDB db)
        {
            this.db = db;
        }
    }
}
