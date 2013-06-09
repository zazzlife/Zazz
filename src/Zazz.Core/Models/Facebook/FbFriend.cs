using System.Runtime.Serialization;

namespace Zazz.Core.Models.Facebook
{
    [DataContract]
    public class FbFriend
    {
        [DataMember(Name = "uid")]
        public long Id { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "pic")]
        public string Photo { get; set; }

        [DataMember(Name = "can_message")]
        public bool CanMessage { get; set; }
    }
}