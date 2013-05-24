using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;
using Zazz.Infrastructure;
using Zazz.Web.Helpers;
using Zazz.Web.Models;

namespace Zazz.Web.Controllers
{
    public class CommentController : Controller
    {
        private readonly IUoW _uow;
        private readonly IPhotoService _photoService;
        private readonly IUserService _userService;
        private readonly ICommentService _commentService;
        private readonly IDefaultImageHelper _defaultImageHelper;

        public CommentController(IUoW uow, IPhotoService photoService, IUserService userService,
            ICommentService commentService, IDefaultImageHelper defaultImageHelper)
        {
            _uow = uow;
            _photoService = photoService;
            _userService = userService;
            _commentService = commentService;
            _defaultImageHelper = defaultImageHelper;
        }

        public ActionResult Get(int id, CommentType commentType, int lastComment)
        {
            if (id == 0)
                throw new ArgumentException("Id cannot be 0", "id");

            if (lastComment == 0)
                throw new ArgumentException("Last Comment cannot be 0", "lastComment");

            var userId = 0;
            if (User.Identity.IsAuthenticated)
                userId = _userService.GetUserId(User.Identity.Name);

            var feedHelper = new FeedHelper(_uow, _userService, _photoService, _defaultImageHelper);
            var comments = feedHelper.GetComments(id, commentType, userId, lastComment, 10);

            return View("FeedItems/_CommentList", comments);
        }

        [Authorize]
        public ActionResult LightboxComments(int id)
        {
            if (id == 0)
                throw new ArgumentException("Id cannot be 0", "id");

            var currentUserId = _userService.GetUserId(User.Identity.Name);
            var feedHelper = new FeedHelper(_uow, _userService, _photoService, _defaultImageHelper);

            var vm = new CommentsViewModel
                     {
                         CommentType = CommentType.Photo,
                         ItemId = id,
                         CurrentUserPhotoUrl = _photoService.GetUserImageUrl(currentUserId),
                         Comments = feedHelper.GetComments(id, CommentType.Photo, currentUserId)
                     };

            return View("FeedItems/_FeedComments", vm);
        }

        [Authorize, HttpPost]
        public ActionResult New(int id, CommentType commentType, string comment)
        {
            if (String.IsNullOrEmpty(comment))
                throw new ArgumentNullException("comment");

            if (id == 0)
                throw new ArgumentException("Id cannot be 0", "id");

            var userId = _userService.GetUserId(User.Identity.Name);
            if (userId == 0)
                throw new SecurityException();

            var c = new Comment
            {
                UserId = userId,
                Message = comment,
                Time = DateTime.UtcNow
            };

            if (commentType == CommentType.Event)
            {
                c.EventComment = new EventComment { EventId = id };
            }
            else if (commentType == CommentType.Photo)
            {
                c.PhotoComment = new PhotoComment { PhotoId = id };
            }
            else if (commentType == CommentType.Post)
            {
                c.PostComment = new PostComment { PostId = id };
            }
            else
            {
                throw new ArgumentException("Invalid feed type", "commentType");
            }

            var commentId = _commentService.CreateComment(c, commentType);
            var commentVm = new CommentViewModel
                            {
                                CommentId = commentId,
                                CommentText = c.Message,
                                IsFromCurrentUser = userId == c.UserId,
                                Time = c.Time,
                                UserId = c.UserId,
                                UserDisplayName = _userService.GetUserDisplayName(userId),
                                UserPhotoUrl = _photoService.GetUserImageUrl(userId)
                            };

            return View("FeedItems/_SingleComment", commentVm);
        }

        [Authorize]
        public void Remove(int id)
        {
            if (id == 0)
                throw new ArgumentException("Id cannot be 0", "id");

            var userId = _userService.GetUserId(User.Identity.Name);
            _commentService.RemoveComment(id, userId);
        }

        [Authorize, HttpPost]
        public void Edit(int id, string comment)
        {
            if (id == 0)
                throw new ArgumentException("Id cannot be 0", "id");

            var userId = _userService.GetUserId(User.Identity.Name);
            _commentService.EditComment(id, userId, comment);
        }
    }
}
