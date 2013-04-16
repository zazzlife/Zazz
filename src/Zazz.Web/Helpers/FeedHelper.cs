using System;
using System.Collections.Generic;
using System.Linq;
using Zazz.Core.Interfaces;
using Zazz.Core.Models;
using Zazz.Core.Models.Data;
using Zazz.Infrastructure;
using Zazz.Web.Models;

namespace Zazz.Web.Helpers
{
    public class FeedHelper
    {
        private readonly IUoW _uow;
        private readonly IUserService _userService;
        private readonly IPhotoService _photoService;
        public int PageSize { get; set; }

        /// <summary>
        /// IMPORTANT: This class does not dispose any of the injected resources.
        /// </summary>
        /// <param name="uow"></param>
        /// <param name="userService"></param>
        /// <param name="photoService"></param>
        public FeedHelper(IUoW uow, IUserService userService, IPhotoService photoService)
        {
            _uow = uow;
            _userService = userService;
            _photoService = photoService;
            PageSize = 10;
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

        private List<FeedViewModel> ConvertFeedsToFeedsViewModel(IEnumerable<Feed> feeds, int currentUserId)
        {
            var vm = new List<FeedViewModel>();

            var currentUserPhotoUrl = _photoService.GetUserImageUrl(currentUserId);
            throw new NotImplementedException();
            //Fix commented lines

            foreach (var feed in feeds)
            {
                var feedVm = new FeedViewModel
                             {
                                 FeedId = feed.Id,
                                 //UserId = feed.UserId,
                                 //UserDisplayName = _userService.GetUserDisplayName(feed.UserId),
                                 //UserImageUrl = _photoService.GetUserImageUrl(feed.UserId),
                                 //IsFromCurrentUser = currentUserId == feed.UserId,
                                 FeedType = feed.FeedType,
                                 Time = feed.Time,
                                 CommentsViewModel = new CommentsViewModel
                                                     {
                                                         CurrentUserPhotoUrl = currentUserPhotoUrl,
                                                     }
                             };

                if (feed.FeedType == FeedType.Event)
                {
                    // EVENT
                    feedVm.EventViewModel = new EventViewModel
                                            {
                                                City = feed.Event.City,
                                                CreatedDate = feed.Event.CreatedDate,
                                                Description = feed.Event.Description,
                                                Id = feed.Event.Id,
                                                //IsOwner = feed.UserId == currentUserId,
                                                Latitude = feed.Event.Latitude,
                                                Location = feed.Event.Location,
                                                Longitude = feed.Event.Longitude,
                                                Name = feed.Event.Name,
                                                Price = feed.Event.Price,
                                                Time = feed.Event.Time,
                                                Street = feed.Event.Street,
                                                PhotoId = feed.Event.PhotoId,
                                                IsFacebookEvent = feed.Event.IsFacebookEvent,
                                                ImageUrl = feed.Event.IsFacebookEvent
                                                               ? new PhotoLinks(feed.Event.FacebookPhotoLink)
                                                               : null,
                                                IsDateOnly = feed.Event.IsDateOnly,
                                                FacebookEventId = feed.Event.FacebookEventId
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
                        feedVm.EventViewModel.ImageUrl = DefaultImageHelper.GetDefaultEventImage();
                    }

                    feedVm.CommentsViewModel.ItemId = feed.EventId.Value;
                    feedVm.CommentsViewModel.Comments = GetComments(feed.EventId.Value,
                                                                    feedVm.CommentsViewModel.CommentType,
                                                                    currentUserId);
                }
                else if (feed.FeedType == FeedType.Picture)
                {
                    var photos = _uow.PhotoRepository.GetPhotos(feed.FeedPhotoIds.Select(f => f.PhotoId)).ToList();
                    feedVm.PhotoViewModel = photos
                        .Select(p => new PhotoViewModel
                                     {
                                         PhotoId = p.Id,
                                         PhotoDescription = p.Description,
                                         FromUserDisplayName = feedVm.UserDisplayName,
                                         FromUserPhotoUrl = feedVm.UserImageUrl,
                                         FromUserId = feedVm.UserId,
                                         PhotoUrl = p.IsFacebookPhoto
                                                        ? new PhotoLinks( p.FacebookLink)
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
                }
                else if (feed.FeedType == FeedType.Post)
                {
                    var post = _uow.PostRepository.GetById(feed.PostId.Value);
                    feedVm.PostViewModel = new PostViewModel
                                           {
                                               PostId = post.Id,
                                               PostText = post.Message
                                           };

                    feedVm.CommentsViewModel.CommentType = CommentType.Post;

                    feedVm.CommentsViewModel.ItemId = feed.PostId.Value;
                    feedVm.CommentsViewModel.Comments = GetComments(feed.PostId.Value,
                                                                    feedVm.CommentsViewModel.CommentType,
                                                                    currentUserId);
                }
                else
                {
                    throw new NotImplementedException("Feed type is not implemented");
                }

                vm.Add(feedVm);
            }

            return vm;
        }

        public List<CommentViewModel> GetComments(int id, CommentType commentType, int currentUserId,
            int lastComment = 0, int pageSize = 5)
        {
            var query = _uow.CommentRepository.GetAll();

            if (commentType == CommentType.Event)
            {
                query = query.Where(c => c.EventId == id);
            }
            else if (commentType == CommentType.Photo)
            {
                query = query.Where(c => c.PhotoId == id);
            }
            else if (commentType == CommentType.Post)
            {
                query = query.Where(c => c.PostId == id);
            }
            else
            {
                throw new ArgumentException("Invalid feed type", "commentType");
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
                                         IsFromCurrentUser = c.FromId == currentUserId,
                                         Time = c.Time,
                                         UserDisplayName = _userService.GetUserDisplayName(c.FromId),
                                         UserId = c.FromId,
                                         UserPhotoUrl = _photoService.GetUserImageUrl(c.FromId)
                                     }).ToList();
        }
    }
}