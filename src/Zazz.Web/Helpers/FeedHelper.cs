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
        /// Returns a list of activities of people that the user follows
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public async Task<List<FeedViewModel>> GetFeedsAsync(int userId, int page = 0)
        {
            var skip = PageSize * page;

            var followIds = _uow.FollowRepository.GetFollowsUserIds(userId).ToList();
            followIds.Add(userId);

            var feeds = _uow.FeedRepository.GetFeeds(followIds)
                            .OrderByDescending(f => f.Time)
                            .Skip(skip)
                            .Take(PageSize)
                            .ToList();

            return await ConvertFeedsToFeedsViewModelAsync(feeds);
        }

        /// <summary>
        /// Returns a list of user activities
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public async Task<List<FeedViewModel>> GetUserActivityFeedAsync(int userId, int page = 0)
        {
            var skip = PageSize * page;
            var feeds = _uow.FeedRepository.GetUserFeeds(userId)
                            .OrderByDescending(f => f.Time)
                            .Skip(skip)
                            .Take(PageSize)
                            .ToList();

            return await ConvertFeedsToFeedsViewModelAsync(feeds);
        }

        private async Task<List<FeedViewModel>> ConvertFeedsToFeedsViewModelAsync(IEnumerable<Feed> feeds)
        {
            var vm = new List<FeedViewModel>();

            foreach (var feed in feeds)
            {
                var feedVm = new FeedViewModel
                             {
                                 UserId = feed.UserId,
                                 UserDisplayName = _userService.GetUserDisplayName(feed.UserId),
                                 UserImageUrl = _photoService.GetUserImageUrl(feed.UserId),
                                 FeedType = feed.FeedType,
                                 Time = feed.Time,
                             };

                if (feed.FeedType == FeedType.Event)
                {
                    // EVENT
                    feedVm.EventViewModel = new EventViewModel
                                            {
                                                City = feed.Post.EventDetail.City,
                                                CreatedDate = feed.Post.CreatedDate,
                                                Detail = feed.Post.Message,
                                                FacebookLink = feed.Post.FacebookLink,
                                                Id = feed.Post.Id,
                                                IsOwner = false,
                                                Latitude = feed.Post.EventDetail.Latitude,
                                                Location = feed.Post.EventDetail.Location,
                                                Longitude = feed.Post.EventDetail.Longitude,
                                                Name = feed.Post.Title,
                                                Price = feed.Post.EventDetail.Price,
                                                Time = feed.Post.EventDetail.Time,
                                                Street = feed.Post.EventDetail.Street,
                                                PhotoId = feed.Post.PhotoId
                                            };

                    if (feedVm.EventViewModel.PhotoId.HasValue)
                    {
                        var photo = _uow.PhotoRepository.GetPhotoWithMinimalData(feedVm.EventViewModel.PhotoId.Value);
                        feedVm.EventViewModel.ImageUrl =
                            _photoService.GeneratePhotoUrl(photo.UploaderId,
                                                           photo.Id);
                    }

                }
                else if (feed.FeedType == FeedType.Picture)
                {
                    var photo = await _uow.PhotoRepository.GetByIdAsync(feed.PhotoId.Value);
                    feedVm.PhotoViewModel = new PhotoViewModel
                                            {
                                                PhotoUrl = _photoService.GeneratePhotoUrl(photo.UploaderId, photo.Id),
                                                PhotoId = photo.Id,
                                                PhotoDescription = photo.Description
                                            };
                }

                vm.Add(feedVm);
            }

            return vm;
        }
    }
}