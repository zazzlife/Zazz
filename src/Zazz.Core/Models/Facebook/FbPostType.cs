using System.Runtime.Serialization;

namespace Zazz.Core.Models.Facebook
{
    [DataContract]
    public enum FbPostType
    {
        [EnumMember(Value = "link")]
        Link,
        [EnumMember(Value = "photo")]
        Photo,
        [EnumMember(Value = "video")]
        Video
    }
}