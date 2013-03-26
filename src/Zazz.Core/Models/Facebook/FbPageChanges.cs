using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Zazz.Core.Models.Facebook
{
    [DataContract]
    public class FbPageChanges
    {
        [DataMember(Name = "entry")]
        public IEnumerable<FbPageChangesEntry> Entries { get; set; }
    }
}