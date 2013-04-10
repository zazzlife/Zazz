using System;
using System.Collections.Generic;
using Zazz.Core.Models.Data;

namespace Zazz.Web.Models
{
    public class FeedViewModel
    {
        public FeedType FeedType { get; set; }

        public string UserImageUrl { get; set; }

        public int UserId { get; set; }

        public string UserDisplayName { get; set; }

        public bool IsFromCurrentUser { get; set; }

        public EventViewModel EventViewModel { get; set; }

        public List<PhotoViewModel> PhotoViewModel { get; set; }

        public PostViewModel PostViewModel { get; set; }

        public CommentsViewModel CommentsViewModel { get; set; }

        public DateTime Time { get; set; }
    }
}