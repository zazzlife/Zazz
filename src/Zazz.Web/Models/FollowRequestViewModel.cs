namespace Zazz.Web.Models
{
    public class FollowRequestViewModel
    {
        public int RequestId { get; set; }

        public int FromUserId { get; set; }

        public string FromUserPictureUrl { get; set; }

        public string FromUsername { get; set; }
    }
}