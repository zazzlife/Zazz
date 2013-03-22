using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
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

        [Authorize]
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

                var userId = _uow.UserRepository.GetIdByUsername(User.Identity.Name);
                if (userId == 0)
                    throw new SecurityException();

                var c = new Comment
                        {
                            FromId = userId,
                            Message = comment,
                            Time = DateTime.UtcNow
                        };

                if (feedType == FeedType.Post || feedType == FeedType.Event)
                {
                    c.PostId = id;
                }
                else if (feedType == FeedType.Picture)
                {
                    c.PhotoId = id;
                }
                else
                {
                    throw new ArgumentException("Invalid feed type", "feedType");
                }

                _uow.CommentRepository.InsertGraph(c);
                await _uow.SaveAsync();

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
    }
}
