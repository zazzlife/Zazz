using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Web.Http;
using Zazz.Core.Exceptions;
using Zazz.Core.Interfaces;
using Zazz.Core.Models;
using Zazz.Core.Models.Data;
using Zazz.Web.Filters;
using Zazz.Web.Interfaces;
using Zazz.Web.Models.Api;

namespace Zazz.Web.Controllers.Api
{
    [HMACAuthorize]
    public class AlbumController : BaseApiController
    {
        private readonly IAlbumService _albumService;
        private readonly IDefaultImageHelper _defaultImageHelper;
        private readonly IPhotoService _photoService;
        private readonly IObjectMapper _objectMapper;

        public AlbumController(IAlbumService albumService, IDefaultImageHelper defaultImageHelper,
            IPhotoService photoService, IObjectMapper objectMapper)
        {
            _albumService = albumService;
            _defaultImageHelper = defaultImageHelper;
            _photoService = photoService;
            _objectMapper = objectMapper;
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
                       Photos = album.Photos.Select(_objectMapper.PhotoToApiPhoto)
                   };
        }

        // POST api/v1/album
        public ApiAlbum Post(ApiAlbum album)
        {
            var a = new Album
                    {
                        CreatedDate = DateTime.UtcNow,
                        IsFacebookAlbum = false,
                        Name = album.Name,
                        UserId = ExtractUserIdFromHeader(),
                    };

            _albumService.CreateAlbum(a);
            
            album.Id = a.Id;
            album.CreatedDate = a.CreatedDate;
            album.Thumbnail = _defaultImageHelper.GetDefaultAlbumImage();

            return album;
        }

        // PUT api/v1/album/5
        public void Put(int id, ApiAlbum album)
        {
            if (id == 0)
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            try
            {
                _albumService.UpdateAlbum(id, album.Name, ExtractUserIdFromHeader());
            }
            catch (NotFoundException)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            catch (SecurityException)
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            }
        }

        // DELETE api/v1/album/5
        public void Delete(int id)
        {
            if (id == 0)
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            try
            {
                _albumService.DeleteAlbum(id, ExtractUserIdFromHeader());
            }
            catch (SecurityException)
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            }
        }
    }
}
