using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
namespace Moo.API.API
{
    [DataContract]
    public class BriefRecord
    {
        [DataMember]
        public Guid? ID;

        [DataMember]
        public string Language;

        [DataMember]
        public bool? PublicCode;

        [DataMember]
        public DateTime? CreateTime;

        [DataMember]
        public Guid? Problem;

        [DataMember]
        public Guid? JudgeInfo;

        [DataMember]
        public Guid? User;
    }
}