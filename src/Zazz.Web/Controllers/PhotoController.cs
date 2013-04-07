using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Infrastructure;
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
        public ActionResult Index(int? id)
        {
            using (_photoService)
            using (_albumService)
            using (_userService)
            {
                var userId = _userService.GetUserId(User.Identity.Name);
                return RedirectToAction("List", new {id = userId, page = 1});
            }
        }

        public ActionResult List(int id, int page = 1)
        {
            using (_photoService)
            using (_albumService)
            using (_userService)
            {
                const int PAGE_SIZE = 30;
                var skip = (page - 1) * PAGE_SIZE;

                var currentUserId = 0;
                if (Request.IsAuthenticated)
                    currentUserId = _userService.GetUserId(User.Identity.Name);

                var photos = _photoService.GetAll()
                    .OrderBy(p => p.Id)
                    .Skip(skip)
                    .Take(PAGE_SIZE)
                    .Select(p => new
                                 {
                                     id = p.Id,
                                     userId = p.UploaderId,
                                     isFromFb = p.IsFacebookPhoto,
                                     fbUrl = p.FacebookLink
                                 })
                    .ToList();

                var photosVm = photos.Select(p => new PhotoViewModel
                                            {
                                                IsFromCurrentUser = p.userId == currentUserId,
                                                PhotoId = p.id,
                                                PhotoUrl = p.isFromFb
                                                               ? p.fbUrl
                                                               : _photoService.GeneratePhotoUrl(p.userId, p.id)
                                            })
                               .ToList();

                if (Request.IsAjaxRequest())
                {
                    return View("_PhotoList", photosVm);
                }


                var vm = new MainPhotoPageViewModel
                         {
                             IsForOwner = currentUserId == id,
                             Photos = photosVm,
                             UserId = id,
                             ViewType = PhotoViewType.Photos
                         };

                return View("MainView", vm);
            }
        }

        public async Task<ActionResult> Albums(int id)
        {
            using (_photoService)
            using (_albumService)
            using (_userService)
            {
                var albums = await _albumService.GetUserAlbumsAsync(id);

                var currentUserId = 0;
                if (User.Identity.IsAuthenticated)
                    currentUserId = _userService.GetUserId(User.Identity.Name);

                var albumsVm = new List<AlbumViewModel>();
                foreach (var a in albums)
                {
                    var album = new AlbumViewModel
                                {
                                    AlbumId = a.Id,
                                    AlbumName = a.Name,
                                    IsFromCurrentUser = currentUserId == id,
                                    UserId = a.UserId
                                };

                    var photo = a.Photos.FirstOrDefault();
                    if (photo == null)
                        album.AlbumPicUrl = DefaultImageHelper.GetDefaultAlbumImage();
                    else
                    {
                        album.AlbumPicUrl = photo.IsFacebookPhoto
                                                ? photo.FacebookLink
                                                : _photoService.GeneratePhotoUrl(photo.UploaderId, photo.Id);
                    }

                    albumsVm.Add(album);
                }

                if (Request.IsAjaxRequest())
                {
                    return View("_AlbumList", albumsVm);
                }

                var fullVm = new MainPhotoPageViewModel
                             {
                                 IsForOwner = currentUserId == id,
                                 Albums = albumsVm,
                                 UserId = id,
                                 ViewType = PhotoViewType.Albums
                             };

                return View("MainView", fullVm);
            }
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
        public async Task<ActionResult> Upload(HttpPostedFileBase image, string description, int? albumId)
        {
            using (_photoService)
            using (_albumService)
            using (_userService)
            {
                var errorMessage = "Image was not valid";
                if (image == null || !ImageValidator.IsValid(image, out errorMessage))
                {
                    ShowAlert(errorMessage, AlertType.Error);
                    return Redirect(HttpContext.Request.UrlReferrer.AbsolutePath);
                }

                await SaveImageAsync(image.InputStream, description, albumId);
            }

            return Redirect(HttpContext.Request.UrlReferrer.AbsolutePath);
        }

        public async Task<JsonNetResult> AjaxUpload(string description, int? albumId, HttpPostedFileBase image)
        {
            using (_photoService)
            using (_albumService)
            using (_userService)
            {
                var response = new FineUploadResponse
                               {
                               };
                var errorMessage = "Image was not valid";
                if (image == null || !ImageValidator.IsValid(image, out errorMessage))
                {
                    response.Success = false;
                    response.Error = errorMessage;
                    return new JsonNetResult(response);
                }

                var photo = await SaveImageAsync(image.InputStream, description, albumId);
                response.PhotoId = photo.Id;
                response.Success = true;
                response.PhotoUrl = _photoService.GeneratePhotoUrl(photo.UploaderId, photo.Id);

                return new JsonNetResult(response);
            }
        }

        private async Task<Photo> SaveImageAsync(Stream image, string description, int? albumId)
        {

            var userId = _userService.GetUserId(User.Identity.Name);

            if (albumId.HasValue)
            {
                var album = await _albumService.GetAlbumAsync(albumId.Value);

                if (album.UserId != userId)
                    throw new SecurityException();
            }

            var photo = new Photo
                        {
                            AlbumId = albumId,
                            Description = description,
                            UploaderId = userId
                        };

            await _photoService.SavePhotoAsync(photo, image, true);
            return photo;
        }

        [Authorize]
        public async Task<ActionResult> Feed(int id)
        {
            using (_photoService)
            using (_albumService)
            using (_userService)
            {
                var userId = _userService.GetUserId(User.Identity.Name);
                var userDisplayName = _userService.GetUserDisplayName(userId);
                var userPhoto = _photoService.GetUserImageUrl(userId);
                var photo = await _photoService.GetPhotoAsync(id);

                var vm = new FeedViewModel
                         {
                             UserId = userId,
                             UserImageUrl = userPhoto,
                             UserDisplayName = userDisplayName,
                             Time = photo.UploadDate,
                             FeedType = FeedType.Picture,
                             IsFromCurrentUser = true,
                             PhotoViewModel = new PhotoViewModel
                                              {
                                                  PhotoId = photo.Id,
                                                  PhotoUrl = _photoService.GeneratePhotoUrl(userId, photo.Id),
                                                  PhotoDescription = photo.Description
                                              },
                             CommentsViewModel = new CommentsViewModel
                                                 {
                                                     Comments = new List<CommentViewModel>(),
                                                     FeedType = FeedType.Picture,
                                                     CurrentUserPhotoUrl = userPhoto,
                                                     ItemId = photo.Id
                                                 }
                         };

                return View("FeedItems/_PicturePostFeedItem", vm);
            }
        }

        [Authorize, HttpGet]
        public async Task<ActionResult> Crop(int id, string @for)
        {
            using (_photoService)
            using (_albumService)
            using (_userService)
            {
                var vm = new CropViewModel();
                var photo = await _photoService.GetPhotoAsync(id);

                if (photo.IsFacebookPhoto)
                {
                    vm.IsFacebookPhoto = true;
                    return View(vm);
                }

                var userId = _userService.GetUserId(User.Identity.Name);
                if (photo.UploaderId != userId)
                    throw new HttpException(401, "You are not authorized to crop this image.");

                vm.PhotoUrl = _photoService.GeneratePhotoUrl(userId, photo.Id);
                vm.Ratio = @for.Equals("cover", StringComparison.InvariantCultureIgnoreCase)
                               ? 10 / 3 : 1;

                return View(vm);
            }
        }

        [Authorize, HttpPost]
        public async Task<ActionResult> Crop(int id, string @for, CropViewModel vm)
        {
            using (_photoService)
            using (_albumService)
            using (_userService)
            {
                var cropArea = new Rectangle((int)vm.X, (int)vm.Y, (int)vm.W, (int)vm.H);
                var photo = await _photoService.GetPhotoAsync(id);

                if (photo.IsFacebookPhoto)
                {
                    vm.IsFacebookPhoto = true;
                    return View(vm);
                }

                var userId = _userService.GetUserId(User.Identity.Name);
                if (photo.UploaderId != userId)
                    throw new HttpException(401, "You are not authorized to crop this image.");

                vm.PhotoUrl = _photoService.GeneratePhotoUrl(userId, photo.Id);
                vm.Ratio = @for.Equals("cover", StringComparison.InvariantCultureIgnoreCase)
                               ? 10 / 3 : 1;

                _photoService.CropPhoto(photo, userId, cropArea);
                return View(vm);
            }
        }
    }
}
