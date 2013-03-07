using System;
using System.IO;
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
            throw new System.NotImplementedException();
        }

        public Task SavePhotoAsync(Photo photo, Stream data)
        {
            throw new System.NotImplementedException();
        }

        public Task RemovePhotoAsync(int photoId, int currentUserId)
        {
            throw new System.NotImplementedException();
        }

        public Task UpdatePhotoAsync(Photo photo, int currentUserId)
        {
            throw new System.NotImplementedException();
        }

        public void Dispose()
        {
            _uoW.Dispose();
        }
    }
}