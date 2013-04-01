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
using Zazz.Web.Models;

namespace Zazz.Web.Controllers
{
    public class AlbumController : BaseController
    {
        private readonly IAlbumService _albumService;
        private readonly IUserService _userService;
        private readonly IPhotoService _photoService;

        public AlbumController(IAlbumService albumService, IUserService userService, IPhotoService photoService)
        {
            _albumService = albumService;
            _userService = userService;
            _photoService = photoService;
        }

        [Authorize]
        public ActionResult Index()
        {
            using (_photoService)
            using (_albumService)
            using (_userService)
            {
                var userId = _userService.GetUserId(User.Identity.Name);
                return RedirectToAction("List", "Album", new { id = userId, page = 1 });
            }
        }

        public async Task<ActionResult> List(int id, int page = 1)
        {
            if (page == 0)
                throw new HttpException(400, "Bad Request");

            const int PAGE_SIZE = 6;
            var skip = (page - 1) * PAGE_SIZE;

            using (_photoService)
            using (_albumService)
            using (_userService)
            {
                var totalAlbumsCount = await _albumService.GetUserAlbumsCountAsync(id);
                var albums = await _albumService.GetUserAlbumsAsync(id, skip, PAGE_SIZE);

                var isOwner = false;
                if (User.Identity.IsAuthenticated)
                {
                    var currentUserId = _userService.GetUserId(User.Identity.Name);
                    isOwner = currentUserId == id;
                }

                var albumsVM = new List<PhotoViewModel>();
                foreach (var album in albums)
                {
                    var albumVm = new PhotoViewModel
                                      {
                                          PhotoId = album.Id,
                                          PhotoDescription = album.Name,
                                          IsFromCurrentUser = isOwner
                                      };
                    var firstAlbumImage = album.Photos.FirstOrDefault();

                    if (firstAlbumImage == null)
                    {
                        albumVm.PhotoUrl = DefaultImageHelper.GetDefaultAlbumImage();
                    }
                    else
                    {
                        if (firstAlbumImage.IsFacebookPhoto)
                        {
                            albumVm.PhotoUrl = firstAlbumImage.FacebookLink;
                        }
                        else
                        {
                            albumVm.PhotoUrl = _photoService.GeneratePhotoUrl(firstAlbumImage.UploaderId,
                                                                              firstAlbumImage.Id);
                        }

                    }

                    albumsVM.Add(albumVm);
                }

                var pagedList = new StaticPagedList<PhotoViewModel>(albumsVM, page, PAGE_SIZE, totalAlbumsCount);

                var vm = new AlbumListViewModel
                             {
                                 IsOwner = isOwner,
                                 Albums = pagedList,
                                 UserId = id
                             };

                return View("List", vm);
            }
        }

        [Authorize, HttpPost]
        public async Task<ActionResult> CreateAlbum(string albumName)
        {
            if (albumName.Length > 50)
            {
                ShowAlert("Album name cannot be longer than 50 characters", AlertType.Error);
                throw new HttpException(400, "Bad Request");
            }
            else
            {
                using (_photoService)
                using (_albumService)
                using (_userService)
                {
                    var userId = _userService.GetUserId(User.Identity.Name);
                    var album = new Album
                    {
                        Name = albumName,
                        UserId = userId
                    };

                    await _albumService.CreateAlbumAsync(album);

                    var vm = new PhotoViewModel
                             {
                                 IsFromCurrentUser = true,
                                 PhotoDescription = albumName,
                                 PhotoId = album.Id,
                                 PhotoUrl = DefaultImageHelper.GetDefaultAlbumImage()
                             };

                    return View("_AlbumThumbnail", vm);
                }
            }
        }

        [Authorize]
        public async Task Remove(int id)
        {
            using (_photoService)
            using (_albumService)
            using (_userService)
            {
                var userId = _userService.GetUserId(User.Identity.Name);

                await _albumService.DeleteAlbumAsync(id, userId);
            }
        }

        [HttpGet]
        public async Task<ActionResult> Photos(int id, int page = 1)
        {
            using (_photoService)
            using (_albumService)
            using (_userService)
            {
                if (page == 0)
                    throw new HttpException(400, "Bad Request");

                var vm = await GetPhotosAsync(id, page);
                return View(vm);
            }
        }

        private async Task<AlbumPhotosViewModel> GetPhotosAsync(int albumId, int page)
        {
            const int PAGE_SIZE = 20;
            var skip = (page - 1)*PAGE_SIZE;

            var userId = 0;
            if (User.Identity.IsAuthenticated)
                userId = _userService.GetUserId(User.Identity.Name);

            var album = await _albumService.GetAlbumAsync(albumId);
            var isOwner = userId == album.UserId;

            var totalPhotos = album.Photos.Count();
            var photos = album.Photos
                              .OrderBy(p => p.Id)
                              .Skip(skip)
                              .Take(PAGE_SIZE);

            var photosVm = photos
                .Select(p => new PhotoViewModel
                             {
                                 PhotoId = p.Id,
                                 PhotoDescription = p.Description,
                                 PhotoUrl = _photoService.GeneratePhotoUrl(p.UploaderId, p.Id),
                                 IsFromCurrentUser = isOwner
                             });

            var pagedList = new StaticPagedList<PhotoViewModel>(photosVm, page, PAGE_SIZE, totalPhotos);

            var vm = new AlbumPhotosViewModel
                     {
                         IsOwner = isOwner,
                         AlbumId = albumId,
                         Photos = pagedList,
                         UserId = album.UserId,
                         AlbumName = album.Name
                     };

            return vm;
        }

        [HttpGet]
        public async Task<JsonResult> GetAlbums()
        {
            using (_photoService)
            using (_albumService)
            using (_userService)
            {
                if (!User.Identity.IsAuthenticated)
                    throw new HttpException(403, "");

                var userId = _userService.GetUserId(User.Identity.Name);
                var albums = await _albumService.GetUserAlbumsAsync(userId);

                var response = albums.Select(a => new { id = a.Id, name = a.Name });

                return Json(response, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetPhotos(int albumId, int page = 1)
        {
            ViewBag.AlbumId = albumId;
            var vm = await GetPhotosAsync(albumId, page);

            return View("_PhotosPartial", vm.Photos);
        }
    }
}
