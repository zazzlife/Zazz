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
using System.Text.RegularExpressions;

namespace Zazz.Web.Helpers
{
    public class FeedHelper : IFeedHelper
    {
        private readonly IUoW _uow;
        private readonly IUserService _userService;
        private readonly IPhotoService _photoService;
        private readonly IDefaultImageHelper _defaultImageHelper;
        private readonly IStaticDataRepository _staticDataRepository;
        private Regex _tagRegex;
        public int PageSize { get; set; }

        public FeedHelper(IUoW uow, IUserService userService, IPhotoService photoService,
            IDefaultImageHelper defaultImageHelper, IStaticDataRepository staticDataRepository)
        {
            _uow = uow;
            _userService = userService;
            _photoService = photoService;
            _defaultImageHelper = defaultImageHelper;
            _staticDataRepository = staticDataRepository;
            _tagRegex = new Regex(@"(?:[^\w]|^)@(\w+)", RegexOptions.IgnoreCase);
            PageSize = 10;
        }

        /// <summary>
        /// Retruns all recent feeds that contain the provided categories.
        /// </summary>
        /// <param name="currentUserId">Current User Id</param>
        /// <param name="tagIds">List of tag ids to filter feeds.</param>
        /// <param name="lastFeedId">id of the last feed. if 0 it loads the most recent feeds else it loads the most recent feeds prior to the provided feed id</param>
        /// <returns></returns>
        public FeedsViewModel GetCategoryFeeds(int currentUserId, List<byte> catIds, int lastFeedId = 0, List<int> tags = null)
        {
            var followIds = _uow.FollowRepository.GetFollowsUserIds(currentUserId).ToList();
            followIds.Add(currentUserId);

            IQueryable<Feed> query;

            if (tags != null)
            {
                if (catIds.Count > 0)
                {
                    query = _uow.FeedRepository.GetFeedsWithCategoriesTags(followIds, catIds, tags);
                }
                else
                {
                    query = _uow.FeedRepository.GetFeedsWithTags(followIds, tags);
                }
            }
            else
            {
                query = _uow.FeedRepository.GetFeedsWithCategories(followIds, catIds);
            }

            var remainingQuery = query;

            if (lastFeedId > 0)
            {
                var maxDate = _uow.FeedRepository.GetFeedDateTime(lastFeedId);
                query = query.Where(f => f.Time < maxDate);
            }
            else if(lastFeedId == 0)
            {
                var minDate = DateTime.UtcNow.AddDays(-7);
                query = query.Where(f => f.Time >= minDate);
            }

            query = query.Take(PageSize);

            var feeds = query.ToList();

            var remaining = 0;
            if (feeds.Count > 0)
            {
                var maxDate = feeds.Last().Time;
                remaining = remainingQuery.Where(f => f.Time < maxDate).Count();
            }
            else if (lastFeedId == 0)
            {
                remaining = remainingQuery.Count();
            }

            return ConvertFeedsToFeedsViewModel(feeds, currentUserId, remaining);
        }

        /// <summary>
        /// Returns a list of activities of people that the user follows and the user himself
        /// </summary>
        /// <param name="currentUserId"></param>
        /// <param name="lastFeedId">id of the last feed. if 0 it loads the most recent feeds else it loads the most recent feeds prior to the provided feed id</param>
        /// <returns></returns>
        public FeedsViewModel GetFeeds(int currentUserId, int lastFeedId = 0)
        {
            var followIds = _uow.FollowRepository.GetFollowsUserIds(currentUserId).ToList();
            followIds.Add(currentUserId);

            var query = _uow.FeedRepository.GetFeeds(followIds);
            var remainingQuery = query;

            if (lastFeedId > 0)
            {
                var maxDate = _uow.FeedRepository.GetFeedDateTime(lastFeedId);
                query = query.Where(f => f.Time < maxDate);
            }
            else if(lastFeedId == 0)
            {
                var minDate = DateTime.UtcNow.AddDays(-7);
                query = query.Where(f => f.Time >= minDate);
            }
            
            query = query.Take(PageSize);

            var feeds = query.ToList();

            var remaining = 0;
            if (feeds.Count > 0)
            {
                var maxDate = feeds.Last().Time;
                remaining = remainingQuery.Where(f => f.Time < maxDate).Count();
            }
            else if (lastFeedId == 0)
            {
                remaining = remainingQuery.Count();
            }

            return ConvertFeedsToFeedsViewModel(feeds, currentUserId, remaining);
        }

        /// <summary>
        /// Returns a list of user activities
        /// </summary>
        /// <param name="userId">Id of the target user</param>
        /// <param name="currentUserId">Id of the current user</param>
        /// <param name="lastFeedId">id of the last feed. if 0 it loads the most recent feeds else it loads the most recent feeds prior to the provided feed id</param>
        /// <returns></returns>
        public FeedsViewModel GetUserActivityFeed(int userId, int currentUserId,
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

        public FeedsViewModel GetUserLikedFeed(int userId, int currentUserId, int lastFeedId = 0)
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

        private FeedsViewModel ConvertFeedsToFeedsViewModel(IEnumerable<Feed> feeds, int currentUserId, int remaining = 0)
        {
            var vm = new FeedsViewModel
            {
                feeds = new List<FeedViewModel>()
            };

            foreach (var feed in feeds)
            {
                vm.feeds.Add(ConvertFeedToFeedViewModel(feed, currentUserId));
            }

            vm.remaining = remaining;
            return vm;
        }
        
        public IEnumerable<PostMsgItemViewModel> GetPostMsgItems(string message)
        {
            Match m = _tagRegex.Match(message);
            int prev = 0;
            List<PostMsgItemViewModel> items = new List<PostMsgItemViewModel>();

            while (m.Success)
            {
                Capture c = m.Groups[1].Captures[0];
                string s = c.ToString();
                int end = c.Index + s.Length;

                if(end == message.Length || message[end] != '@')
                {
                    items.Add(new PostMsgItemViewModel {
                        ClubId = -1,
                        Text = message.Substring(prev, c.Index - 1 - prev)
                    });
                    items.Add(new PostMsgItemViewModel {
                        ClubId = _userService.GetUserId(s),
                        Text = '@' + s
                    });

                    prev = end;
                }

                m = m.NextMatch();
            }

            items.Add(new PostMsgItemViewModel {
                ClubId = -1,
                Text = message.Substring(prev)
            });
            
            return items;
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
                    OwnerName = _userService.GetUserDisplayName(feed.EventFeed.Event.UserId),
                    ProfileImage = _photoService.GetUserDisplayPhoto(feed.EventFeed.Event.UserId),
                    CoverImage = feed.EventFeed.Event.User.AccountType == AccountType.Club && feed.EventFeed.Event.User.ClubDetail.CoverPhotoId.HasValue
                        ? _photoService.GeneratePhotoUrl(feed.EventFeed.Event.UserId, feed.EventFeed.Event.User.ClubDetail.CoverPhotoId.Value)
                        : null
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
                    Message = GetPostMsgItems(post.Message)
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