using System.Runtime.Serialization;

namespace Zazz.Core.Models.Facebook
{
    [DataContract]
    public class FbPageChangedFieldValue
    {
        /*
         * Items:
         *  status
         *  post
         */

        [DataMember(Name = "item")]
        public string Item { get; set; }

        /*
        * Verbs:
        *  add
        * remove
        */

        [DataMember(Name = "verb")]
        public string Verb { get; set; }

        [DataMember(Name = "post_id")]
        public string PostId { get; set; }
    }
}