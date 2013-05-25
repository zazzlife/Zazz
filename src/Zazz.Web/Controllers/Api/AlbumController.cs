using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Zazz.Core.Interfaces;
using Zazz.Core.Models;
using Zazz.Web.Models.Api;

namespace Zazz.Web.Controllers.Api
{
    public class AlbumController : BaseApiController
    {
        private readonly IAlbumService _albumService;
        private readonly IDefaultImageHelper _defaultImageHelper;
        private readonly IPhotoService _photoService;
        private readonly IUserService _userService;

        public AlbumController(IAlbumService albumService, IDefaultImageHelper defaultImageHelper,
            IPhotoService photoService, IUserService userService)
        {
            _albumService = albumService;
            _defaultImageHelper = defaultImageHelper;
            _photoService = photoService;
            _userService = userService;
        }

        // GET api/v1/album/5
        public ApiAlbum Get(int id)
        {
            var album = _albumService.GetAlbum(id, true);
            if (album == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            PhotoLinks albumThumbnail;
            if (album.Photos.Count < 1)
            {
                albumThumbnail = _defaultImageHelper.GetDefaultAlbumImage();
            }
            else
            {
                var firstPhoto = album.Photos.First();
                if (firstPhoto.IsFacebookPhoto)
                {
                    albumThumbnail = new PhotoLinks(firstPhoto.FacebookLink);
                }
                else
                {
                    albumThumbnail = _photoService.GeneratePhotoUrl(firstPhoto.UserId, firstPhoto.Id);
                }
            }

            return new ApiAlbum
                   {
                       CreatedDate = album.CreatedDate,
                       Id = album.Id,
                       Name = album.Name,
                       UserId = album.UserId,
                       Thumbnail = albumThumbnail,
                       Photos = album.Photos.Select(p => new ApiPhoto
                                                         {
                                                             Description = p.Description,
                                                             PhotoId = p.Id,
                                                             UserId = p.UserId,
                                                             UserDisplayName = _userService.GetUserDisplayName(p.UserId),
                                                             UserDisplayPhoto = _photoService.GetUserImageUrl(p.UserId),
                                                             PhotoLinks = p.IsFacebookPhoto
                                                             ? new PhotoLinks(p.FacebookLink)
                                                             : _photoService.GeneratePhotoUrl(p.UserId, p.Id)
                                                         })
                   };
        }

        // POST api/album
        public void Post([FromBody]string value)
        {
        }

        // PUT api/album/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/album/5
        public void Delete(int id)
        {
        }
    }
}
