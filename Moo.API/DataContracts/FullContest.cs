using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
namespace Moo.API.DataContracts
{
    [DataContract]
    public class FullContest
    {
        [DataMember]
        public int? ID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public DateTime? StartTime { get; set; }

        [DataMember]
        public DateTime? EndTime { get; set; }

        [DataMember]
        public string Status { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public bool? LockProblemOnStart { get; set; }

        [DataMember]
        public bool? LockTestCaseOnStart { get; set; }

        [DataMember]
        public bool? LockPostOnStart { get; set; }

        [DataMember]
        public bool? HideTestCaseOnStart { get; set; }

        [DataMember]
        public bool? EnableTestingOnStart { get; set; }

        [DataMember]
        public bool? HideProblemOnStart { get; set; }

        [DataMember]
        public bool? LockRecordOnStart { get; set; }

        [DataMember]
        public bool? LockArticleOnStart { get; set; }

        [DataMember]
        public bool? LockProblemOnEnd { get; set; }

        [DataMember]
        public bool? LockTestCaseOnEnd { get; set; }

        [DataMember]
        public bool? LockPostOnEnd { get; set; }

        [DataMember]
        public bool? HideTestCaseOnEnd { get; set; }

        [DataMember]
        public bool? EnableTestingOnEnd { get; set; }

        [DataMember]
        public bool? HideProblemOnEnd { get; set; }

        [DataMember]
        public bool? LockRecordOnEnd { get; set; }

        [DataMember]
        public bool? LockArticleOnEnd { get; set; }

        [DataMember]
        public bool? ViewResultAnyTime { get; set; }

        [DataMember]
        public bool? HideJudgeInfoOnStart { get; set; }

        [DataMember]
        public bool? HideJudgeInfoOnEnd { get; set; }

        [DataMember]
        public List<int> User { get; set; }

        [DataMember]
        public List<int> Problem { get; set; }
    }
}