﻿using System;
using System.Collections.Generic;
using System.Drawing;
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
        private readonly ICacheService _cacheService;

        public PhotoService(IUoW uow, IFileService fileService, string rootPath, ICacheService cacheService)
        {
            _uow = uow;
            _fileService = fileService;
            _rootPath = rootPath;
            _cacheService = cacheService;
        }

        public IQueryable<Photo> GetAll()
        {
            return _uow.PhotoRepository.GetAll();
        }

        public Task<Photo> GetPhotoAsync(int id)
        {
            return _uow.PhotoRepository.GetByIdAsync(id);
        }

        public string GeneratePhotoUrl(int userId, int photoId)
        {
            return String.Format("/picture/user/{0}/{1}.jpg", userId, photoId);
        }

        public string GeneratePhotoFilePath(int userId, int photoId)
        {
            return String.Format(@"{0}\picture\user\{1}\{2}.jpg", _rootPath, userId, photoId);
        }

        public Task<string> GetPhotoDescriptionAsync(int photoId)
        {
            return _uow.PhotoRepository.GetDescriptionAsync(photoId);
        }

        public async Task<int> SavePhotoAsync(Photo photo, Stream data, bool showInFeed)
        {
            photo.UploadDate = DateTime.UtcNow;
            _uow.PhotoRepository.InsertGraph(photo);
            _uow.SaveChanges();

            if (showInFeed)
            {
                // Checking if the last user feed is photo and it is less than 24 hours.

                var lastFeed = _uow.FeedRepository.GetUserLastFeed(photo.UserId);
                if (lastFeed != null && lastFeed.FeedType == FeedType.Picture &&
                    lastFeed.Time >= DateTime.UtcNow.AddDays(-1))
                {
                    lastFeed.FeedPhotoIds.Add(new FeedPhotoId
                                              {
                                                  PhotoId = photo.Id
                                              });
                }
                else
                {
                    var feed = new Feed
                               {
                                   FeedType = FeedType.Picture,
                                   Time = photo.UploadDate,
                                   UserId = photo.UserId
                               };

                    feed.FeedPhotoIds.Add(new FeedPhotoId
                                          {
                                              PhotoId = photo.Id
                                          });

                    _uow.FeedRepository.InsertGraph(feed);
                }

                _uow.SaveChanges();
            }

            var path = GeneratePhotoFilePath(photo.UserId, photo.Id);

            if (data != Stream.Null)
                await _fileService.SaveFileAsync(path, data);

            return photo.Id;
        }

        public async Task RemovePhotoAsync(int photoId, int currentUserId)
        {
            var photo = await _uow.PhotoRepository.GetByIdAsync(photoId);
            if (photo.UserId != currentUserId)
                throw new SecurityException();

            var userDetail = photo.User.UserDetail;

            if (photo.Id == userDetail.ProfilePhotoId)
                photo.User.UserDetail.ProfilePhotoId = 0;

            if (photo.Id == userDetail.CoverPhotoId)
                photo.User.UserDetail.CoverPhotoId = 0;

            // removing photo id from the feed photo ids and if it's the last one in the collection, will delete the feed.

            _uow.FeedPhotoIdRepository.RemoveByPhotoIdAndReturnFeedId(photoId);

            _uow.EventRepository.ResetPhotoId(photoId);
            _uow.UserRepository.ResetPhotoId(photoId);
            _uow.CommentRepository.RemovePhotoComments(photoId);

            _uow.PhotoRepository.Remove(photo);
            _uow.SaveChanges();

            var filePath = GeneratePhotoFilePath(photo.UserId, photo.Id);
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
            _uow.SaveChanges();
        }

        public string GetUserImageUrl(int userId)
        {
            var cache = _cacheService.GetUserPhotoUrl(userId);
            if (!String.IsNullOrEmpty(cache))
                return cache;

            var photoId = _uow.UserRepository.GetUserPhotoId(userId);

            var img = String.Empty;

            if (photoId == 0)
            {
                var gender = _uow.UserRepository.GetUserGender(userId);
                img = DefaultImageHelper.GetUserDefaultImage(gender);
            }
            else
            {
                var photo = _uow.PhotoRepository.GetPhotoWithMinimalData(photoId);
                if (photo == null)
                {
                    var gender = _uow.UserRepository.GetUserGender(userId);
                    img = DefaultImageHelper.GetUserDefaultImage(gender);
                }
                else if (photo.IsFacebookPhoto)
                {
                    img = photo.FacebookPicUrl;
                }
                else
                {
                    img = GeneratePhotoUrl(photo.UserId, photo.Id);
                }
            }

            _cacheService.AddUserPhotoUrl(userId, img);

            return img;
        }

        //http://tech.pro/tutorial/620/csharp-tutorial-image-editing-saving-cropping-and-resizing
        public void CropPhoto(Photo photo, int currentUserId, Rectangle cropArea)
        {
            if (photo.UserId != currentUserId)
                throw new SecurityException();

            var imgPath = GeneratePhotoFilePath(photo.UserId, photo.Id);

            using (var bmp = new Bitmap(imgPath))
            using (var croppedBmp = bmp.Clone(cropArea, bmp.PixelFormat))
            {
                bmp.Dispose();
                croppedBmp.Save(imgPath);
            }
        }

        public void Dispose()
        {
            _uow.Dispose();
        }
    }
}