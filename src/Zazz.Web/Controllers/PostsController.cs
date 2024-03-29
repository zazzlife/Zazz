﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Repositories;
using Zazz.Core.Interfaces.Services;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;
using Zazz.Web.Helpers;
using Zazz.Web.Interfaces;
using Zazz.Web.Models;
using StructureMap.Pipeline;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Zazz.Web.Controllers
{
    public class PostsController : BaseController
    {
        private readonly IPostService _postService;
        private readonly IFeedHelper _feedHelper;
        private readonly ICategoryService _categoryService;
        private readonly ICategoryStatsCache _categoryStatsCache;

        public PostsController(IPostService postService, IUserService userService,
                               IPhotoService photoService, IDefaultImageHelper defaultImageHelper,
                               IFeedHelper feedHelper, ICategoryService categoryService,
                               IStaticDataRepository staticDataRepository,
                               ICategoryStatsCache categoryStatsCache)
            : base(userService, photoService, defaultImageHelper, staticDataRepository, categoryService)
        {
            _postService = postService;
            _feedHelper = feedHelper;
            _categoryService = categoryService;
            _categoryStatsCache = categoryStatsCache;
        }

        [Authorize]
        public ActionResult Show(int id)
        {
            var currentUserId = UserService.GetUserId(User.Identity.Name);
            var feed = _feedHelper.GetSinglePostFeed(id, currentUserId);

            if (feed == null)
                throw new HttpException(404, "Post not found.");

            var vm = new ShowPostViewModel
                     {
                         FeedViewModel = feed
                     };

            return View(vm);
        }

        [Authorize, HttpPost]
        public ActionResult UpdateCategories(int id, IEnumerable<int> categories)
        {
            var post = _postService.GetPost(id);
            var userId = UserService.GetUserId(User.Identity.Name);

            _postService.EditPost(id, post.Message, categories, userId);

            _categoryService.UpdateStatistics();
            _categoryStatsCache.LastUpdate = DateTime.UtcNow.AddMinutes(-6);
            return Redirect(HttpContext.Request.UrlReferrer.AbsolutePath);
        }

        [Authorize,HttpPost]
        public ActionResult New(string message, int? toUser, IEnumerable<int> categories, string metaData)
        {
            string tagUsers = "";
            string lockuser = "";
            try
            {
                var result = new JsonResult
                {
                    Data = JsonConvert.DeserializeObject(metaData)
                };

                JObject jsonData = (JObject)result.Data;
                JArray tagUser = (JArray)jsonData["taguser"];
                JArray lockUser = (JArray)jsonData["lockuser"];
                tagUsers = tagUser.ToString().Replace("[", "").Replace("]","").Trim();
                lockuser = lockUser.ToString().Replace("[", "").Replace("]", "").Trim();
            }
            catch (Exception et)
            {
            }

            message = message.Trim().Replace("&nbsp;","");

            if (String.IsNullOrEmpty(message))
                throw new ArgumentNullException("message");

            var userId = UserService.GetUserId(User.Identity.Name);
            var post = new Post
                       {
                           CreatedTime = DateTime.UtcNow,
                           Message = message,
                           FromUserId = userId,
                           ToUserId = toUser,
                           TagUsers = tagUsers,
                           Lockusers = lockuser
                       };

            _postService.NewPost(post, categories);
            
            var userPhotoUrl = PhotoService.GetUserDisplayPhoto(userId);

            var vm = new FeedViewModel
                     {
                         IsFromCurrentUser = true,
                         FeedType = FeedType.Post,
                         UserId = userId,
                         Time = post.CreatedTime,
                         UserDisplayName = UserService.GetUserDisplayName(userId),
                         UserDisplayPhoto = userPhotoUrl,
                         Post = new PostViewModel
                                         {
                                             PostId = post.Id,
                                             Message = _feedHelper.GetPostMsgItems(message),
                                             ToUserId = toUser,
                                             ToUserDisplayName = toUser.HasValue
                                                                     ? UserService.GetUserDisplayName(toUser.Value)
                                                                     : null,
                                             ToUserDisplayPhoto = toUser.HasValue
                                                                  ? PhotoService.GetUserDisplayPhoto(toUser.Value)
                                                                  : null,
                                            Categories = categories != null 
                                                ? StaticDataRepository.GetCategories()
                                                        .Where(c => categories.Any(ca => ca == c.Id))
                                                        .Select(c => c.Name)
                                                : Enumerable.Empty<string>()
                                            
                                         },
                         Comments = new CommentsViewModel
                                             {
                                                 Comments = new List<CommentViewModel>(),
                                                 CurrentUserDisplayPhoto = userPhotoUrl,
                                                 CommentType = CommentType.Post,
                                                 ItemId = post.Id
                                             }
                     };

            _categoryService.UpdateStatistics();
            _categoryStatsCache.LastUpdate = DateTime.UtcNow.AddMinutes(-6);
            return View("FeedItems/_PostFeedItem", vm);
        }

        [Authorize]
        public void Remove(int id)
        {
            var userId = UserService.GetUserId(User.Identity.Name);
            _postService.DeletePost(id, userId);
        }

        [Authorize, HttpPost]
        public void Edit(int id, string text)
        {
            var userId = UserService.GetUserId(User.Identity.Name);
            _postService.EditPost(id, text, null, userId);
        }
    }
}
