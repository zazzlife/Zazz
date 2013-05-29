using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Web.Http;
using Zazz.Core.Exceptions;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Web.Filters;
using Zazz.Web.Interfaces;
using Zazz.Web.Models.Api;

namespace Zazz.Web.Controllers.Api
{
    [HMACAuthorize]
    public class PostsController : BaseApiController
    {
        private readonly IUserService _userService;
        private readonly IPhotoService _photoService;
        private readonly IPostService _postService;
        private readonly IObjectMapper _objectMapper;

        public PostsController(IUserService userService, IPhotoService photoService, IPostService postService,
            IObjectMapper objectMapper)
        {
            _userService = userService;
            _photoService = photoService;
            _postService = postService;
            _objectMapper = objectMapper;
        }

        // GET api/v1/post/5
        public ApiPost Get(int id)
        {
            if (id == 0)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            var post = _postService.GetPost(id);
            if (post == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            return _objectMapper.PostToApiPost(post);
        }

        // POST api/v1/post
        public ApiPost Post([FromBody] ApiPost post)
        {
            if (String.IsNullOrWhiteSpace(post.Message))
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            var p = new Post
                    {
                        CreatedTime = DateTime.UtcNow,
                        FromUserId = ExtractUserIdFromHeader(),
                        Message = post.Message,
                        ToUserId = post.ToUserId
                    };

            _postService.NewPost(p);
            post.PostId = p.Id;

            return post;
        }

        // PUT api/v1/post/5
        public void Put(int id, [FromBody] ApiPost post)
        {
            if (id == 0)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            if (String.IsNullOrWhiteSpace(post.Message))
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            try
            {
                _postService.EditPost(id, post.Message, ExtractUserIdFromHeader());
            }
            catch (NotFoundException)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            catch (SecurityException)
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            }
        }

        // DELETE api/v1/post/5
        public void Delete(int id)
        {
            if (id == 0)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            try
            {
                _postService.RemovePost(id, ExtractUserIdFromHeader());
            }
            catch (SecurityException)
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            }
        }
    }
}
