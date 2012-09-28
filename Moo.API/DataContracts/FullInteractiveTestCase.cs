using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
namespace Moo.API.DataContracts
{
    public class FullInteractiveTestCase:FullTestCase
    {
        [DataMember]
        public byte[] TestData { get; set; }

        [DataMember]
        public int? TimeLimit { get; set; }

        [DataMember]
        public int? MemoryLimit { get; set; }

        [DataMember]
        public int? Invoker { get; set; }
    }
}