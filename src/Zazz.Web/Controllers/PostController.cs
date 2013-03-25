using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Web.Models;

namespace Zazz.Web.Controllers
{
    public class PostController : Controller
    {
        private readonly IPostService _postService;
        private readonly IUserService _userService;
        private readonly IPhotoService _photoService;

        public PostController(IPostService postService, IUserService userService, IPhotoService photoService)
        {
            _postService = postService;
            _userService = userService;
            _photoService = photoService;
        }

        [Authorize, HttpPost]
        public async Task<ActionResult> New(string message)
        {
            using (_userService)
            using (_photoService)
            using (_postService)
            {
                if (String.IsNullOrEmpty(message))
                    throw new ArgumentNullException("message");

                var userId = _userService.GetUserId(User.Identity.Name);
                var post = new Post
                           {
                               CreatedTime = DateTime.UtcNow,
                               Message = message,
                               UserId = userId
                           };

                await _postService.NewPostAsync(post);

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
                                                 PostText = message
                                             },
                             CommentsViewModel = new CommentsViewModel
                                                 {
                                                     Comments = new List<CommentViewModel>(),
                                                     CurrentUserPhotoUrl = userPhotoUrl,
                                                     FeedType = FeedType.Post,
                                                     ItemId = post.Id
                                                 }
                         };

                return View("FeedItems/_PostFeedItem", vm);
            }
        }

        [Authorize]
        public async Task Remove(int id)
        {
            using (_userService)
            using (_photoService)
            using (_postService)
            {
                var userId = _userService.GetUserId(User.Identity.Name);
                await _postService.RemovePostAsync(id, userId);
            }
        }

        [Authorize, HttpPost]
        public async Task Edit(int id, string text)
        {
            using (_userService)
            using (_photoService)
            using (_postService)
            {
                var userId = _userService.GetUserId(User.Identity.Name);
                await _postService.EditPostAsync(id, text, userId);
            }
        }
    }
}
