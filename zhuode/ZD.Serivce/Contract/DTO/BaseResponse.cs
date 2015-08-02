using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace ZD.Serivce.Contract
{
    [DataContract]
    public class BaseResponse
    {
        [DataMember]
        public bool IsSuccess { get; set; }
        [DataMember]
        public int ErrorType { get; set; }
        [DataMember]
        public string ErrorMessage { get; set; }
    }
}