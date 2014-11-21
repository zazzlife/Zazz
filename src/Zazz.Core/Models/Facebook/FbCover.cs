using System.Runtime.Serialization;

namespace Zazz.Core.Models.Facebook
{
    [DataContract]
    public class FbCover
    {
        [DataMember(Name = "cover_id")]
        public long coverid { get; set; }

        [DataMember(Name = "source")]
        public string coverlink { get; set; }

        [DataMember(Name = "offset_y")]
        public float offsetY { get; set; }

        [DataMember(Name = "offset_x")]
        public float offsetX { get; set; }
    }
}
