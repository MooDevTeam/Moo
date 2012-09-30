﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
namespace Moo.API.DataContracts
{
    [DataContract]
    public class BriefArticleRevision
    {
        [DataMember]
        public int? ID { get; set; }

        [DataMember]
        public DateTime? CreateTime { get; set; }

        [DataMember]
        public int? CreatedBy { get; set; }

        [DataMember]
        public int? Article { get; set; }
    }
}