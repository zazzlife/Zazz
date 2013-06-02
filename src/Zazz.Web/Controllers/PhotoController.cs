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
using Zazz.Core.Models.Data.Enums;
using Zazz.Infrastructure;
using Zazz.Infrastructure.Helpers;
using Zazz.Web.Interfaces;
using Zazz.Web.Models;

namespace Zazz.Web.Controllers
{
    public class PhotoController : BaseController
    {
        private readonly IAlbumService _albumService;
        private readonly IImageValidator _imageValidator;

        public PhotoController(IPhotoService photoService, IAlbumService albumService,
            IUserService userService, IDefaultImageHelper defaultImageHelper, IImageValidator imageValidator) 
            : base (userService, photoService, defaultImageHelper)
        {
            _albumService = albumService;
            _imageValidator = imageValidator;
        }

        [Authorize]
        public ActionResult Index()
        {
            var userId = UserService.GetUserId(User.Identity.Name);
            return RedirectToAction("List", new { id = userId, page = 1 });
        }

        public ActionResult List(int id, int? albumId, int page = 1)
        {
            const int PAGE_SIZE = 30;
            var skip = (page - 1) * PAGE_SIZE;

            var currentUserId = 0;
            if (Request.IsAuthenticated)
                currentUserId = UserService.GetUserId(User.Identity.Name);

            var query = PhotoService.GetAll().Where(p => p.UserId == id);
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
                                                                 : PhotoService.GeneratePhotoUrl(p.userId, p.id),
                                                  FromUserId = id,
                                                  FromUserDisplayName = UserService.GetUserDisplayName(id),
                                                  PhotoDescription = p.description,
                                                  FromUserPhotoUrl = PhotoService.GetUserImageUrl(id)
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
                         UserDisplayName = UserService.GetUserDisplayName(id)
                     };

            return View("MainView", vm);
        }

        public ActionResult Albums(int id)
        {
            var albums = _albumService.GetUserAlbums(id);

            var currentUserId = 0;
            if (User.Identity.IsAuthenticated)
                currentUserId = UserService.GetUserId(User.Identity.Name);

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
                                            : PhotoService.GeneratePhotoUrl(photo.UserId, photo.Id);
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
                             UserDisplayName = UserService.GetUserDisplayName(id)
                         };

            return View("MainView", fullVm);
        }

        [Authorize]
        public ActionResult Remove(int id)
        {
            var userId = UserService.GetUserId(User.Identity.Name);
            PhotoService.RemovePhoto(id, userId);

            return Redirect(HttpContext.Request.UrlReferrer.AbsolutePath);
        }

        [Authorize]
        public ActionResult Upload(HttpPostedFileBase image, string description, int? albumId, bool showInFeed)
        {
            var errorMessage = "Image was not valid";
            if (image == null || !_imageValidator.IsValid(image, out errorMessage))
            {
                ShowAlert(errorMessage, AlertType.Error);
                return Redirect(HttpContext.Request.UrlReferrer.AbsolutePath);
            }

            SaveImage(image.InputStream, description, albumId, showInFeed);

            return Redirect(HttpContext.Request.UrlReferrer.AbsolutePath);
        }

        public JsonNetResult AjaxUpload(string description, int? albumId, HttpPostedFileBase image, bool showInFeed)
        {
            var response = new FineUploadResponse();
            var errorMessage = "Image was not valid";
            if (image == null || !_imageValidator.IsValid(image, out errorMessage))
            {
                response.Success = false;
                response.Error = errorMessage;
                return new JsonNetResult(response);
            }

            var photo = SaveImage(image.InputStream, description, albumId, showInFeed);
            response.PhotoId = photo.Id;
            response.Success = true;
            response.PhotoUrl = PhotoService.GeneratePhotoUrl(photo.UserId, photo.Id).OriginalLink;

            return new JsonNetResult(response);
        }

        private Photo SaveImage(Stream image, string description, int? albumId, bool showInFeed)
        {
            var userId = UserService.GetUserId(User.Identity.Name);

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

            PhotoService.SavePhoto(photo, image, showInFeed);
            return photo;
        }

        [Authorize]
        public ActionResult Feed(int id)
        {
            var userId = UserService.GetUserId(User.Identity.Name);
            var userDisplayName = UserService.GetUserDisplayName(userId);
            var userPhoto = PhotoService.GetUserImageUrl(userId);
            var photo = PhotoService.GetPhoto(id);

            var vm = new FeedViewModel
                     {
                         UserId = userId,
                         UserImageUrl = userPhoto,
                         UserDisplayName = userDisplayName,
                         Time = photo.UploadDate,
                         FeedType = FeedType.Photo,
                         IsFromCurrentUser = true,
                         PhotoViewModel = new List<PhotoViewModel>
                                              {
                                                  new PhotoViewModel
                                                  {
                                                      PhotoId = photo.Id,
                                                      PhotoUrl = PhotoService.GeneratePhotoUrl(userId, photo.Id),
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

        [Authorize, HttpGet]
        public ActionResult Crop(int id, string @for)
        {
            var userId = UserService.GetUserId(User.Identity.Name);
            var photo = PhotoService.GetPhoto(id);

            if (photo.UserId != userId)
                throw new HttpException(401, "You are not authorized to crop this image.");

            var vm = new CropViewModel();

            if (photo.IsFacebookPhoto)
            {
                vm.IsFacebookPhoto = true;
                return View(vm);
            }

            vm.PhotoUrl = PhotoService.GeneratePhotoUrl(userId, photo.Id).OriginalLink;
            vm.Ratio = @for.Equals("cover", StringComparison.InvariantCultureIgnoreCase)
                           ? 2.5 : 1;

            return View(vm);
        }

        [Authorize, HttpPost]
        public ActionResult Crop(int id, string @for, CropViewModel vm)
        {
            var cropArea = new Rectangle((int)vm.X, (int)vm.Y, (int)vm.W, (int)vm.H);
            var photo = PhotoService.GetPhoto(id);

            if (photo.IsFacebookPhoto)
            {
                vm.IsFacebookPhoto = true;
                return View(vm);
            }

            var userId = UserService.GetUserId(User.Identity.Name);
            if (photo.UserId != userId)
                throw new HttpException(401, "You are not authorized to crop this image.");

            vm.PhotoUrl = PhotoService.GeneratePhotoUrl(userId, photo.Id).OriginalLink;
            vm.Ratio = @for.Equals("cover", StringComparison.InvariantCultureIgnoreCase)
                           ? 2.5 : 1;

            PhotoService.CropPhoto(photo, userId, cropArea);
            return View(vm);
        }

        [Authorize]
        public ActionResult GetPhotos(int? albumId, int page = 1)
        {
            ViewBag.AlbumId = albumId;

            const int PAGE_SIZE = 20;
            var skip = (page - 1) * PAGE_SIZE;

            var currentUserId = UserService.GetUserId(User.Identity.Name);

            var query = PhotoService.GetAll().Where(p => p.UserId == currentUserId);
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
                               : PhotoService.GeneratePhotoUrl(p.userId, p.id)
            })
                                 .ToList();


            var pagedList = new StaticPagedList<PhotoViewModel>(photosVm, page, PAGE_SIZE, query.Count());

            return View("_SelectPhoto", pagedList);
        }
    }
}
