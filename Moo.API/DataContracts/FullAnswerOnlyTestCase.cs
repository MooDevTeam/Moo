using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
namespace Moo.API.DataContracts
{
    public class FullAnswerOnlyTestCase:FullTestCase
    {
        [DataMember]
        public byte[] TestData { get; set; }

        [DataMember]
        public int? Judger { get; set; }
    }
}