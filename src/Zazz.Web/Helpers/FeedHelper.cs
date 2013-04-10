using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
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
        /// <param name="page"></param>
        /// <returns></returns>
        public async Task<List<FeedViewModel>> GetFeedsAsync(int currentUserId, int page = 0)
        {
            var skip = PageSize * page;

            var followIds = _uow.FollowRepository.GetFollowsUserIds(currentUserId).ToList();
            followIds.Add(currentUserId);

            var feeds = _uow.FeedRepository.GetFeeds(followIds)
                            .OrderByDescending(f => f.Time)
                            .Skip(skip)
                            .Take(PageSize)
                            .ToList();

            return await ConvertFeedsToFeedsViewModelAsync(feeds, currentUserId);
        }

        /// <summary>
        /// Returns a list of user activities
        /// </summary>
        /// <param name="userId">Id of the target user</param>
        /// <param name="currentUserId">Id of the current user</param>
        /// <param name="page"></param>
        /// <returns></returns>
        public async Task<List<FeedViewModel>> GetUserActivityFeedAsync(int userId, int currentUserId, int page = 0)
        {
            var skip = PageSize * page;
            var feeds = _uow.FeedRepository.GetUserFeeds(userId)
                            .OrderByDescending(f => f.Time)
                            .Skip(skip)
                            .Take(PageSize)
                            .ToList();

            return await ConvertFeedsToFeedsViewModelAsync(feeds, currentUserId);
        }

        private async Task<List<FeedViewModel>> ConvertFeedsToFeedsViewModelAsync(IEnumerable<Feed> feeds, int currentUserId)
        {
            var vm = new List<FeedViewModel>();

            var currentUserPhotoUrl = _photoService.GetUserImageUrl(currentUserId);

            foreach (var feed in feeds)
            {
                var feedVm = new FeedViewModel
                             {
                                 UserId = feed.UserId,
                                 UserDisplayName = _userService.GetUserDisplayName(feed.UserId),
                                 UserImageUrl = _photoService.GetUserImageUrl(feed.UserId),
                                 IsFromCurrentUser = currentUserId == feed.UserId,
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
                                                IsOwner = feed.UserId == currentUserId,
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
                                                               ? feed.Event.FacebookPhotoLink
                                                               : null,
                                                IsDateOnly = feed.Event.IsDateOnly,
                                                FacebookEventId = feed.Event.FacebookEventId
                                            };

                    feedVm.CommentsViewModel.CommentType = CommentType.Event;

                    if (feedVm.EventViewModel.PhotoId.HasValue)
                    {
                        var photo = _uow.PhotoRepository.GetPhotoWithMinimalData(feedVm.EventViewModel.PhotoId.Value);
                        feedVm.EventViewModel.ImageUrl =
                            _photoService.GeneratePhotoUrl(photo.UploaderId,
                                                           photo.Id);
                    }

                    feedVm.CommentsViewModel.ItemId = feed.EventId.Value;
                    feedVm.CommentsViewModel.Comments = GetComments(feed.EventId.Value,
                                                                    feedVm.CommentsViewModel.CommentType,
                                                                    currentUserId);
                }
                else if (feed.FeedType == FeedType.Picture)
                {
                    //var photo = await _uow.PhotoRepository.GetByIdAsync(feed.PhotoId.Value);
                    //feedVm.PhotoViewModel = new PhotoViewModel
                    //                        {
                    //                            PhotoId = photo.Id,
                    //                            PhotoDescription = photo.Description,
                    //                            FromUserDisplayName = feedVm.UserDisplayName,
                    //                            FromUserPhotoUrl = feedVm.UserImageUrl,
                    //                            FromUserId = feedVm.UserId
                    //                        };

                    //feedVm.CommentsViewModel.CommentType = CommentType.Photo;

                    //feedVm.PhotoViewModel.PhotoUrl = photo.IsFacebookPhoto
                    //    ? photo.FacebookLink
                    //    : _photoService.GeneratePhotoUrl(photo.UploaderId, photo.Id);

                    //feedVm.CommentsViewModel.ItemId = feed.PhotoId.Value;
                    //feedVm.CommentsViewModel.Comments = GetComments(feed.PhotoId.Value,
                    //                                                feedVm.CommentsViewModel.CommentType,
                    //                                                currentUserId);
                }
                else if (feed.FeedType == FeedType.Post)
                {
                    var post = await _uow.PostRepository.GetByIdAsync(feed.PostId.Value);
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