using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
namespace Moo.API.DataContracts
{
    [DataContract]
    public class FullRecord
    {
        [DataMember]
        public int? ID;

        [DataMember]
        public string Code;

        [DataMember]
        public string Language;

        [DataMember]
        public bool? PublicCode;

        [DataMember]
        public DateTime? CreateTime;

        [DataMember]
        public int? Problem;

        [DataMember]
        public int? JudgeInfo;

        [DataMember]
        public int? User;
    }
}