using System;
using System.Collections.Generic;
using Zazz.Core.Models;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;

namespace Zazz.Web.Models
{
    public class FeedViewModel
    {
        public int FeedId { get; set; }

        public FeedType FeedType { get; set; }

        /// <summary>
        /// Image url of owner of the feed. Not necessarily the current user.
        /// </summary>
        public PhotoLinks UserImageUrl { get; set; }

        /// <summary>
        /// Owner of this feed. Not necessarily the current user.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Display name of owner of the feed. Not necessarily the current user.
        /// </summary>
        public string UserDisplayName { get; set; }

        public bool IsFromCurrentUser { get; set; }

        public bool CurrentUserCanRemoveFeed { get; set; }

        public EventViewModel EventViewModel { get; set; }

        public List<PhotoViewModel> PhotoViewModel { get; set; }

        public PostViewModel PostViewModel { get; set; }

        public CommentsViewModel CommentsViewModel { get; set; }

        public DateTime Time { get; set; }
    }
}