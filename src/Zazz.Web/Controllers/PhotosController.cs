﻿using System;
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
using Zazz.Core.Interfaces.Repositories;
using Zazz.Core.Interfaces.Services;
using Zazz.Core.Models;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;
using Zazz.Infrastructure;
using Zazz.Infrastructure.Helpers;
using Zazz.Web.Interfaces;
using Zazz.Web.Models;

namespace Zazz.Web.Controllers
{
    public class PhotosController : BaseController
    {
        private readonly IAlbumService _albumService;
        private readonly IImageValidator _imageValidator;
        private readonly IUoW _uow;
        private readonly ICategoryService _categoryService;
        private readonly ICategoryStatsCache _categoryStatsCache;

        public PhotosController(IPhotoService photoService, IAlbumService albumService,
                                IUserService userService, IDefaultImageHelper defaultImageHelper,
                                IImageValidator imageValidator, IStaticDataRepository staticDataRepository,
                                ICategoryService categoryService, IUoW uow, ICategoryStatsCache categoryStatsCache)
            : base(userService, photoService, defaultImageHelper, staticDataRepository, categoryService)
        {
            _albumService = albumService;
            _imageValidator = imageValidator;
            _uow = uow;
            _categoryService = categoryService;
            _categoryStatsCache = categoryStatsCache;
        }

        [Authorize]
        public ActionResult Index()
        {
            var userId = UserService.GetUserId(User.Identity.Name);
            return RedirectToAction("List", new { id = userId, page = 1 });
        }

        public ActionResult List(int id, int? albumId)
        {
            var currentUserId = 0;
            if (Request.IsAuthenticated)
                currentUserId = UserService.GetUserId(User.Identity.Name);

            var query = PhotoService.GetAll().Where(p => p.UserId == id);
            if (albumId.HasValue)
                query = query.Where(p => p.AlbumId == albumId.Value);

            var photos = query
                .OrderBy(p => p.Id)
                .Select(p => new
                             {
                                 id = p.Id,
                                 userId = p.UserId,
                                 description = p.Description,
                                 taguser = p.TagUser
                             })
                .ToList();

            var photosVm = photos.Select(p => new PhotoViewModel
                                              {
                                                  IsFromCurrentUser = p.userId == currentUserId,
                                                  PhotoId = p.id,
                                                  PhotoUrl = PhotoService.GeneratePhotoUrl(p.userId, p.id),
                                                  FromUserId = id,
                                                  FromUserDisplayName = UserService.GetUserDisplayName(id),
                                                  Description = p.description,
                                                  FromUserPhotoUrl = PhotoService.GetUserDisplayPhoto(id),
                                                  TagUser = p.taguser
                                              })
                                 .ToList();

            if (Request.IsAjaxRequest())
            {
                return View("_PhotoList", photosVm);
            }


            var vm = new PhotoListViewModel
                     {
                         IsForCurrentUser = currentUserId == id,
                         CurrentUserId = currentUserId,
                         UserId = id,
                         Photos = photosVm
                     };

            return View(vm);
        }

        public ActionResult Albums(int id)
        {
            var currentUserId = 0;
            if (User.Identity.IsAuthenticated)
                currentUserId = UserService.GetUserId(User.Identity.Name);

            var albums = _albumService.GetUserAlbums(id, true);
            var vm = new AlbumsListViewModel
                     {
                         IsForCurrentUser = currentUserId == id,
                         Albums = new List<AlbumViewModel>(),
                         UserId = id,
                         CurrentUserId = currentUserId
                     };

            foreach (var a in albums)
            {
                var album = new AlbumViewModel
                            {
                                AlbumId = a.Id,
                                AlbumName = a.Name,
                                IsFromCurrentUser = currentUserId == a.UserId,
                                UserId = a.UserId,
                                PhotosCount = a.Photos.Count,
                                AlbumThumbnails = a.Photos.Count > 1
                                                      ? a.Photos
                                                         .Select(p => PhotoService.GeneratePhotoUrl(p.UserId, p.Id))
                                                         .Take(3)
                                                      : new List<PhotoLinks>(1)
                                                        {
                                                            DefaultImageHelper
                                                                .GetDefaultAlbumImage()
                                                        }
                            };

                vm.Albums.Add(album);
            }

            return View(vm);
        }

        [Authorize]
        public ActionResult Remove(int id)
        {
            var userId = UserService.GetUserId(User.Identity.Name);
            PhotoService.RemovePhoto(id, userId);

            return Redirect(HttpContext.Request.UrlReferrer.AbsolutePath);
        }

