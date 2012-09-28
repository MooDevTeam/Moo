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
        public Role Organizer
        {
            get
            {
                return (from r in db.Roles
                        where r.Name == "Organizer"
                        select r).Single<Role>();
            }
        }
        public Role Worker
        {
            get
            {
                return (from r in db.Roles
                        where r.Name == "Worker"
                        select r).Single<Role>();
            }
        }
        public Role NormalUser
        {
            get
            {
                return (from r in db.Roles
                        where r.Name == "NormalUser"
                        select r).Single<Role>();
            }
        }
        public Role Reader
        {
            get
            {
                return (from r in db.Roles
                        where r.Name == "Reader"
                        select r).Single<Role>();
            }
        }
        public SiteRoles(MooDB db)
        {
            this.db = db;
        }
    }
}
