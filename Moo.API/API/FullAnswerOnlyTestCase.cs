using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
namespace Moo.API.API
{
    public class FullAnswerOnlyTestCase:FullTestCase
    {
        [DataMember]
        public byte[] TestData { get; set; }

        [DataMember]
        public Guid? Judger { get; set; }
    }
}