using System.Runtime.Serialization;

namespace Zazz.Core.Models.Facebook
{
    [DataContract]
    public class FbPageChangedFieldValue
    {
        [DataMember(Name = "item")]
        public string Item { get; set; }

        [DataMember(Name = "verb")]
        public string Verb { get; set; }

        [DataMember(Name = "user_id")]
        public long UserId { get; set; }
    }
}