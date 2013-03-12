using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using PagedList;
using Zazz.Core.Interfaces;
using Zazz.Web.Models;

namespace Zazz.Web.Controllers
{
    public class AlbumController : Controller
    {
        private readonly IAlbumService _albumService;
        private readonly IUserService _userService;

        public AlbumController(IAlbumService albumService, IUserService userService)
        {
            _albumService = albumService;
            _userService = userService;
        }

        [Authorize]
        public ActionResult Index()
        {
            using (_albumService)
            using (_userService)
            {
                var userId = _userService.GetUserId(User.Identity.Name);
                return RedirectToAction("List", "Album", new {id = userId, page = 1});
            }
        }
        
        public async Task<ActionResult> List(int id, int page = 1)
        {
            if (page == 0)
                throw new HttpException(400, "Bad Request");

            const int PAGE_SIZE = 6;
            var skip = (page - 1)*PAGE_SIZE;

            using (_albumService)
            using (_userService)
            {
                var totalAlbums = await _albumService.GetUserAlbumsCountAsync(id);
                var albums = await _albumService.GetUserAlbumsAsync(id, skip, PAGE_SIZE);

                var albumsVM = albums.Select(album => new AlbumViewModel
                                                          {
                                                              Id = album.Id,
                                                              Name = album.Name,
                                                              ThumbnailUrl = ""
                                                          });

                var isOwner = false;
                if (User.Identity.IsAuthenticated)
                {
                    var currentUserId = _userService.GetUserId(User.Identity.Name);
                    isOwner = currentUserId == id;
                }

                var pagedList = new StaticPagedList<AlbumViewModel>(albumsVM, page, PAGE_SIZE, totalAlbums);

                var vm = new AlbumListViewModel
                             {
                                 IsOwner = isOwner,
                                 Albums = pagedList
                             };

                return View("List", vm);
            }
        }

        [Authorize, HttpPost]
        public ActionResult CreateAlbum(string albumName)
        {
            return RedirectToAction("List");
        }
    }
}
