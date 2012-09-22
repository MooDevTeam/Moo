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
        public Guid? ID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Type { get; set; }

        [DataMember]
        public int? SubmissionCount { get; set; }

        [DataMember]
        public double? AverageScore { get; set; }

        [DataMember]
        public int? SubmissionUser { get; set; }

        [DataMember]
        public int? MaximumScore { get; set; }

        [DataMember]
        public int? MyScore { get; set; }

        [DataMember]
        public Guid? LatestRevision { get; set; }

        [DataMember]
        public Guid? LatestSolution { get; set; }
    }
}