using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moo.Core.Security;
namespace Moo.Core.DB
{
    public partial class Record
    {
        public bool IsPublicCode(MooDB db)
        {
            return (from a in db.ACL
                    where a.Allowed && a.Object == ID
                        && a.Subject == SiteRoles.ReaderID && a.Function.ID == Security.Security.GetFunctionID("record.code.read")
                    select a).Any();
        }

        public void TogglePublicCode(MooDB db, bool value)
        {
            if (value)
            {
                db.ACL.AddObject(new ACE()
                {
                    Subject = new SiteRoles(db).Reader.ID,
                    Object = ID,
                    Allowed = true,
                    Function = Security.Security.GetFunction(db, "record.code.read")
                });
            }
            else
            {
                db.ACL.DeleteObject((from a in db.ACL
                                     where a.Function.ID == Security.Security.GetFunctionID("record.code.read") && a.Allowed
                                          && a.Object == ID && a.Subject == new SiteRoles(db).Reader.ID
                                     select a).Single());
            }
        }
    }
}
