using System;
using System.Runtime.Serialization;

namespace Zazz.Core.Models.Facebook
{
    [DataContract]
    public class FbComment
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "from")]
        public FbUser From { get; set; }

        [DataMember(Name = "message")]
        public string Message { get; set; }

        [DataMember(Name = "created_time")]
        public DateTime CreatedTime { get; set; }
    }
}