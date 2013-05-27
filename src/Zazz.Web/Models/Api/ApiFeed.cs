using System;
using System.Collections.Generic;
using Zazz.Core.Models;
using Zazz.Core.Models.Data.Enums;

namespace Zazz.Web.Models.Api
{
    public class ApiFeed
    {
        public int FeedId { get; set; }

        public int UserId { get; set; }

        public PhotoLinks UserDisplayPhoto { get; set; }

        public string UserDisplayName { get; set; }

        public bool CanCurrentUserRemoveFeed { get; set; }

        public FeedType FeedType { get; set; }

        public DateTime Time { get; set; }

        public IEnumerable<ApiPhoto> Photos { get; set; }

        public ApiPost Post { get; set; }

        public ApiEvent ApiEvent { get; set; }

        public IEnumerable<ApiComment> Comments { get; set; }
    }
}