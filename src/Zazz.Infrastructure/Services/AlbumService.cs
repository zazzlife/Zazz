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
        private readonly IUoW _uoW;
        private readonly IPhotoService _photoService;

        public AlbumService(IUoW uoW, IPhotoService photoService)
        {
            _uoW = uoW;
            _photoService = photoService;
        }

        public Task<Album> GetAlbumAsync(int albumId)
        {
            return _uoW.AlbumRepository.GetByIdAsync(albumId);
        }

        public Task<List<Album>> GetUserAlbumsAsync(int userId, int skip, int take)
        {
            return Task.Run(() => _uoW.AlbumRepository.GetAll()
                                      .Where(a => a.UserId == userId)
                                      .OrderBy(a => a.Id)
                                      .Skip(skip)
                                      .Take(take).ToList());
        }

        public Task<List<Album>> GetUserAlbumsAsync(int userId)
        {
            return Task.Run(() => _uoW.AlbumRepository.GetAll()
                                      .Where(a => a.UserId == userId).ToList());
        }

        public Task<int> GetUserAlbumsCountAsync(int userId)
        {
            return Task.Run(() => _uoW.AlbumRepository.GetAll().Count(a => a.UserId == userId));
        }

        public async Task CreateAlbumAsync(Album album)
        {
            _uoW.AlbumRepository.InsertGraph(album);

            await _uoW.SaveAsync();
        }

        public async Task UpdateAlbumAsync(Album album, int currentUserId)
        {
            if (album.Id == 0)
                throw new ArgumentException("Album id cannot be 0");

            var ownerId = await _uoW.AlbumRepository.GetOwnerIdAsync(album.Id);
            if (ownerId != currentUserId)
                throw new SecurityException();

            _uoW.AlbumRepository.InsertOrUpdate(album);
            await _uoW.SaveAsync();
        }

        public async Task DeleteAlbumAsync(int albumId, int currentUserId)
        {
            if (albumId == 0)
                throw new ArgumentException("Album Id cannot be 0", "albumId");

            var ownerId = await _uoW.AlbumRepository.GetOwnerIdAsync(albumId);
            if (ownerId != currentUserId)
                throw new SecurityException();

            var photosIds = _uoW.AlbumRepository.GetAlbumPhotoIds(albumId).ToList();

            foreach (var photoId in photosIds)
                await _photoService.RemovePhotoAsync(photoId, currentUserId);

            await _uoW.AlbumRepository.RemoveAsync(albumId);
            await _uoW.SaveAsync();
        }

        public void Dispose()
        {
            _uoW.Dispose();
        }
    }
}