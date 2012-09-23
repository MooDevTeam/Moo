using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
using Moo.Core.DB;
namespace Moo.API.API
{
    [DataContract]
    public class FullUser
    {
        [DataMember]
        public Guid? ID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Password { get; set; }

        [DataMember]
        public string BriefDescription { get; set; }

        [DataMember]
        public string Email { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public int? Score { get; set; }

        [DataMember]
        public string PreferredLanguage { get; set; }

        [DataMember]
        public List<Guid> Role { get; set; }
    }
}