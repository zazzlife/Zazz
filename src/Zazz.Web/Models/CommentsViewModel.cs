using System.Collections.Generic;
using Zazz.Core.Models.Data;

namespace Zazz.Web.Models
{
    /// <summary>
    /// View Model of Comments for a certain feed.
    /// </summary>
    public class CommentsViewModel
    {
        public string CurrentUserPhotoUrl { get; set; }

        /// <summary>
        /// This is id of the item that the user can comment on. (post/event/photo/...)
        /// </summary>
        public int ItemId { get; set; }

        public FeedType FeedType { get; set; }

        public List<CommentViewModel> Comments { get; set; }
    }
}