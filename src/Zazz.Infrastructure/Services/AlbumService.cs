using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

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

        public Album GetAlbum(int albumId)
        {
            return _uow.AlbumRepository.GetById(albumId);
        }

        public List<Album> GetUserAlbums(int userId, int skip, int take)
        {
            return _uow.AlbumRepository.GetAll()
                       .Where(a => a.UserId == userId)
                       .OrderBy(a => a.Id)
                       .Skip(skip)
                       .Take(take).ToList();
        }

        public List<Album> GetUserAlbums(int userId)
        {
            return _uow.AlbumRepository.GetAll()
                       .Where(a => a.UserId == userId).ToList();
        }

        public int GetUserAlbumsCount(int userId)
        {
            return _uow.AlbumRepository.GetAll().Count(a => a.UserId == userId);
        }

        public void CreateAlbum(Album album)
        {
            _uow.AlbumRepository.InsertGraph(album);
            // there is a direct call to repository in FacebookService (get page photos)
            _uow.SaveChanges();
        }

        public void UpdateAlbum(Album album, int currentUserId)
        {
            if (album.Id == 0)
                throw new ArgumentException("Album id cannot be 0");

            var ownerId = _uow.AlbumRepository.GetOwnerId(album.Id);
            if (ownerId != currentUserId)
                throw new SecurityException();

            _uow.AlbumRepository.InsertOrUpdate(album);
            _uow.SaveChanges();
        }

        public async Task DeleteAlbumAsync(int albumId, int currentUserId)
        {
            if (albumId == 0)
                throw new ArgumentException("Album Id cannot be 0", "albumId");

            var ownerId = _uow.AlbumRepository.GetOwnerId(albumId);
            if (ownerId != currentUserId)
                throw new SecurityException();

            var photosIds = _uow.AlbumRepository.GetAlbumPhotoIds(albumId).ToList();

            foreach (var photoId in photosIds)
                _photoService.RemovePhoto(photoId, currentUserId);

            _uow.AlbumRepository.Remove(albumId);
            _uow.SaveChanges();
        }

        public void Dispose()
        {
            _uow.Dispose();
        }
    }
}