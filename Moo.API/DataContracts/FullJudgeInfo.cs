using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
namespace Moo.API.DataContracts
{
    [DataContract]
    public class FullJudgeInfo
    {
        [DataMember]
        public int? ID { get; set; }

        [DataMember]
        public int? Score { get; set; }

        [DataMember]
        public string Info { get; set; }

        [DataMember]
        public int? Record { get; set; }
    }
}