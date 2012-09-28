using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
namespace Moo.API.API
{
    [DataContract]
    public class FullPost
    {
        [DataMember]
        public Guid? ID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public bool? OnTop { get; set; }

        [DataMember]
        public DateTime? ReplyTime { get; set; }

        [DataMember]
        public Guid? Problem { get; set; }
    }
}