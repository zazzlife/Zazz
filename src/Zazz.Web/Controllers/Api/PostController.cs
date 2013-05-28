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
            throw new NotImplementedException();
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
