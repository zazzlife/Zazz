using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Infrastructure.Services
{
    public class PhotoService : IPhotoService
    {
        private readonly IUoW _uoW;
        private readonly IFileService _fileService;
        private readonly string _rootPath;

        public PhotoService(IUoW uoW, IFileService fileService, string rootPath)
        {
            _uoW = uoW;
            _fileService = fileService;
            _rootPath = rootPath;
        }

        public List<Photo> GetAlbumPhotos(int albumId, int skip, int take)
        {
            return _uoW.PhotoRepository.GetAll()
                       .Where(p => p.AlbumId == albumId)
                       .OrderBy(p => p.Id)
                       .Skip(skip)
                       .Take(take)
                       .ToList();
        }

        public int GetAlbumPhotosCount(int albumId)
        {
            return _uoW.PhotoRepository.GetAll()
                       .Count(p => p.AlbumId == albumId);
        }

        public string GeneratePhotoUrl(int userId, int albumId, int photoId)
        {
            return String.Format("/picture/user/{0}/{1}/{2}.jpg", userId, albumId, photoId);
        }

        public string GeneratePhotoFilePath(int userId, int albumId, int photoId)
        {
            return String.Format(@"{0}\picture\user\{1}\{2}\{3}.jpg", _rootPath, userId, albumId, photoId);
        }

        public Task<string> GetPhotoDescriptionAsync(int photoId)
        {
            return _uoW.PhotoRepository.GetDescriptionAsync(photoId);
        }

        public async Task<int> SavePhotoAsync(Photo photo, Stream data)
        {
            photo.UploadDate = DateTime.UtcNow;
            _uoW.PhotoRepository.InsertGraph(photo);
            await _uoW.SaveAsync();

            var path = GeneratePhotoFilePath(photo.UploaderId, photo.AlbumId, photo.Id);
            await _fileService.SaveFileAsync(path, data);

            return photo.Id;
        }

        public async Task RemovePhotoAsync(int photoId, int currentUserId)
        {
            var photo = await _uoW.PhotoRepository.GetByIdAsync(photoId);
            if (photo.UploaderId != currentUserId)
                throw new SecurityException();

            _uoW.PhotoRepository.Remove(photo);
            await _uoW.SaveAsync();

            var filePath = GeneratePhotoFilePath(photo.UploaderId, photo.AlbumId, photo.Id);
            _fileService.RemoveFile(filePath);
        }

        public async Task UpdatePhotoAsync(Photo photo, int currentUserId)
        {
            if (photo.Id == 0)
                throw new ArgumentException();

            var ownerId = await _uoW.PhotoRepository.GetOwnerIdAsync(photo.Id);
            if (ownerId != currentUserId)
                throw new SecurityException();

            _uoW.PhotoRepository.InsertOrUpdate(photo);
            await _uoW.SaveAsync();
        }

        public void Dispose()
        {
            _uoW.Dispose();
        }
    }
}