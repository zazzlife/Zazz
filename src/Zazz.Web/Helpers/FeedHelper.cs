using System;
using System.Collections.Generic;
using System.Linq;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Repositories;
using Zazz.Core.Interfaces.Services;
using Zazz.Core.Models;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;
using Zazz.Infrastructure;
using Zazz.Infrastructure.Helpers;
using Zazz.Web.Interfaces;
using Zazz.Web.Models;
using Zazz.Web.Models.Api;

namespace Zazz.Web.Helpers
{
    public class FeedHelper : IFeedHelper
    {
        private readonly IUoW _uow;
        private readonly IUserService _userService;
        private readonly IPhotoService _photoService;
        private readonly IDefaultImageHelper _defaultImageHelper;
        private readonly IStaticDataRepository _staticDataRepository;
        public int PageSize { get; set; }

        public FeedHelper(IUoW uow, IUserService userService, IPhotoService photoService,
            IDefaultImageHelper defaultImageHelper, IStaticDataRepository staticDataRepository)
        {
            _uow = uow;
            _userService = userService;
            _photoService = photoService;
            _defaultImageHelper = defaultImageHelper;
            _staticDataRepository = staticDataRepository;
            PageSize = 10;
        }

        /// <summary>
        /// Retruns all recent feeds that contain the provided categories.
        /// </summary>
        /// <param name="currentUserId">Current User Id</param>
        /// <param name="tagIds">List of tag ids to filter feeds.</param>
        /// <param name="lastFeedId">id of the last feed. if 0 it loads the most recent feeds else it loads the most recent feeds prior to the provided feed id</param>
        /// <returns></returns>
        public List<FeedViewModel> GetCategoryFeeds(int currentUserId, List<byte> tagIds, int lastFeedId = 0)
        {
            var query = _uow.FeedRepository.GetFeedsWithCategories(tagIds);

            if (lastFeedId != 0)
                query = query.Where(f => f.Id < lastFeedId);

            query = query.Take(PageSize);

            var feeds = query.ToList();
            return ConvertFeedsToFeedsViewModel(feeds, currentUserId);
        }

        /// <summary>
        /// Returns a list of activities of people that the user follows and the user himself
        /// </summary>
        /// <param name="currentUserId"></param>
        /// <param name="lastFeedId">id of the last feed. if 0 it loads the most recent feeds else it loads the most recent feeds prior to the provided feed id</param>
        /// <returns></returns>
        public List<FeedViewModel> GetFeeds(int currentUserId, int lastFeedId = 0)
        {
            var followIds = _uow.FollowRepository.GetFollowsUserIds(currentUserId).ToList();
            followIds.Add(currentUserId);

            var query = _uow.FeedRepository.GetFeeds(followIds);

            if (lastFeedId != 0)
                query = query.Where(f => f.Id < lastFeedId);


            query = query.OrderByDescending(f => f.Time)
                         .Take(PageSize);


            var feeds = query.ToList();
            return ConvertFeedsToFeedsViewModel(feeds, currentUserId);
        }

        /// <summary>
        /// Returns a list of user activities
        /// </summary>
        /// <param name="userId">Id of the target user</param>
        /// <param name="currentUserId">Id of the current user</param>
        /// <param name="lastFeedId">id of the last feed. if 0 it loads the most recent feeds else it loads the most recent feeds prior to the provided feed id</param>
        /// <returns></returns>
        public List<FeedViewModel> GetUserActivityFeed(int userId, int currentUserId,
                                                                        int lastFeedId = 0)
        {
            var query = _uow.FeedRepository.GetUserFeeds(userId);
            if (lastFeedId != 0)
                query = query.Where(f => f.Id < lastFeedId);

            query = query.OrderByDescending(f => f.Time)
                         .Take(PageSize);

            var feeds = query.ToList();
            return ConvertFeedsToFeedsViewModel(feeds, currentUserId);
        }

