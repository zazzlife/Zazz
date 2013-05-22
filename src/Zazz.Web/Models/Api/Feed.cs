using System;
using Zazz.Core.Models;
using Zazz.Core.Models.Data.Enums;

namespace Zazz.Web.Models.Api
{
    public abstract class BaseFeedApiResponse
    {
        public int UserId { get; set; }

        public PhotoLinks UserDisplayPhoto { get; set; }

        public string UserDisplayName { get; set; }

        public bool CanCurrentUserRemoveFeed { get; set; }

        public FeedType FeedType { get; set; }

        public DateTime Time { get; set; }
    }
}