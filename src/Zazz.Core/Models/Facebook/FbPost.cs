using System;
using System.Runtime.Serialization;

namespace Zazz.Core.Models.Facebook
{
    public class FbPost
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "from")]
        public FbUser From { get; set; }

        [DataMember(Name = "picture")]
        public string PictureUrl { get; set; }

        [DataMember(Name = "link")]
        public string Link { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "created_time")]
        public DateTime CreatedTime { get; set; }
    }
}