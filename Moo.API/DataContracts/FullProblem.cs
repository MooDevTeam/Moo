using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
namespace Moo.API.DataContracts
{
    [DataContract]
    public class FullProblem
    {
        [DataMember]
        public int? ID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Type { get; set; }

        [DataMember]
        public DateTime? CreateTime { get; set; }

        [DataMember]
        public int? SubmissionTimes { get; set; }

        [DataMember]
        public double? AverageScore { get; set; }

        [DataMember]
        public int? SubmissionUser { get; set; }

        [DataMember]
        public int? MaximumScore { get; set; }

        [DataMember]
        public int? MyScore { get; set; }

        [DataMember]
        public int? LatestRevision { get; set; }

        [DataMember]
        public int? CreatedBy { get; set; }

        [DataMember]
        public bool? Hidden { get; set; }

        [DataMember]
        public bool? Locked { get; set; }

        [DataMember]
        public bool? RecordLocked { get; set; }

        [DataMember]
        public bool? PostLocked { get; set; }

        [DataMember]
        public bool? ArticleLocked { get; set; }

        [DataMember]
        public bool? TestCaseLocked { get; set; }

        [DataMember]
        public bool? EnableTesting { get; set; }

        [DataMember]
        public bool? TestCaseHidden { get; set; }

        [DataMember]
        public bool? JudgeInfoHidden { get; set; }
    }
}