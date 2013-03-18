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
        private readonly IUoW _uow;
        private readonly IFileService _fileService;
        private readonly string _rootPath;

        public PhotoService(IUoW uow, IFileService fileService, string rootPath)
        {
            _uow = uow;
            _fileService = fileService;
            _rootPath = rootPath;
        }

        public Task<Photo> GetPhotoAsync(int id)
        {
            return _uow.PhotoRepository.GetByIdAsync(id);
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
            return _uow.PhotoRepository.GetDescriptionAsync(photoId);
        }

        public async Task<int> SavePhotoAsync(Photo photo, Stream data, bool showInFeed)
        {
            photo.UploadDate = DateTime.UtcNow;
            _uow.PhotoRepository.InsertGraph(photo);
            await _uow.SaveAsync();

            if (showInFeed)
            {
                var feed = new Feed
                {
                    FeedType = FeedType.Picture,
                    PhotoId = photo.Id,
                    Time = photo.UploadDate,
                    UserId = photo.UploaderId
                };

                _uow.FeedRepository.InsertGraph(feed);
                await _uow.SaveAsync();
            }

            var path = GeneratePhotoFilePath(photo.UploaderId, photo.AlbumId, photo.Id);
            await _fileService.SaveFileAsync(path, data);

            return photo.Id;
        }

        public async Task RemovePhotoAsync(int photoId, int currentUserId)
        {
            var photo = await _uow.PhotoRepository.GetByIdAsync(photoId);
            if (photo.UploaderId != currentUserId)
                throw new SecurityException();

            var userDetail = photo.Uploader.UserDetail;

            if (photo.Id == userDetail.ProfilePhotoId)
                photo.Uploader.UserDetail.ProfilePhotoId = 0;

            if (photo.Id == userDetail.CoverPhotoId)
                photo.Uploader.UserDetail.CoverPhotoId = 0;

            _uow.FeedRepository.RemovePhotoFeed(photoId);
            _uow.PostRepository.ResetPhotoId(photoId);
            _uow.PhotoRepository.Remove(photo);
            await _uow.SaveAsync();

            var filePath = GeneratePhotoFilePath(photo.UploaderId, photo.AlbumId, photo.Id);
            _fileService.RemoveFile(filePath);
        }

        public async Task UpdatePhotoAsync(Photo photo, int currentUserId)
        {
            if (photo.Id == 0)
                throw new ArgumentException();

            var ownerId = await _uow.PhotoRepository.GetOwnerIdAsync(photo.Id);
            if (ownerId != currentUserId)
                throw new SecurityException();

            _uow.PhotoRepository.InsertOrUpdate(photo);
            await _uow.SaveAsync();
        }

        public string GetUserImageUrl(int userId)
        {
            var photoId = _uow.UserRepository.GetUserPhotoId(userId);

            if (photoId == 0)
            {
                var gender = _uow.UserRepository.GetUserGender(userId);
                return DefaultImageHelper.GetUserDefaultImage(gender);
            }

            var photo = _uow.PhotoRepository.GetPhotoWithMinimalData(photoId);
            if (photo == null)
            {
                var gender = _uow.UserRepository.GetUserGender(userId);
                return DefaultImageHelper.GetUserDefaultImage(gender);
            }

            return GeneratePhotoUrl(photo.UploaderId, photo.AlbumId, photo.Id);
        }

        public void Dispose()
        {
            _uow.Dispose();
        }
    }
}