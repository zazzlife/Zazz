using System;
using System.Runtime.Serialization;

namespace Zazz.Core.Models.Facebook
{
    [DataContract]
    public class FbEvent
    {
        [DataMember(Name = "eid")]
        public long Id { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "location")]
        public string Location { get; set; }

        [DataMember(Name = "pic_square")]
        public string Pic { get; set; }

        [DataMember(Name = "start_time")]
        public DateTimeOffset StartTime { get; set; }

        [DataMember(Name = "end_time")]
        public DateTimeOffset? EndTime { get; set; }

        [DataMember(Name = "venue")]
        public FbVenue Venue { get; set; }

        [DataMember(Name = "is_date_only")]
        public bool IsDateOnly { get; set; }

        [DataMember(Name = "updated_time")]
        public long UpdatedTime { get; set; }

        [DataMember(Name = "updated_time")]
        public FbCover CoverPic { get; set; }
    }
}