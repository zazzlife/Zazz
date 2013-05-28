using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Zazz.Core.Interfaces;
using Zazz.Web.Filters;
using Zazz.Web.Models.Api;

namespace Zazz.Web.Controllers.Api
{
    [HMACAuthorize]
    public class PostController : ApiController
    {
        private readonly IUserService _userService;
        private readonly IPhotoService _photoService;
        private readonly IPostService _postService;

        public PostController(IUserService userService, IPhotoService photoService, IPostService postService)
        {
            _userService = userService;
            _photoService = photoService;
            _postService = postService;
        }

        // GET api/v1/post/5
        public ApiPost Get(int id)
        {
            if (id == 0)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            var post = _postService.GetPost(id);
            if (post == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            return new ApiPost
                   {
                       PostId = post.Id,
                       FromUserId = post.FromUserId,
                       FromUserDisplayName = _userService.GetUserDisplayName(post.FromUserId),
                       FromUserDisplayPhoto = _photoService.GetUserImageUrl(post.FromUserId),
                       Message = post.Message,
                       Time = post.CreatedTime,
                       ToUserId = post.ToUserId,
                       ToUserDisplayName = post.ToUserId.HasValue
                                               ? _userService.GetUserDisplayName(post.ToUserId.Value)
                                               : null,
                       ToUserDisplayPhoto = post.ToUserId.HasValue
                                                ? _photoService.GetUserImageUrl(post.ToUserId.Value)
                                                : null,
                   };
        }

        // POST api/v1/post
        public void Post(ApiPost post)
        {
        }

        // PUT api/v1/post/5
        public void Put(int id, ApiPost value)
        {
        }

        // DELETE api/v1/post/5
        public void Delete(int id)
        {
        }
    }
}
