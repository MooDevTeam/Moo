using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
namespace Moo.API.DataContracts
{
    [DataContract]
    public class FullMail
    {
        [DataMember]
        public int? ID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public bool? IsRead { get; set; }

        [DataMember]
        public string Content { get; set; }

        [DataMember]
        public DateTime? CreateTime { get; set; }

        [DataMember]
        public int? To { get; set; }

        [DataMember]
        public int? From { get; set; }
    }
}