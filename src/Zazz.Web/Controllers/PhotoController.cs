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
    public class PhotoController : BaseController
    {
        private readonly IPhotoService _photoService;
        private readonly IAlbumService _albumService;
        private readonly IUserService _userService;

        public PhotoController(IPhotoService photoService, IAlbumService albumService, IUserService userService)
        {
            _photoService = photoService;
            _albumService = albumService;
            _userService = userService;
        }

        [Authorize]
        public async Task<ActionResult> Remove(int id)
        {
            using (_photoService)
            using (_albumService)
            using (_userService)
            {
                var userId = _userService.GetUserId(User.Identity.Name);
                await _photoService.RemovePhotoAsync(id, userId);
            }

            return Redirect(HttpContext.Request.UrlReferrer.AbsolutePath);
        }

        [Authorize]
        public async Task<ActionResult> Upload(HttpPostedFileBase image, string description, int albumId)
        {
            var errorMessage = "Image was not valid";
            if (image == null || !ImageValidator.IsValid(image, out errorMessage))
            {
                ShowAlert(errorMessage, AlertType.Error);
                return Redirect(HttpContext.Request.UrlReferrer.AbsolutePath);
            }

            using (_photoService)
            using (_albumService)
            using (_userService)
            {
                var userId = _userService.GetUserId(User.Identity.Name);
                var album = await _albumService.GetAlbumAsync(albumId);

                if (album.UserId != userId)
                    throw new SecurityException();

                var photo = new Photo
                {
                    AlbumId = albumId,
                    Description = description,
                    UploaderId = userId
                };

                await _photoService.SavePhotoAsync(photo, image.InputStream);
            }

            return Redirect(HttpContext.Request.UrlReferrer.AbsolutePath);
        }

    }
}
