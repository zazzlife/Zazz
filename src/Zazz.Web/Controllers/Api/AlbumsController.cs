﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Web.Http;
using Zazz.Core.Exceptions;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Services;
using Zazz.Core.Models;
using Zazz.Core.Models.Data;
using Zazz.Web.Filters;
using Zazz.Web.Interfaces;
using Zazz.Web.Models.Api;

namespace Zazz.Web.Controllers.Api
{
    [OAuth2Authorize]
    public class AlbumsController : BaseApiController
    {
        private readonly IAlbumService _albumService;
        private readonly IDefaultImageHelper _defaultImageHelper;
        private readonly IPhotoService _photoService;
        private readonly IObjectMapper _objectMapper;

        public AlbumsController(IAlbumService albumService, IDefaultImageHelper defaultImageHelper,
            IPhotoService photoService, IObjectMapper objectMapper)
        {
            _albumService = albumService;
            _defaultImageHelper = defaultImageHelper;
            _photoService = photoService;
            _objectMapper = objectMapper;
        }

        // GET api/v1/users/{id}/albums?lastAlbum=
        public IEnumerable<ApiAlbum> GetUserAlbums(int userId, int? lastAlbum = null)
        {
            if (userId < 1)
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            var albums = _albumService.GetUserAlbums(userId, true)
                .ToList()
                .Select(a => new ApiAlbum
                             {
                                 CreatedDate = a.CreatedDate,
                                 AlbumId = a.Id,
                                 Name = a.Name,
                                 UserId = a.UserId,
                                 Thumbnail = a.Photos.FirstOrDefault() == null
                                 ? _defaultImageHelper.GetDefaultAlbumImage()
                                 : _photoService.GeneratePhotoUrl(a.UserId, a.Photos.First().Id),
                                 Photos = a.Photos.Select(_objectMapper.PhotoToApiPhoto)
                             });

            return albums;
        }

        // GET api/v1/albums/5
        public ApiAlbum Get(int id)
        {
            if (id == 0)
                throw new HttpResponseException(HttpStatusCode.BadRequest);

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
                albumThumbnail = _photoService.GeneratePhotoUrl(firstPhoto.UserId, firstPhoto.Id);
            }

            return new ApiAlbum
                   {
                       CreatedDate = album.CreatedDate,
                       AlbumId = album.Id,
                       Name = album.Name,
                       UserId = album.UserId,
                       Thumbnail = albumThumbnail,
                       Photos = album.Photos.Select(_objectMapper.PhotoToApiPhoto)
                   };
        }

        // POST api/v1/albums
        public HttpResponseMessage Post(ApiAlbum album)
        {
            var a = new Album
                    {
                        CreatedDate = DateTime.UtcNow,
                        IsFacebookAlbum = false,
                        Name = album.Name,
                        UserId = CurrentUserId
                    };

            _albumService.CreateAlbum(a);
            
            album.AlbumId = a.Id;
            album.CreatedDate = a.CreatedDate;
            album.Thumbnail = _defaultImageHelper.GetDefaultAlbumImage();

            var response = Request.CreateResponse(HttpStatusCode.Created, album);
            return response;
        }

        // PUT api/v1/albums/5
        public void Put(int id, ApiAlbum album)
        {
            if (id == 0)
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            try
            {
                _albumService.UpdateAlbum(id, album.Name, CurrentUserId);
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

        // DELETE api/v1/albums/5
        public void Delete(int id)
        {
            if (id == 0)
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            try
            {
                _albumService.DeleteAlbum(id, CurrentUserId);
            }
            catch (SecurityException)
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            }
        }
    }
}
