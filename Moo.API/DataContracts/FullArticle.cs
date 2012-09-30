using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
namespace Moo.API.DataContracts
{
    [DataContract]
    public class FullArticle
    {
        [DataMember]
        public int? ID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public DateTime? CreateTime { get; set; }

        [DataMember]
        public int? CreatedBy { get; set; }

        [DataMember]
        public int? Problem { get; set; }

        [DataMember]
        public int? Category { get; set; }

        [DataMember]
        public int? LatestRevision { get; set; }
    }
}