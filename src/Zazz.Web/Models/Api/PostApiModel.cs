using Zazz.Core.Models;

namespace Zazz.Web.Models.Api
{
    public class PostApiModel
    {
        public int PostId { get; set; }

        public string Message { get; set; }

        public int? ToUserId { get; set; }

        public string ToUserDisplayName { get; set; }

        public PhotoLinks ToUserPhotoUrl { get; set; }
    }
}