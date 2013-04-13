using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using PagedList;
using Zazz.Core.Interfaces;
using Zazz.Core.Models;
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
        public ActionResult Index()
        {
            using (_photoService)
            using (_albumService)
            using (_userService)
            {
                var userId = _userService.GetUserId(User.Identity.Name);
                return RedirectToAction("List", new { id = userId, page = 1 });
            }
        }

        public ActionResult List(int id, int? albumId, int page = 1)
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

                var query = _photoService.GetAll().Where(p => p.UserId == id);
                if (albumId.HasValue)
                    query = query.Where(p => p.AlbumId == albumId.Value);

                var photos = query
                    .OrderBy(p => p.Id)
                    .Skip(skip)
                    .Take(PAGE_SIZE)
                    .Select(p => new
                                 {
                                     id = p.Id,
                                     userId = p.UserId,
                                     isFromFb = p.IsFacebookPhoto,
                                     fbUrl = p.FacebookLink,
                                     description = p.Description
                                 })
                    .ToList();

                var photosVm = photos.Select(p => new PhotoViewModel
                                                  {
                                                      IsFromCurrentUser = p.userId == currentUserId,
                                                      PhotoId = p.id,
                                                      PhotoUrl = p.isFromFb
                                                                     ? new PhotoLinks(p.fbUrl)
                                                                     : _photoService.GeneratePhotoUrl(p.userId, p.id),
                                                      FromUserId = id,
                                                      FromUserDisplayName = _userService.GetUserDisplayName(id),
                                                      PhotoDescription = p.description,
                                                      FromUserPhotoUrl = _photoService.GetUserImageUrl(id)
                                                  })
                                     .ToList();

                if (Request.IsAjaxRequest())
                {
                    return View("_PhotoList", photosVm);
                }


                var vm = new MainPhotoPageViewModel
                         {
                             IsForCurrentUser = currentUserId == id,
                             Photos = photosVm,
                             UserId = id,
                             ViewType = PhotoViewType.Photos,
                             UserDisplayName = _userService.GetUserDisplayName(id)
                         };

                return View("MainView", vm);
            }
        }

        public ActionResult Albums(int id)
        {
            using (_photoService)
            using (_albumService)
            using (_userService)
            {
                var albums = _albumService.GetUserAlbums(id);

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

                    var photo = a.Photos.FirstOrDefault(); //TODO: dont use this navigation property, it is getting all pictures.
                    if (photo == null)
                        album.AlbumPicUrl = DefaultImageHelper.GetDefaultAlbumImage();
                    else
                    {
                        album.AlbumPicUrl = photo.IsFacebookPhoto
                                                ? new PhotoLinks(photo.FacebookLink)
                                                : _photoService.GeneratePhotoUrl(photo.UserId, photo.Id);
                    }

                    albumsVm.Add(album);
                }

                if (Request.IsAjaxRequest())
                {
                    return View("_AlbumList", albumsVm);
                }

                var fullVm = new MainPhotoPageViewModel
                             {
                                 IsForCurrentUser = currentUserId == id,
                                 Albums = albumsVm,
                                 UserId = id,
                                 ViewType = PhotoViewType.Albums,
                                 UserDisplayName = _userService.GetUserDisplayName(id)
                             };

                return View("MainView", fullVm);
            }
        }

        [Authorize]
        public ActionResult Remove(int id)
        {
            using (_photoService)
            using (_albumService)
            using (_userService)
            {
                var userId = _userService.GetUserId(User.Identity.Name);
                _photoService.RemovePhoto(id, userId);
            }

            return Redirect(HttpContext.Request.UrlReferrer.AbsolutePath);
        }

        [Authorize]
        public ActionResult Upload(HttpPostedFileBase image, string description, int? albumId)
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

                SaveImage(image.InputStream, description, albumId);
            }

            return Redirect(HttpContext.Request.UrlReferrer.AbsolutePath);
        }

        public JsonNetResult AjaxUpload(string description, int? albumId, HttpPostedFileBase image)
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

                var photo = SaveImage(image.InputStream, description, albumId);
                response.PhotoId = photo.Id;
                response.Success = true;
                response.PhotoUrl = _photoService.GeneratePhotoUrl(photo.UserId, photo.Id).OriginalLink;

                return new JsonNetResult(response);
            }
        }

        private Photo SaveImage(Stream image, string description, int? albumId)
        {

            var userId = _userService.GetUserId(User.Identity.Name);

            if (albumId.HasValue)
            {
                var album = _albumService.GetAlbum(albumId.Value);

                if (album.UserId != userId)
                    throw new SecurityException();
            }

            var photo = new Photo
                        {
                            AlbumId = albumId,
                            Description = description,
                            UserId = userId
                        };

            _photoService.SavePhoto(photo, image, true);
            return photo;
        }

        [Authorize]
        public ActionResult Feed(int id)
        {
            using (_photoService)
            using (_albumService)
            using (_userService)
            {
                var userId = _userService.GetUserId(User.Identity.Name);
                var userDisplayName = _userService.GetUserDisplayName(userId);
                var userPhoto = _photoService.GetUserImageUrl(userId);
                var photo = _photoService.GetPhoto(id);

                var vm = new FeedViewModel
                         {
                             UserId = userId,
                             UserImageUrl = userPhoto,
                             UserDisplayName = userDisplayName,
                             Time = photo.UploadDate,
                             FeedType = FeedType.Picture,
                             IsFromCurrentUser = true,
                             PhotoViewModel = new List<PhotoViewModel>
                                              {
                                                  new PhotoViewModel
                                                  {
                                                      PhotoId = photo.Id,
                                                      PhotoUrl = _photoService.GeneratePhotoUrl(userId, photo.Id),
                                                      PhotoDescription = photo.Description,
                                                      IsFromCurrentUser = true,
                                                      FromUserDisplayName = userDisplayName,
                                                      FromUserId = userId,
                                                      FromUserPhotoUrl = userPhoto
                                                  }
                                              },
                             CommentsViewModel = new CommentsViewModel
                                                 {
                                                     Comments = new List<CommentViewModel>(),
                                                     CommentType = CommentType.Photo,
                                                     CurrentUserPhotoUrl = userPhoto,
                                                     ItemId = photo.Id
                                                 }
                         };

                return View("FeedItems/_PicturePostFeedItem", vm);
            }
        }

        [Authorize, HttpGet]
        public ActionResult Crop(int id, string @for)
        {
            using (_photoService)
            using (_albumService)
            using (_userService)
            {
                var vm = new CropViewModel();
                var photo = _photoService.GetPhoto(id);

                if (photo.IsFacebookPhoto)
                {
                    vm.IsFacebookPhoto = true;
                    return View(vm);
                }

                var userId = _userService.GetUserId(User.Identity.Name);
                if (photo.UserId != userId)
                    throw new HttpException(401, "You are not authorized to crop this image.");

                vm.PhotoUrl = _photoService.GeneratePhotoUrl(userId, photo.Id).OriginalLink;
                vm.Ratio = @for.Equals("cover", StringComparison.InvariantCultureIgnoreCase)
                               ? 2.5 : 1;

                return View(vm);
            }
        }

        [Authorize, HttpPost]
        public ActionResult Crop(int id, string @for, CropViewModel vm)
        {
            using (_photoService)
            using (_albumService)
            using (_userService)
            {
                var cropArea = new Rectangle((int)vm.X, (int)vm.Y, (int)vm.W, (int)vm.H);
                var photo = _photoService.GetPhoto(id);

                if (photo.IsFacebookPhoto)
                {
                    vm.IsFacebookPhoto = true;
                    return View(vm);
                }

                var userId = _userService.GetUserId(User.Identity.Name);
                if (photo.UserId != userId)
                    throw new HttpException(401, "You are not authorized to crop this image.");

                vm.PhotoUrl = _photoService.GeneratePhotoUrl(userId, photo.Id).OriginalLink;
                vm.Ratio = @for.Equals("cover", StringComparison.InvariantCultureIgnoreCase)
                               ? 2.5 : 1;
                
                _photoService.CropPhoto(photo, userId, cropArea);
                return View(vm);
            }
        }

        [Authorize]
        public ActionResult GetPhotos(int? albumId, int page = 1)
        {
            ViewBag.AlbumId = albumId;

            const int PAGE_SIZE = 20;
            var skip = (page - 1) * PAGE_SIZE;

            var currentUserId = _userService.GetUserId(User.Identity.Name);

            var query = _photoService.GetAll().Where(p => p.UserId == currentUserId);
            if (albumId.HasValue)
                query = query.Where(p => p.AlbumId == albumId.Value);

            var photos = query
                .OrderBy(p => p.Id)
                .Skip(skip)
                .Take(PAGE_SIZE)
                .Select(p => new
                {
                    id = p.Id,
                    userId = p.UserId,
                    isFromFb = p.IsFacebookPhoto,
                    fbUrl = p.FacebookLink
                })
                .ToList();

            var photosVm = photos.Select(p => new PhotoViewModel
            {
                IsFromCurrentUser = p.userId == currentUserId,
                PhotoId = p.id,
                PhotoUrl = p.isFromFb
                               ? new PhotoLinks(p.fbUrl)
                               : _photoService.GeneratePhotoUrl(p.userId, p.id)
            })
                                 .ToList();


            var pagedList = new StaticPagedList<PhotoViewModel>(photosVm, page, PAGE_SIZE, query.Count());

            return View("_SelectPhoto", pagedList);
        }
    }
}
