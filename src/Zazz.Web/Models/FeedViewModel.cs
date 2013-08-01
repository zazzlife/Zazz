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
        public PhotoLinks UserDisplayPhoto { get; set; }

        /// <summary>
        /// Owner of this feed. Not necessarily the current user.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Display name of owner of the feed. Not necessarily the current user.
        /// </summary>
        public string UserDisplayName { get; set; }

        public bool IsFromCurrentUser { get; set; }

        public bool CanCurrentUserRemoveFeed { get; set; }

        public EventViewModel Event { get; set; }

        public List<PhotoViewModel> Photos { get; set; }

        public PostViewModel Post { get; set; }

        public CommentsViewModel Comments { get; set; }

        public DateTime Time { get; set; }
    }
}