using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Web.Helpers;
using Zazz.Web.Models;

namespace Zazz.Web.Controllers
{
    [Authorize]
    public class FollowController : BaseController
    {
        private readonly IFollowService _followService;
        private readonly IUserService _userService;
        private readonly IPhotoService _photoService;

        public FollowController(IFollowService followService, IUserService userService, IPhotoService photoService)
        {
            _followService = followService;
            _userService = userService;
            _photoService = photoService;
        }

        public async void FollowUser(int id)
        {
            using (_followService)
            using (_userService)
            using (_photoService)
            {
                var currentUserId = _userService.GetUserId(User.Identity.Name);
                var accountType = _userService.GetUserAccountType(id);

                if (accountType == AccountType.User)
                {
                    await _followService.SendFollowRequestAsync(currentUserId, id);
                }
                else if (accountType == AccountType.ClubAdmin)
                {
                    await _followService.FollowClubAdminAsync(currentUserId, id);
                }
            }
        }

        public async void AcceptFollow(int id)
        {
            using (_followService)
            using (_userService)
            using (_photoService)
            {
                var currentUserId = _userService.GetUserId(User.Identity.Name);
                await _followService.AcceptFollowRequestAsync(id, currentUserId);
            }
        }

        public async void RejectFollow(int id)
        {
            using (_followService)
            using (_userService)
            using (_photoService)
            {
                var currentUserId = _userService.GetUserId(User.Identity.Name);
                await _followService.RejectFollowRequestAsync(id, currentUserId);
            }
        }

        public async Task<ActionResult> GetFollowRequests()
        {
            using (_followService)
            using (_userService)
            using (_photoService)
            {
                var userId = _userService.GetUserId(User.Identity.Name);
                var userFollows = await _followService.GetFollowRequestsAsync(userId);

                var vm = new List<FollowRequestViewModel>();

                foreach (var followRequest in userFollows)
                {
                    var r = new FollowRequestViewModel
                            {
                                FromUserId = followRequest.FromUserId,
                                FromUsername = followRequest.FromUser.Username,
                                RequestId = followRequest.Id
                            };

                    var imgId = followRequest.FromUser.UserDetail.ProfilePhotoId;
                    if (imgId == 0)
                    { 
                        // the user has not uploaded an image
                        r.FromUserPictureUrl = DefaultImageHelper
                            .GetUserDefaultImage(followRequest.FromUser.UserDetail.Gender);
                    }
                    else
                    {
                        var photo = await _photoService.GetPhotoAsync(imgId);
                        if (photo == null)
                        {
                            // the image might have been deleted just a few miliseconds ago
                            r.FromUserPictureUrl = DefaultImageHelper
                                .GetUserDefaultImage(followRequest.FromUser.UserDetail.Gender);
                        }
                        else
                        {
                            var photoUrl = _photoService.GeneratePhotoUrl(photo.UploaderId, photo.AlbumId, photo.Id);
                            r.FromUserPictureUrl = photoUrl;
                        }
                    }

                    vm.Add(r);
                }

                return View("_FollowRequestsPartial", vm);
            }
        }

        public string GetFollowRequestsCount()
        {
            using (_followService)
            using (_userService)
            using (_photoService)
            {
                var userId = _userService.GetUserId(User.Identity.Name);
                var requestsCount = _followService.GetFollowRequestsCountAsync(userId).Result;

                if (requestsCount == 0)
                    return "";
                else
                {
                    return
                        "<span id=\"follow-request-count\" class=\"badge badge-small badge-info\">"
                        + requestsCount + "</span>";
                }
            }
        }
    }
}
