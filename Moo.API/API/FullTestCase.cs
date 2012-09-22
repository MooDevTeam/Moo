using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
namespace Moo.API.API
{
    [DataContract]
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