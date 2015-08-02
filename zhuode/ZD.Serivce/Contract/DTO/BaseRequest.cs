using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace ZD.Serivce.Contract
{
    [DataContract]
    public class BaseRequest
    {
        [DataMember]
        public string Token { get; set; }
    }
}