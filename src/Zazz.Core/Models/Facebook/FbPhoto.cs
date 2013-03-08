using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Zazz.Core.Models.Facebook
{
    [DataContract]
    public class FbPhoto : FbImage
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "from")]
        public FbUser From { get; set; }

        [DataMember(Name = "images")]
        public IEnumerable<FbImage> Images { get; set; }

        [DataMember(Name = "created_time")]
        public DateTime CreatedTime { get; set; }
    }
}