using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
namespace Moo.API.API
{
    [DataContract]
    public class FullProblem
    {
        [DataMember]
        public int ID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Type { get; set; }

        [DataMember]
        public bool Lock { get; set; }

        [DataMember]
        public bool LockSolution { get; set; }

        [DataMember]
        public bool LockTestCase { get; set; }

        [DataMember]
        public bool LockPost { get; set; }

        [DataMember]
        public bool LockRecord { get; set; }

        [DataMember]
        public bool AllowTesting { get; set; }

        [DataMember]
        public bool Hidden{get;set;}

        [DataMember]
        public bool TestCaseHidden { get; set; }

        [DataMember]
        public int SubmissionCount { get; set; }

        [DataMember]
        public long ScoreSum { get; set; }

        [DataMember]
        public int SubmissionUser { get; set; }

        [DataMember]
        public int? MaximumScore { get; set; }

        [DataMember]
        public int? LatestRevision { get; set; }

        [DataMember]
        public int? LatestSolution { get; set; }
    }
}