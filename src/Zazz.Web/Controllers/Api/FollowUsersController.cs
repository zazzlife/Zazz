using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.IO;
using System.Security;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Zazz.Core.Exceptions;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Services;
using Zazz.Core.Models.Data;
using Zazz.Web.Filters;
using Zazz.Web.Interfaces;
using Zazz.Web.Models.Api;
using Zazz.Core.Interfaces.Repositories;
using Zazz.Core.Models;
using Zazz.Core.Models.Data.Enums;

namespace Zazz.Web.Controllers.Api
{
    [OAuth2Authorize]
    public class FollowUsersController : BaseApiController
    {
        private readonly IObjectMapper _objectMapper;
        private readonly IUserService _userService;
        private readonly IUoW _uow;
        private readonly IPhotoService _photoService;
        private readonly IFollowService _followService;

        public FollowUsersController(IUserService userService, IObjectMapper objectMapper, IUoW uow, IPhotoService photoService, IFollowService followService)
        {
            _objectMapper = objectMapper;
            _userService = userService;
            _uow = uow;
            _photoService = photoService;
            _followService = followService;
        }

        public IEnumerable<ApiUserProfile> Get(int id)
        {
            var user = _userService.GetUser(id, true, true, true, true);
            return UserFollowers(user);
        }

        private IEnumerable<ApiUserProfile> UserFollowers(User user)
        {
            var follows = _uow.FollowRepository.GetUserFollowers(user.Id)
                .Select(f => new
                {
                    id = f.ToUserId,
                    f.ToUser.ProfilePhotoId
                }).ToList();
            return follows.Select(f => new ApiUserProfile
            {
                Id = f.id,
                DisplayName = _userService.GetUserDisplayName(f.id),
                DisplayPhoto = _photoService.GetUserDisplayPhoto(f.id)
            });

            /*
            var requestIds = _followService
                    .GetFollowRequests(user.Id)
                    .Select(f => f.FromUserId).ToList();

            return requestIds.Select(id => new ApiUserProfile
            {
                Id = id,
                DisplayName = _userService.GetUserDisplayName(id),
                DisplayPhoto = _photoService.GetUserDisplayPhoto(id)
            });
             * */
           
        }

        // POST api/followusers
        public void Post([FromBody]string value)
        {
        }

        // PUT api/followusers/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/followusers/5
        public void Delete(int id)
        {
        }
    }
}
