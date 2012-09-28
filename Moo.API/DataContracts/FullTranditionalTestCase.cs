using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
namespace Moo.API.DataContracts
{
    public class FullTranditionalTestCase : FullTestCase
    {
        [DataMember]
        public byte[] Input { get; set; }

        [DataMember]
        public byte[] Answer { get; set; }

        [DataMember]
        public int? TimeLimit { get; set; }

        [DataMember]
        public int? MemoryLimit { get; set; }

        [DataMember]
        public int? Score { get; set; }
    }
}