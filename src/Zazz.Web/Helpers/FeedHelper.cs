using System;
using System.Collections.Generic;
using System.Linq;
using Zazz.Core.Interfaces;
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
        public int PageSize { get; set; }

        public FeedHelper(IUoW uow, IUserService userService, IPhotoService photoService,
            IDefaultImageHelper defaultImageHelper)
        {
            _uow = uow;
            _userService = userService;
            _photoService = photoService;
            _defaultImageHelper = defaultImageHelper;
            PageSize = 10;
        }

        /// <summary>
        /// Retruns all recent feeds that contain the provided tags.
        /// </summary>
        /// <param name="currentUserId">Current User Id</param>
        /// <param name="tagIds">List of tag ids to filter feeds.</param>
        /// <param name="lastFeedId">id of the last feed. if 0 it loads the most recent feeds else it loads the most recent feeds prior to the provided feed id</param>
        /// <returns></returns>
        public List<FeedViewModel> GetTaggedFeeds(int currentUserId, List<byte> tagIds, int lastFeedId = 0)
        {
            var query = _uow.FeedRepository.GetFeedsWithTags(tagIds);

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
            var currentUserPhotoUrl = _photoService.GetUserImageUrl(currentUserId);
            var feedVm = new FeedViewModel
            {
                FeedId = feed.Id,
                FeedType = feed.FeedType,
                Time = feed.Time,
                CommentsViewModel = new CommentsViewModel
                {
                    CurrentUserPhotoUrl = currentUserPhotoUrl,
                }
            };

            #region Event
            if (feed.FeedType == FeedType.Event && feed.EventFeed != null)
            {
                // EVENT

                FillUserDetails(ref feedVm, feed.EventFeed.Event.UserId, currentUserId);

                feedVm.EventViewModel = new EventViewModel
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
                    FacebookEventId = feed.EventFeed.Event.FacebookEventId
                };

                feedVm.CommentsViewModel.CommentType = CommentType.Event;

                if (feedVm.EventViewModel.PhotoId.HasValue)
                {
                    var photo = _uow.PhotoRepository.GetPhotoWithMinimalData(feedVm.EventViewModel.PhotoId.Value);
                    feedVm.EventViewModel.ImageUrl = _photoService.GeneratePhotoUrl(photo.UserId,
                                                                                    photo.Id);
                }

                if (feedVm.EventViewModel.ImageUrl == null)
                {
                    // this event doesn't have a picture
                    feedVm.EventViewModel.ImageUrl = _defaultImageHelper.GetDefaultEventImage();
                }

                feedVm.CommentsViewModel.ItemId = feed.EventFeed.EventId;
                feedVm.CommentsViewModel.Comments = GetComments(feed.EventFeed.EventId,
                                                                feedVm.CommentsViewModel.CommentType,
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

                feedVm.PhotoViewModel = photos
                    .Select(p => new PhotoViewModel
                    {
                        PhotoId = p.Id,
                        PhotoDescription = p.Description,
                        FromUserDisplayName = feedVm.UserDisplayName,
                        FromUserPhotoUrl = feedVm.UserImageUrl,
                        FromUserId = p.UserId,
                        PhotoUrl = p.IsFacebookPhoto
                                       ? new PhotoLinks(p.FacebookLink)
                                       : _photoService.GeneratePhotoUrl(p.UserId, p.Id)
                    }).ToList();

                feedVm.CommentsViewModel.CommentType = CommentType.Photo;

                if (photos.Count == 1)
                {
                    var photoId = photos.First().Id;
                    feedVm.CommentsViewModel.ItemId = photoId;
                    feedVm.CommentsViewModel.Comments = GetComments(photoId,
                                                                    feedVm.CommentsViewModel.CommentType,
                                                                    currentUserId);
                }

                #endregion

                #region Post

            }
            else if (feed.FeedType == FeedType.Post)
            {
                FillUserDetails(ref feedVm, feed.PostFeed.Post.FromUserId, currentUserId);

                var post = feed.PostFeed.Post;
                feedVm.PostViewModel = new PostViewModel
                {
                    PostId = post.Id,
                    PostText = post.Message
                };

                if (post.ToUserId.HasValue)
                {
                    var toUserId = post.ToUserId.Value;
                    feedVm.PostViewModel.ToUserId = toUserId;
                    feedVm.PostViewModel.ToUserDisplayName = _userService.GetUserDisplayName(toUserId);
                    feedVm.PostViewModel.ToUserPhotoUrl = _photoService.GetUserImageUrl(toUserId);
                    feedVm.CurrentUserCanRemoveFeed = post.ToUserId == currentUserId;
                }

                feedVm.CommentsViewModel.CommentType = CommentType.Post;

                feedVm.CommentsViewModel.ItemId = feed.PostFeed.PostId;
                feedVm.CommentsViewModel.Comments = GetComments(feed.PostFeed.PostId,
                                                                feedVm.CommentsViewModel.CommentType,
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
            feedVm.UserImageUrl = _photoService.GetUserImageUrl(userId);
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
                                         UserPhotoUrl = _photoService.GetUserImageUrl(c.UserId)
                                     }).ToList();
        }

        public ApiFeed FeedViewModelToApiModel(FeedViewModel feed)
        {
            return new ApiFeed
            {
                FeedId = feed.FeedId,
                FeedType = feed.FeedType,
                UserId = feed.UserId,
                UserDisplayName = feed.UserDisplayName,
                UserDisplayPhoto = feed.UserImageUrl,
                CanCurrentUserRemoveFeed = feed.CurrentUserCanRemoveFeed || feed.IsFromCurrentUser,
                Time = feed.Time,

                Comments = feed.CommentsViewModel.Comments == null
                ? null //Enumerable.Empty<ApiComment>()
                : feed.CommentsViewModel.Comments.Select(c => new ApiComment
                {
                    CommentId = c.CommentId,
                    CommentText = c.CommentText,
                    IsFromCurrentUser = c.IsFromCurrentUser,
                    UserId = c.UserId,
                    UserDisplayName = c.UserDisplayName,
                    UserDisplayPhoto = c.UserPhotoUrl,
                    Time = c.Time
                }),

                Photos = feed.FeedType == FeedType.Photo
                ? feed.PhotoViewModel.Select(p => new ApiPhoto
                {
                    PhotoId = p.PhotoId,
                    Description = p.PhotoDescription,
                    UserId = p.FromUserId,
                    UserDisplayName = p.FromUserDisplayName,
                    UserDisplayPhoto = p.FromUserPhotoUrl,
                    PhotoLinks = p.PhotoUrl
                })
                : null,

                Post = feed.FeedType == FeedType.Post
                ? new ApiPost
                {
                    Message = feed.PostViewModel.PostText,
                    PostId = feed.PostViewModel.PostId,
                    FromUserId = feed.UserId,
                    FromUserDisplayName = feed.UserDisplayName,
                    FromUserDisplayPhoto = feed.UserImageUrl,
                    ToUserDisplayName = feed.PostViewModel.ToUserDisplayName,
                    ToUserId = feed.PostViewModel.ToUserId,
                    ToUserDisplayPhoto = feed.PostViewModel.ToUserPhotoUrl,
                    Time = feed.Time
                }
                : null,

                ApiEvent = feed.FeedType == FeedType.Event
                ? new ApiEvent
                {
                    City = feed.EventViewModel.City,
                    CreatedTime = feed.EventViewModel.CreatedDate.HasValue
                    ? feed.EventViewModel.CreatedDate.Value
                    : DateTime.MinValue,
                    Description = feed.EventViewModel.Description,
                    EventId = feed.EventViewModel.Id,
                    FacebookLink = feed.EventViewModel.FacebookEventId.HasValue
                        ? "https://www.facebook.com/events/" + feed.EventViewModel.FacebookEventId.Value
                        : null,
                    ImageUrl = feed.EventViewModel.ImageUrl,
                    IsDateOnly = feed.EventViewModel.IsDateOnly,
                    IsFacebookEvent = feed.EventViewModel.IsFacebookEvent,
                    IsFromCurrentUser = feed.EventViewModel.IsOwner,
                    Latitude = feed.EventViewModel.Latitude,
                    Longitude = feed.EventViewModel.Longitude,
                    Location = feed.EventViewModel.Location,
                    Name = feed.EventViewModel.Name,
                    Price = feed.EventViewModel.Price,
                    Street = feed.EventViewModel.Street,
                    Time = feed.EventViewModel.Time,
                    UtcTime = feed.EventViewModel.Time.ToUniversalTime().DateTime,
                    UserDisplayName = feed.UserDisplayName,
                    UserDisplayPhoto = feed.UserImageUrl,
                    UserId = feed.UserId
                }
                : null
            };
        }

        public ApiComment CommentViewModelToApiModel(CommentViewModel commentViewModel)
        {
            return new ApiComment
                   {
                       CommentId = commentViewModel.CommentId,
                       CommentText = commentViewModel.CommentText,
                       IsFromCurrentUser = commentViewModel.IsFromCurrentUser,
                       Time = commentViewModel.Time,
                       UserDisplayName = commentViewModel.UserDisplayName,
                       UserDisplayPhoto = commentViewModel.UserPhotoUrl,
                       UserId = commentViewModel.UserId
                   };
        }
    }
}