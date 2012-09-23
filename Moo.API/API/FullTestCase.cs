using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
namespace Moo.API.API
{
    [DataContract]
    [KnownType(typeof(FullTranditionalTestCase))]
    [KnownType(typeof(FullSpecialJudgedTestCase))]
    [KnownType(typeof(FullInteractiveTestCase))]
    [KnownType(typeof(FullAnswerOnlyTestCase))]
    public class FullTestCase
    {
        [DataMember]
        public Guid? ID { get; set; }

        [DataMember]
        public Guid? CreatedBy { get; set; }

        [DataMember]
        public Guid? Problem { get; set; }
    }
}