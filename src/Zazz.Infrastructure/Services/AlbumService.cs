using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using Zazz.Core.Exceptions;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.DTOs;

namespace Zazz.Infrastructure.Services
{
    public class AlbumService : IAlbumService
    {
        private readonly IUoW _uow;
        private readonly IPhotoService _photoService;

        public AlbumService(IUoW uow, IPhotoService photoService)
        {
            _uow = uow;
            _photoService = photoService;
        }

        public AlbumWithThumbnailDTO GetAlbumWithThumbnail(int albumId)
        {
            return _uow.AlbumRepository.GetAlbumWithThumbnail(albumId);
        }

        public Album GetAlbum(int albumId, bool includePhotos)
        {
            return _uow.AlbumRepository.GetById(albumId);
        }

        public IQueryable<Album> GetUserAlbums(int userId, bool includePhotos = false)
        {
            return _uow.AlbumRepository.GetUserAlbums(userId, includePhotos);
        }

        public void CreateAlbum(Album album)
        {
            if (album.CreatedDate == default (DateTime))
                album.CreatedDate = DateTime.UtcNow;

            _uow.AlbumRepository.InsertGraph(album);
            // there is a direct call to repository in FacebookService (get page photos)
            _uow.SaveChanges();
        }

        public void UpdateAlbum(int albumId, string newName, int currentUserId)
        {
            var album = _uow.AlbumRepository.GetById(albumId);
            if (album == null)
                throw new NotFoundException();

            if (album.UserId != currentUserId)
                throw new SecurityException();

            album.Name = newName;
            _uow.SaveChanges();
        }

        public void DeleteAlbum(int albumId, int currentUserId)
        {
            var album = _uow.AlbumRepository.GetById(albumId, true);

            if (album == null)
                throw new NotFoundException();

            if (album.UserId != currentUserId)
                throw new SecurityException();

            foreach (var p in album.Photos)
                _photoService.RemovePhoto(p.Id, currentUserId);

            _uow.AlbumRepository.Remove(album);
            _uow.SaveChanges();
        }
    }
}