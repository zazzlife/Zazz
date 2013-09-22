using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Services;

namespace Zazz.Web.Controllers
{
    [Authorize]
    public class LikeController : Controller
    {
        private readonly ILikeService _likeService;
        private readonly IUserService _userService;

        public LikeController(ILikeService likeService, IUserService userService)
        {
            _likeService = likeService;
            _userService = userService;
        }

        public void Add(int id)
        {
            var userId = _userService.GetUserId(User.Identity.Name);
            _likeService.AddPhotoLike(id, userId);
        }

        public void Remove(int id)
        {
            var userId = _userService.GetUserId(User.Identity.Name);
            _likeService.RemovePhotoLike(id, userId);
        }

        public bool Exists(int id)
        {
            var userId = _userService.GetUserId(User.Identity.Name);
            return _likeService.PhotoLikeExists(id, userId);
        }

        public int Count(int id)
        {
            return _likeService.GetPhotoLikesCount(id);
        }
    }
}
