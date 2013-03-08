using System.Runtime.Serialization;

namespace Zazz.Core.Models.Facebook
{
    [DataContract]
    public class FbUser
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "email")]
        public string Email { get; set; }

        [DataMember(Name = "gender")]
        public string Gender { get; set; }
    }
}