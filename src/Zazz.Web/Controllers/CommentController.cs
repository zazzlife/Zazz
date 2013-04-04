using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Web.Helpers;
using Zazz.Web.Models;

namespace Zazz.Web.Controllers
{
    public class CommentController : Controller
    {
        private readonly IUoW _uow;
        private readonly IPhotoService _photoService;
        private readonly IUserService _userService;

        public CommentController(IUoW uow, IPhotoService photoService, IUserService userService)
        {
            _uow = uow;
            _photoService = photoService;
            _userService = userService;
        }

        public async Task<ActionResult> Get(int id, FeedType feedType, int lastComment)
        {
            using (_uow)
            using (_photoService)
            using (_userService)
            {
                if (id == 0)
                    throw new ArgumentException("Id cannot be 0", "id");

                if (lastComment == 0)
                    throw new ArgumentException("Last Comment cannot be 0", "lastComment");

                var userId = 0;
                if (User.Identity.IsAuthenticated)
                    userId = _userService.GetUserId(User.Identity.Name);

                var feedHelper = new FeedHelper(_uow, _userService, _photoService);
                var comments = feedHelper.GetComments(id, feedType, userId, lastComment, 10);

                return View("FeedItems/_CommentList", comments);
            }
        }
            
        [Authorize, HttpPost]
        public async Task<ActionResult> New(int id, FeedType feedType, string comment)
        {
            using (_uow)
            using (_photoService)
            using (_userService)
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
                            FromId = userId,
                            Message = comment,
                            Time = DateTime.UtcNow
                        };

                if (feedType == FeedType.Event)
                {
                    c.EventId = id;
                }
                else if (feedType == FeedType.Picture)
                {
                    c.PhotoId = id;
                }
                else if (feedType == FeedType.Post)
                {
                    c.PostId = id;
                }
                else
                {
                    throw new ArgumentException("Invalid feed type", "feedType");
                }

                _uow.CommentRepository.InsertGraph(c);
                _uow.SaveChanges();

                var commentVm = new CommentViewModel
                                {
                                    CommentId = c.Id,
                                    CommentText = c.Message,
                                    IsFromCurrentUser = userId == c.FromId,
                                    Time = c.Time,
                                    UserId = c.FromId,
                                    UserDisplayName = _userService.GetUserDisplayName(userId),
                                    UserPhotoUrl = _photoService.GetUserImageUrl(userId)
                                };

                return View("FeedItems/_SingleComment", commentVm);
            }
        }

        [Authorize]
        public async Task Remove(int id)
        {
            using (_uow)
            using (_photoService)
            using (_userService)
            {
                if (id == 0)
                    throw new ArgumentException("Id cannot be 0", "id");

                var userId = _userService.GetUserId(User.Identity.Name);
                var comment = await _uow.CommentRepository.GetByIdAsync(id);
                if (comment == null)
                    return;

                if (comment.FromId != userId)
                    throw new SecurityException();

                _uow.CommentRepository.Remove(comment);

                _uow.SaveChanges();
            }
        }

        [Authorize, HttpPost]
        public async Task Edit(int id, string comment)
        {
            using (_uow)
            using (_photoService)
            using (_userService)
            {
                if (id == 0)
                    throw new ArgumentException("Id cannot be 0", "id");

                var userId = _userService.GetUserId(User.Identity.Name);
                var c = await _uow.CommentRepository.GetByIdAsync(id);
                if (c == null)
                    return;

                if (c.FromId != userId)
                    throw new SecurityException();

                c.Message = comment;

                _uow.SaveChanges();
            }
        }
    }
}