        public List<FeedViewModel> GetUserLikedFeed(int userId, int currentUserId, int lastFeedId = 0)
        {
            var query = _uow.FeedRepository.GetUserLikedFeeds(userId);
            if (lastFeedId != 0)
                query = query.Where(f => f.Id < lastFeedId);

            query = query.OrderByDescending(f => f.Time)
                         .Take(PageSize);

            var feeds = query.ToList();
            return ConvertFeedsToFeedsViewModel(feeds, currentUserId);
        }

        public FeedViewModel GetSinglePostFeed(int postId, int currentUserId)
        {
            var feed = _uow.FeedRepository.GetPostFeed(postId);

            if (feed == null)
                return null;

            return ConvertFeedToFeedViewModel(feed, currentUserId);
        }

        private List<FeedViewModel> ConvertFeedsToFeedsViewModel(IEnumerable<Feed> feeds, int currentUserId)
        {
            var vm = new List<FeedViewModel>();

            foreach (var feed in feeds)
            {
                vm.Add(ConvertFeedToFeedViewModel(feed, currentUserId));
            }

            return vm;
        }

        private FeedViewModel ConvertFeedToFeedViewModel(Feed feed, int currentUserId)
        {
            var currentUserPhotoUrl = _photoService.GetUserDisplayPhoto(currentUserId);
            var feedVm = new FeedViewModel
            {
                FeedId = feed.Id,
                FeedType = feed.FeedType,
                Time = feed.Time,
                Comments = new CommentsViewModel
                {
                    CurrentUserDisplayPhoto = currentUserPhotoUrl,
                }
            };

            #region Event
            if (feed.FeedType == FeedType.Event && feed.EventFeed != null)
            {
                // EVENT

                FillUserDetails(ref feedVm, feed.EventFeed.Event.UserId, currentUserId);

                feedVm.Event = new EventViewModel
                {
                    City = feed.EventFeed.Event.City,
                    CreatedDate = feed.EventFeed.Event.CreatedDate,
                    Description = feed.EventFeed.Event.Description,
                    Id = feed.EventFeed.Event.Id,
                    Latitude = feed.EventFeed.Event.Latitude,
                    Location = feed.EventFeed.Event.Location,
                    Longitude = feed.EventFeed.Event.Longitude,
                    Name = feed.EventFeed.Event.Name,
                    Price = feed.EventFeed.Event.Price,
                    Time = feed.EventFeed.Event.Time,
                    Street = feed.EventFeed.Event.Street,
                    PhotoId = feed.EventFeed.Event.PhotoId,
                    IsFacebookEvent = feed.EventFeed.Event.IsFacebookEvent,
                    ImageUrl = feed.EventFeed.Event.IsFacebookEvent
                                   ? new PhotoLinks(feed.EventFeed.Event.FacebookPhotoLink)
                                   : null,
                    IsDateOnly = feed.EventFeed.Event.IsDateOnly,
                    FacebookEventId = feed.EventFeed.Event.FacebookEventId,
                    OwnerName = _userService.GetUserDisplayName(feed.EventFeed.Event.UserId)
                };

                feedVm.Comments.CommentType = CommentType.Event;

                if (feedVm.Event.PhotoId.HasValue)
                {
                    var photo = _uow.PhotoRepository.GetPhotoWithMinimalData(feedVm.Event.PhotoId.Value);
                    feedVm.Event.ImageUrl = _photoService.GeneratePhotoUrl(photo.UserId,
                                                                                    photo.Id);
                }

                if (feedVm.Event.ImageUrl == null)
                {
                    // this event doesn't have a picture
                    feedVm.Event.ImageUrl = _defaultImageHelper.GetDefaultEventImage();
                }

                feedVm.Comments.ItemId = feed.EventFeed.EventId;
                feedVm.Comments.Comments = GetComments(feed.EventFeed.EventId,
                                                                feedVm.Comments.CommentType,
                                                                currentUserId);

            #endregion

                #region Photo
            }
            else if (feed.FeedType == FeedType.Photo)
            {
                var photos = _uow.PhotoRepository.GetPhotos(feed.FeedPhotos.Select(f => f.PhotoId)).ToList();

                if (photos.Count > 0)
                {
                    FillUserDetails(ref feedVm, photos.First().UserId, currentUserId);
                }

                feedVm.Photos = photos
                    .Select(p => new PhotoViewModel
                    {
                        PhotoId = p.Id,
                        AlbumId = p.AlbumId,
                        Description = p.Description,
                        FromUserDisplayName = feedVm.UserDisplayName,
                        FromUserPhotoUrl = feedVm.UserDisplayPhoto,
                        FromUserId = p.UserId,
                        PhotoUrl = _photoService.GeneratePhotoUrl(p.UserId, p.Id)
                    }).ToList();

                feedVm.Comments.CommentType = CommentType.Photo;

                if (photos.Count == 1)
                {
                    var photoId = photos.First().Id;
                    feedVm.Comments.ItemId = photoId;
                    feedVm.Comments.Comments = GetComments(photoId,
                                                                    feedVm.Comments.CommentType,
                                                                    currentUserId);
                }

                #endregion

                #region Post

            }
            else if (feed.FeedType == FeedType.Post)
            {
                FillUserDetails(ref feedVm, feed.PostFeed.Post.FromUserId, currentUserId);

                var post = feed.PostFeed.Post;
                feedVm.Post = new PostViewModel
                {
                    PostId = post.Id,
                    PostText = post.Message
                };

                if (post.ToUserId.HasValue)
                {
                    var toUserId = post.ToUserId.Value;
                    feedVm.Post.ToUserId = toUserId;
                    feedVm.Post.ToUserDisplayName = _userService.GetUserDisplayName(toUserId);
                    feedVm.Post.ToUserDisplayPhoto = _photoService.GetUserDisplayPhoto(toUserId);
                    feedVm.CanCurrentUserRemoveFeed = post.ToUserId == currentUserId;
                }

                if (post.Categories.Any())
                {
                    feedVm.Post.Categories = _staticDataRepository.GetCategories()
                        .Where(c => post.Categories.Any(pc => pc.CategoryId == c.Id))
                        .Select(c => c.Name);
                }

                feedVm.Comments.CommentType = CommentType.Post;

                feedVm.Comments.ItemId = feed.PostFeed.PostId;
                feedVm.Comments.Comments = GetComments(feed.PostFeed.PostId,
                                                                feedVm.Comments.CommentType,
                                                                currentUserId);

                #endregion
            }
            else
            {
                throw new NotImplementedException("Feed type is not implemented");
            }

            return feedVm;
        }

