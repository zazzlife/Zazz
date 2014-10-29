using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Services;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;
using Zazz.Web.Filters;
using Zazz.Web.Interfaces;
using Zazz.Web.Models.Api;

namespace Zazz.Web.Controllers.Api
{
    [OAuth2Authorize]
    public class PostLikeController : BaseApiController
    {

        private readonly IObjectMapper _objectMapper;
        private readonly ILikeService _likeService;
        private readonly IUserService _userService;

        public PostLikeController(IUserService userService, ILikeService likeService,
            IObjectMapper objectMapper)
        {
            _likeService = likeService;
            _objectMapper = objectMapper;
            _userService = userService;
        }


        // GET /api/v1/like/posts/5
        public void Get(int id)
        {
            _likeService.AddPhotoLike(id, CurrentUserId);
        }

        // PPOST /api/v1/like/posts/5
        public void Post(int id)
        {
            
        }

        // PUT api/postlike/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE /api/v1/like/posts/5
        public void Delete(int id)
        {
            
        }
    }
}
