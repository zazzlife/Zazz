using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Repositories;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;
using Zazz.Web.Helpers;
using Zazz.Web.Interfaces;
using Zazz.Web.Models;

namespace Zazz.Web.Controllers
{
    public class PostsController : UserPageLayoutBaseController
    {
        private readonly IPostService _postService;
        private readonly IFeedHelper _feedHelper;

        public PostsController(IPostService postService, IUserService userService,
                               IPhotoService photoService, IDefaultImageHelper defaultImageHelper,
                               IFeedHelper feedHelper, ICategoryService categoryService,
                               IStaticDataRepository staticDataRepository)
            : base(userService, photoService, defaultImageHelper, categoryService, staticDataRepository)
        {
            _postService = postService;
            _feedHelper = feedHelper;
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
        public ActionResult New(string message, int? toUser, IEnumerable<int> categories)
        {
            if (String.IsNullOrEmpty(message))
                throw new ArgumentNullException("message");

            var userId = UserService.GetUserId(User.Identity.Name);
            var post = new Post
                       {
                           CreatedTime = DateTime.UtcNow,
                           Message = message,
                           FromUserId = userId,
                           ToUserId = toUser
                       };

            _postService.NewPost(post, categories);

            var userPhotoUrl = PhotoService.GetUserImageUrl(userId);

            var vm = new FeedViewModel
                     {
                         IsFromCurrentUser = true,
                         FeedType = FeedType.Post,
                         UserId = userId,
                         Time = post.CreatedTime,
                         UserDisplayName = UserService.GetUserDisplayName(userId),
                         UserImageUrl = userPhotoUrl,
                         PostViewModel = new PostViewModel
                                         {
                                             PostId = post.Id,
                                             PostText = message,
                                             ToUserId = toUser,
                                             ToUserDisplayName = toUser.HasValue
                                                                     ? UserService.GetUserDisplayName(toUser.Value)
                                                                     : null,
                                             ToUserPhotoUrl = toUser.HasValue
                                                                  ? PhotoService.GetUserImageUrl(toUser.Value)
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
            var userId = UserService.GetUserId(User.Identity.Name);
            _postService.RemovePost(id, userId);
        }

        [Authorize, HttpPost]
        public void Edit(int id, string text)
        {
            var userId = UserService.GetUserId(User.Identity.Name);
            _postService.EditPost(id, text, null, userId);
        }
    }
}
