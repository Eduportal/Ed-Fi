using System;
using System.Runtime.Serialization;

namespace EdFi.Ods.Common.ExceptionHandling
{
    [Serializable, DataContract]
    public class RESTError
    {
        [DataMember(Name = "code")]
        public int Code { get; set; }
        [DataMember(Name = "type")]
        public string Type { get; set; }
        [DataMember(Name = "message")]
        public string Message { get; set; }
    }
}