        [Authorize, HttpPost]
        public ActionResult UpdateCategories(int id, IEnumerable<int> categories)
        {
            var photo = PhotoService.GetPhoto(id);
            var userId = UserService.GetUserId(User.Identity.Name); 

            PhotoService.UpdatePhoto(photo, userId, categories);

            _categoryService.UpdateStatistics();
            _categoryStatsCache.LastUpdate = DateTime.UtcNow.AddMinutes(-6);
            return Redirect(HttpContext.Request.UrlReferrer.AbsolutePath);
        }

        [Authorize]
        public ActionResult Upload(HttpPostedFileBase image, string description, int? albumId, bool showInFeed)
        {
            using (image.InputStream)
            {
                var errorMessage = "Image was not valid";
                if (!_imageValidator.IsValid(image, out errorMessage))
                {
                    ShowAlert(errorMessage, AlertType.Error);
                    return Redirect(HttpContext.Request.UrlReferrer.AbsolutePath);
                }

                SaveImage(image.InputStream, description, albumId, showInFeed, Enumerable.Empty<int>(),"");
                return Redirect(HttpContext.Request.UrlReferrer.AbsolutePath);
            }
        }

        public JsonNetResult AjaxUpload(string description, int? albumId, HttpPostedFileBase image, bool showInFeed)
        {
            using (image.InputStream)
            {
                var categories = Enumerable.Empty<int>();

                var c = Request.Params["categories"];
                if (!String.IsNullOrWhiteSpace(c))
                    categories = c.Split(',').Select(Int32.Parse);

                var tagusers = Request.Params["taguser"];

                var response = new FineUploadResponse();
                var errorMessage = "Image was not valid";
                if (!_imageValidator.IsValid(image, out errorMessage))
                {
                    response.Success = false;
                    response.Error = errorMessage;
                    return new JsonNetResult(response);
                }

                var photo = SaveImage(image.InputStream, description, albumId, showInFeed, categories ,tagusers);
                response.PhotoId = photo.Id;
                response.Success = true;
                response.PhotoUrl = PhotoService.GeneratePhotoUrl(photo.UserId, photo.Id).OriginalLink;

                return new JsonNetResult(response);
            }
        }

        private Photo SaveImage(Stream image, string description, int? albumId, bool showInFeed,
            IEnumerable<int> categories, string tagusers)
        {
            var userId = UserService.GetUserId(User.Identity.Name);
            var photo = new Photo
                        {
                            AlbumId = albumId,
                            Description = description,
                            UserId = userId,
                            TagUser = tagusers
                        };

            PhotoService.SavePhoto(photo, image, showInFeed, categories);
            return photo;
        }

        [Authorize]
        public ActionResult Feed(int id)
        {
            var userId = UserService.GetUserId(User.Identity.Name);
            var userDisplayName = UserService.GetUserDisplayName(userId);
            var userPhoto = PhotoService.GetUserDisplayPhoto(userId);
            var photo = PhotoService.GetPhoto(id);

            var vm = new FeedViewModel
                     {
                         UserId = userId,
                         UserDisplayPhoto = userPhoto,
                         UserDisplayName = userDisplayName,
                         Time = photo.UploadDate,
                         FeedType = FeedType.Photo,
                         IsFromCurrentUser = true,
                         Photos = new List<PhotoViewModel>
                                              {
                                                  new PhotoViewModel
                                                  {
                                                      PhotoId = photo.Id,
                                                      PhotoUrl = PhotoService.GeneratePhotoUrl(userId, photo.Id),
                                                      Description = photo.Description,
                                                      IsFromCurrentUser = true,
                                                      FromUserDisplayName = userDisplayName,
                                                      FromUserId = userId,
                                                      FromUserPhotoUrl = userPhoto,
                                                      TagUser = photo.TagUser
                                                  }
                                              },
                         Comments = new CommentsViewModel
                                             {
                                                 Comments = new List<CommentViewModel>(),
                                                 CommentType = CommentType.Photo,
                                                 CurrentUserDisplayPhoto = userPhoto,
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
                                 taguser = p.TagUser
                             })
                .ToList();

            var photosVm = photos.Select(p => new PhotoViewModel
                                              {
                                                  IsFromCurrentUser = p.userId == currentUserId,
                                                  PhotoId = p.id,
                                                  PhotoUrl = PhotoService.GeneratePhotoUrl(p.userId, p.id),
                                                  TagUser = p.taguser
                                              })
                                 .ToList();


            var pagedList = new StaticPagedList<PhotoViewModel>(photosVm, page, PAGE_SIZE, query.Count());

            return View("_SelectPhoto", pagedList);
        }
    }
}
