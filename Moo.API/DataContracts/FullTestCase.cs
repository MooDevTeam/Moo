using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
namespace Moo.API.DataContracts
{
    [DataContract]
    [KnownType(typeof(FullTraditionalTestCase))]
    [KnownType(typeof(FullSpecialJudgedTestCase))]
    [KnownType(typeof(FullInteractiveTestCase))]
    [KnownType(typeof(FullAnswerOnlyTestCase))]
    public class FullTestCase
    {
        [DataMember]
        public int? ID { get; set; }

        [DataMember]
        public int? CreatedBy { get; set; }

        [DataMember]
        public int? Problem { get; set; }
    }
}