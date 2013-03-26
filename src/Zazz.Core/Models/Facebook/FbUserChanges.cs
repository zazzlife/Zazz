using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Zazz.Core.Models.Facebook
{
    [DataContract]
    public class FbUserChanges
    {
        [DataMember(Name = "entry")]
        public IEnumerable<FbUserChangesEntry> Entries { get; set; }
    }
}