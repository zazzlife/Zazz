using System.Runtime.Serialization;

namespace Zazz.Core.Models.Facebook
{
    [DataContract]
    public class FbVenue
    {
        [DataMember(Name = "street")]
        public string Street { get; set; }

        [DataMember(Name = "city")]
        public string City { get; set; }

        [DataMember(Name = "state")]
        public string State { get; set; }

        [DataMember(Name = "country")]
        public string Country { get; set; }

        [DataMember(Name = "latitude")]
        public float? Latitude { get; set; }

        [DataMember(Name = "longitude")]
        public float? Longitude { get; set; }
    }
}