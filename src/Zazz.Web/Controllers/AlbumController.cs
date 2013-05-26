using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using PagedList;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Infrastructure;
using Zazz.Infrastructure.Helpers;
using Zazz.Web.Models;

namespace Zazz.Web.Controllers
{
    public class AlbumController : BaseController
    {
        private readonly IAlbumService _albumService;

        public AlbumController(IAlbumService albumService, IUserService userService,
            IPhotoService photoService, IDefaultImageHelper defaultImageHelper)
            : base(userService, photoService, defaultImageHelper)
        {
            _albumService = albumService;
        }

        [Authorize, HttpPost]
        public ActionResult CreateAlbum(string value)
        {
            if (value.Length > 50)
            {
                ShowAlert("Album name cannot be longer than 50 characters", AlertType.Error);
                throw new HttpException(400, "Bad Request");
            }
            else
            {
                var userId = UserService.GetUserId(User.Identity.Name);
                var album = new Album
                {
                    Name = value,
                    UserId = userId
                };

                _albumService.CreateAlbum(album);

                var vm = new AlbumViewModel
                         {
                             AlbumId = album.Id,
                             AlbumName = value,
                             AlbumPicUrl = DefaultImageHelper.GetDefaultAlbumImage(),
                             IsFromCurrentUser = true,
                             UserId = userId
                         };

                return View("_SingleAlbum", vm);
            }
        }

        [Authorize]
        public void Remove(int id)
        {
            var userId = UserService.GetUserId(User.Identity.Name);
            _albumService.DeleteAlbum(id, userId);
        }

        [HttpGet]
        public JsonResult GetAlbums()
        {
            if (!User.Identity.IsAuthenticated)
                throw new HttpException(403, "");

            var userId = UserService.GetUserId(User.Identity.Name);
            var albums = _albumService.GetUserAlbums(userId);

            var response = albums.Select(a => new { id = a.Id, name = a.Name });

            return Json(response, JsonRequestBehavior.AllowGet);
        }
    }
}
