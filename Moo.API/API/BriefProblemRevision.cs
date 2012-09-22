using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Moo.API.API
{
    public class BriefProblemRevision
    {
        public Guid? ID { get; set; }
        public Guid? CreatedBy { get; set; }
        public string Reason { get; set; }
        public Guid? Problem { get; set; }
    }
}