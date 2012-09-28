using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
namespace Moo.API.DataContracts
{
    [DataContract]
    public class FullPost
    {
        [DataMember]
        public int? ID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public bool? OnTop { get; set; }

        [DataMember]
        public DateTime? ReplyTime { get; set; }

        [DataMember]
        public int? Problem { get; set; }
    }
}