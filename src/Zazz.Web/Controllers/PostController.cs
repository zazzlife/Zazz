using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;
using Zazz.Web.Helpers;
using Zazz.Web.Models;

namespace Zazz.Web.Controllers
{
    public class PostController : Controller
    {
        private readonly IPostService _postService;
        private readonly IUserService _userService;
        private readonly IPhotoService _photoService;
        private readonly IUoW _uow;
        private readonly IDefaultImageHelper _defaultImageHelper;

        public PostController(IPostService postService, IUserService userService,
            IPhotoService photoService, IUoW uow, IDefaultImageHelper defaultImageHelper)
        {
            _postService = postService;
            _userService = userService;
            _photoService = photoService;
            _uow = uow;
            _defaultImageHelper = defaultImageHelper;
        }

        [Authorize]
        public ActionResult Show(int id)
        {
            var currentUserId = _userService.GetUserId(User.Identity.Name);
            var feed = new FeedHelper(_uow, _userService, _photoService, _defaultImageHelper)
                .GetSinglePostFeed(id, currentUserId);

            if (feed == null)
                throw new HttpException(404, "Post not found.");

            var vm = new ShowPostViewModel
                     {
                         CurrentUserDisplayName = _userService.GetUserDisplayName(currentUserId),
                         CurrentUserPhoto = _photoService.GetUserImageUrl(currentUserId),
                         FeedViewModel = feed
                     };

            return View(vm);
        }

        [Authorize, HttpPost]
        public ActionResult New(string message, int? toUser)
        {
            if (String.IsNullOrEmpty(message))
                throw new ArgumentNullException("message");

            var userId = _userService.GetUserId(User.Identity.Name);
            var post = new Post
                       {
                           CreatedTime = DateTime.UtcNow,
                           Message = message,
                           FromUserId = userId,
                           ToUserId = toUser
                       };

            _postService.NewPost(post);

            var userPhotoUrl = _photoService.GetUserImageUrl(userId);

            var vm = new FeedViewModel
                     {
                         IsFromCurrentUser = true,
                         FeedType = FeedType.Post,
                         UserId = userId,
                         Time = post.CreatedTime,
                         UserDisplayName = _userService.GetUserDisplayName(userId),
                         UserImageUrl = userPhotoUrl,
                         PostViewModel = new PostViewModel
                                         {
                                             PostId = post.Id,
                                             PostText = message,
                                             ToUserId = toUser,
                                             ToUserDisplayName = toUser.HasValue
                                                                     ? _userService.GetUserDisplayName(toUser.Value)
                                                                     : null,
                                             ToUserPhotoUrl = toUser.HasValue
                                                                  ? _photoService.GetUserImageUrl(toUser.Value)
                                                                  : null
                                         },
                         CommentsViewModel = new CommentsViewModel
                                             {
                                                 Comments = new List<CommentViewModel>(),
                                                 CurrentUserPhotoUrl = userPhotoUrl,
                                                 CommentType = CommentType.Post,
                                                 ItemId = post.Id
                                             }
                     };

            return View("FeedItems/_PostFeedItem", vm);
        }

        [Authorize]
        public void Remove(int id)
        {
            var userId = _userService.GetUserId(User.Identity.Name);
            _postService.RemovePost(id, userId);
        }

        [Authorize, HttpPost]
        public void Edit(int id, string text)
        {
            var userId = _userService.GetUserId(User.Identity.Name);
            _postService.EditPost(id, text, userId);
        }
    }
}
