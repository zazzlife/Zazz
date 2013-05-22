using System;
using System.Collections.Generic;
using Zazz.Core.Models;
using Zazz.Core.Models.Data.Enums;

namespace Zazz.Web.Models.Api
{
    public class FeedApiResponse
    {
        public int UserId { get; set; }

        public PhotoLinks UserDisplayPhoto { get; set; }

        public string UserDisplayName { get; set; }

        public bool CanCurrentUserRemoveFeed { get; set; }

        public FeedType FeedType { get; set; }

        public DateTime Time { get; set; }

        public IEnumerable<PhotoApiModel> Photos { get; set; }

        public PostApiModel Post { get; set; }

        public EventApiModel Event { get; set; }

        public IEnumerable<CommentApiModel> Comments { get; set; }
    }
}