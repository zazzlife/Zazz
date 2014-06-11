using System.Collections.Generic;
using Zazz.Core.Models;

namespace Zazz.Web.Models
{
    public class PostViewModel
    {
        public int PostId { get; set; }

        public string PostText { get; set; }

        public int? ToUserId { get; set; }

        public string ToUserDisplayName { get; set; }

        public PhotoLinks ToUserDisplayPhoto { get; set; }

        public IEnumerable<string> Categories { get; set; }

        public static string GetImgName(string Name) {
            return Name.ToLower().Replace(' ', '_').Replace('-', '_');
        }
    }
}