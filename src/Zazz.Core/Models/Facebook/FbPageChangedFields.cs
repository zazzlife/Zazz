using System.Runtime.Serialization;

namespace Zazz.Core.Models.Facebook
{
    [DataContract]
    public class FbPageChangedFields
    {
        [DataMember(Name = "field")]
        public string Field { get; set; }

        [DataMember(Name = "value")]
        public FbPageChangedFieldValue Values { get; set; }
    }
}