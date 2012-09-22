using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
namespace Moo.API.API
{
    [DataContract]
    public class FullJudgeInfo
    {
        [DataMember]
        public Guid? ID { get; set; }

        [DataMember]
        public int? Score { get; set; }

        [DataMember]
        public string Info { get; set; }

        [DataMember]
        public Guid? Record { get; set; }
    }
}