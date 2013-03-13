using System;
using System.Collections.Generic;
using System.IO;
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
    public class UploadController : BaseController
    {
        private readonly IPhotoService _photoService;
        private readonly IAlbumService _albumService;
        private readonly IUserService _userService;

        public UploadController(IPhotoService photoService, IAlbumService albumService, IUserService userService)
        {
            _photoService = photoService;
            _albumService = albumService;
            _userService = userService;
        }

        public ActionResult Index()
        {
            throw new HttpException(404, "Not Found");
        }

        [Authorize]
        public async Task<ActionResult> Image(HttpPostedFileBase image, string description, int albumId)
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

        public JsonResult ImageAjax(HttpPostedFileBase image)
        {
            var result = new UploadFileModel
                             {
                                 name = image.FileName,
                                 size = image.ContentLength,
                                 type = image.ContentType
                             };

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public class UploadFileModel
        {
            public string name { get; set; }

            public string type { get; set; }

            public int size { get; set; }
        }
    }
}
