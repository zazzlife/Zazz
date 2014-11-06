using System;
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
    public class FollowingController : BaseApiController
    {
        private readonly IObjectMapper _objectMapper;
        private readonly IUserService _userService;
        private readonly IUoW _uow;
        private readonly IPhotoService _photoService;

        public FollowingController(IUserService userService, IObjectMapper objectMapper, IUoW uow, IPhotoService photoService)
        {
            _objectMapper = objectMapper;
            _userService = userService;
            _uow = uow;
            _photoService = photoService;
        }

        // GET api/following/5
        public IEnumerable<ApiUserProfile> Get(int id)
        {
            var user = _userService.GetUser(id, true, true, true, true);
            return UserFollowing(user);   
        }

        private IEnumerable<ApiUserProfile> UserFollowing(User user)
        {
            var follows = _uow.FollowRepository.GetUserFollows(user.Id)
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
        }       

        // POST api/following
        public void Post([FromBody]string value)
        {
        }

        // PUT api/following/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/following/5
        public void Delete(int id)
        {
        }
    }
}
