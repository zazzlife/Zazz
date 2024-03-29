﻿using System.Collections.Generic;
using Zazz.Core.Models.Data.Enums;
using Zazz.Web.Models;
using Zazz.Web.Models.Api;

namespace Zazz.Web.Interfaces
{
    public interface IFeedHelper
    {
        int PageSize { get; set; }

        /// <summary>
        /// Retruns all recent feeds that contain the provided categories.
        /// </summary>
        /// <param name="currentUserId">Current User Id</param>
        /// <param name="tagIds">List of tag ids to filter feeds.</param>
        /// <param name="lastFeedId">id of the last feed. if 0 it loads the most recent feeds else it loads the most recent feeds prior to the provided feed id</param>
        /// <returns></returns>
        FeedsViewModel GetCategoryFeeds(int currentUserId, List<byte> catIds, int lastFeedId = 0, List<int> tags = null, bool userFollow = true, bool inSameCity = true);

        /// <summary>
        /// Returns a list of activities of people that the user follows and the user himself
        /// </summary>
        /// <param name="currentUserId"></param>
        /// <param name="lastFeedId">id of the last feed. if 0 it loads the most recent feeds else it loads the most recent feeds prior to the provided feed id</param>
        /// <returns></returns>
        FeedsViewModel GetFeeds(int currentUserId, int lastFeedId = 0, bool userFollow = true, bool inSameCity = true);

        /// <summary>
        /// Returns a list of user activities
        /// </summary>
        /// <param name="userId">Id of the target user</param>
        /// <param name="currentUserId">Id of the current user</param>
        /// <param name="lastFeedId">id of the last feed. if 0 it loads the most recent feeds else it loads the most recent feeds prior to the provided feed id</param>
        /// <returns></returns>
        FeedsViewModel GetUserActivityFeed(int userId, int currentUserId,
                                                                int lastFeedId = 0);

        /// <summary>
        /// Returns feeds of received likes
        /// </summary>
        /// <param name="userId">Id of the target user</param>
        /// <param name="currentUserId">Id of the current user</param>
        /// <param name="lastFeedId">id of the last feed. if 0 it loads the most recent feeds else it loads the most recent feeds prior to the provided feed id</param>
        /// <returns></returns>
        FeedsViewModel GetUserLikedFeed(int userId, int currentUserId,
                                                                int lastFeedId = 0);
        
        FeedViewModel GetSinglePostFeed(int postId, int currentUserId);

        List<CommentViewModel> GetComments(int id, CommentType commentType, int currentUserId,
                                                           int lastComment = 0, int pageSize = 5);

        IEnumerable<PostMsgItemViewModel> GetPostMsgItems(string message);
    }
}