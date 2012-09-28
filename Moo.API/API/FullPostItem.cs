using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
namespace Moo.API.API
{
    [DataContract]
    public class FullPostItem
    {
        [DataMember]
        public Guid? ID { get; set; }

        [DataMember]
        public string Content { get; set; }

        [DataMember]
        public DateTime? CreateTime { get; set; }

        [DataMember]
        public Guid? CreatedBy { get; set; }

        [DataMember]
        public Guid? Post { get; set; }
    }
}