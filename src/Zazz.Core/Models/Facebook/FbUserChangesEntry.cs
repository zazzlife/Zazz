using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Zazz.Core.Models.Facebook
{
    [DataContract]
    public class FbUserChangesEntry
    {
        [DataMember(Name = "uid")]
        public long UserId { get; set; }

        [DataMember(Name = "changed_fields")]
        public IEnumerable<string> ChangedFields { get; set; }
    }
}