        private void FillUserDetails(ref FeedViewModel feedVm, int userId, int currentUserId)
        {
            feedVm.UserId = userId;
            feedVm.UserDisplayName = _userService.GetUserDisplayName(userId);
            feedVm.UserDisplayPhoto = _photoService.GetUserDisplayPhoto(userId);
            feedVm.IsFromCurrentUser = userId == currentUserId;
        }

        public List<CommentViewModel> GetComments(int id, CommentType commentType, int currentUserId,
            int lastComment = 0, int pageSize = 5)
        {
            var query = _uow.CommentRepository.GetAll();

            if (commentType == CommentType.Event)
            {
                query = query.Where(c => c.EventComment.EventId == id);
            }
            else if (commentType == CommentType.Photo)
            {
                query = query.Where(c => c.PhotoComment.PhotoId == id);
            }
            else if (commentType == CommentType.Post)
            {
                query = query.Where(c => c.PostComment.PostId == id);
            }
            else
            {
                throw new ArgumentException("Invalid comment type", "commentType");
            }

            if (lastComment != 0)
                query = query.Where(c => c.Id < lastComment);


            query = query.OrderByDescending(c => c.Time)
                         .Take(pageSize);

            var comments = query.ToList();

            return comments.Select(c => new CommentViewModel
                                     {
                                         CommentId = c.Id,
                                         CommentText = c.Message,
                                         IsFromCurrentUser = c.UserId == currentUserId,
                                         Time = c.Time,
                                         UserDisplayName = _userService.GetUserDisplayName(c.UserId),
                                         UserId = c.UserId,
                                         UserDisplayPhoto = _photoService.GetUserDisplayPhoto(c.UserId)
                                     }).ToList();
        }
    }
}