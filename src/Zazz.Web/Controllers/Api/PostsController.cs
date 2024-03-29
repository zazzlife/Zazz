﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Web.Http;
using Zazz.Core.Exceptions;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Services;
using Zazz.Core.Models.Data;
using Zazz.Web.Filters;
using Zazz.Web.Interfaces;
using Zazz.Web.Models.Api;

namespace Zazz.Web.Controllers.Api
{
    [OAuth2Authorize]
    public class PostsController : BaseApiController
    {
        private readonly IPostService _postService;
        private readonly IObjectMapper _objectMapper;

        public PostsController(IPostService postService, IObjectMapper objectMapper)
        {
            _postService = postService;
            _objectMapper = objectMapper;
        }

        // GET api/v1/posts/5
        public ApiPost Get(int id)
        {
            if (id == 0)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            var post = _postService.GetPost(id);
            if (post == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            return _objectMapper.PostToApiPost(post);
        }

        // POST api/v1/posts
        public HttpResponseMessage Post([FromBody] ApiPost post)
        {
            if (post.Message == null || post.Message.Count() != 1)
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            var p = new Post
                    {
                        CreatedTime = DateTime.UtcNow,
                        FromUserId = CurrentUserId,
                        Message = post.Message.First().Text,
                        ToUserId = post.ToUserId,
                        TagUsers = post.TagUsers,
                        Lockusers = post.Lockusers
                    };

            _postService.NewPost(p, post.Categories);
            post.PostId = p.Id;
            post.Time = p.CreatedTime;
            post.FromUserId = p.FromUserId;

            var response = Request.CreateResponse(HttpStatusCode.Created, post);
            return response;
        }

        // PUT api/v1/posts/5
        public void Put(int id, [FromBody] ApiPost post)
        {
            if (id == 0)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            if (post.Message == null || post.Message.Count() != 1)
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            try
            {
                _postService.EditPost(id, post.Message.First().Text, post.Categories, CurrentUserId);
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

        // DELETE api/v1/posts/5
        public void Delete(int id)
        {
            if (id == 0)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            try
            {
                _postService.DeletePost(id, CurrentUserId);
            }
            catch (SecurityException)
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            }
        }
    }
}
