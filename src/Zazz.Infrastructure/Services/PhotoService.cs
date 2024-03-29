﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using Zazz.Core.Attributes;
using Zazz.Core.Exceptions;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Repositories;
using Zazz.Core.Interfaces.Services;
using Zazz.Core.Models;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;
using Zazz.Infrastructure.Helpers;

namespace Zazz.Infrastructure.Services
{
    public class PhotoService : IPhotoService
    {
        private readonly IUoW _uow;
        private readonly ICacheService _cacheService;
        private readonly IStringHelper _stringHelper;
        private readonly IStaticDataRepository _staticDataRepository;
        private readonly IDefaultImageHelper _defaultImageHelper;
        private readonly IImageProcessor _imageProcessor;
        private readonly IStorageService _storageService;
        private readonly INotificationService _notificationService;

        public PhotoService(IUoW uow, ICacheService cacheService,
            IStringHelper stringHelper, IStaticDataRepository staticDataRepository,
            IDefaultImageHelper defaultImageHelper, IImageProcessor imageProcessor, IStorageService storageService, INotificationService notificationService)
        {
            _uow = uow;
            _cacheService = cacheService;
            _stringHelper = stringHelper;
            _staticDataRepository = staticDataRepository;
            _defaultImageHelper = defaultImageHelper;
            _imageProcessor = imageProcessor;
            _storageService = storageService;
            _notificationService = notificationService;
        }

        public IQueryable<Photo> GetLatestUserPhotos(int userId, int count)
        {
            return _uow.PhotoRepository.GetLatestUserPhotos(userId, count);
        }

        public IQueryable<Photo> GetAll()
        {
            return _uow.PhotoRepository.GetAll();
        }

        public IQueryable<Photo> GetUserPhotos(int userId, int take, int? lastPhotoId = null)
        {
            var query = _uow.PhotoRepository.GetAll()
                            .Where(p => p.UserId == userId);

            if (lastPhotoId.HasValue)
                query = query.Where(p => p.Id < lastPhotoId.Value);

            return query.Take(take);
        }

        public Photo GetPhoto(int id)
        {
            return _uow.PhotoRepository.GetById(id);
        }

        public PhotoLinks GeneratePhotoUrl(int userId, int photoId)
        {
            var baseUrl = _storageService.BasePhotoUrl; //sample: http://www.zazzlife.com/picture/user

            var links = new PhotoLinks();
            var type = typeof(PhotoLinks);

            foreach (var p in type.GetProperties())
            {
                var attr = p.GetCustomAttributes(typeof(PhotoAttribute), false)
                                            .Cast<PhotoAttribute>()
                                            .FirstOrDefault();

                if (attr == null)
                    continue;

                var fileName = GenerateFileName(userId, photoId, attr.Suffix); //sample: /1/2-m.jpg
                var fullPath = baseUrl + fileName;
                p.SetValue(links, fullPath);
            }

            return links;
        }

        private string GenerateFileName(int userId, int photoId, string suffix)
        {
            return String.IsNullOrEmpty(suffix)
                       ? String.Format("/{0}/{1}.jpg", userId, photoId)
                       : String.Format("/{0}/{1}-{2}.jpg", userId, photoId, suffix);
        }

        public int SavePhoto(Photo photo, Stream data, bool showInFeed, IEnumerable<int> categories)
        {
            if (photo == null)
                throw new ArgumentNullException("photo");

            if (photo.UserId == 0)
                throw new ArgumentException("User id cannot be 0");

            if (data == Stream.Null)
                throw new ArgumentNullException("data");

            if (!data.CanSeek)
                throw new ArgumentException("the stream is not seek-able", "data");

            if (photo.AlbumId.HasValue)
            {
                var album = _uow.AlbumRepository.GetById(photo.AlbumId.Value);
                if (album == null)
                    throw new NotFoundException();

                if (album.UserId != photo.UserId)
                    throw new SecurityException();
            }

            if (categories != null)
            {
                foreach (var c in categories)
                {
                    var cat = _staticDataRepository.GetCategories()
                                                   .SingleOrDefault(cate => cate.Id == c);
                    if (cat != null)
                        photo.Categories.Add(new PhotoCategory { CategoryId = (byte)c });
                }
            }

            if (photo.UploadDate == default(DateTime))
                photo.UploadDate = DateTime.UtcNow;

            _uow.PhotoRepository.InsertGraph(photo);
            _uow.SaveChanges();

            if (showInFeed)
            {
                // Checking if the last user feed is photo and it is less than 24 hours.

                var lastFeed = _uow.FeedRepository.GetUserLastFeed(photo.UserId);

                if (lastFeed != null && lastFeed.FeedType == FeedType.Photo && //last feed check
                    lastFeed.Time >= DateTime.UtcNow.AddMinutes(-1) && //last upload date check
                    lastFeed.FeedPhotos.Count < 9 && //maximum number of photos check 
                    lastFeed.FeedPhotos.First().Photo.AlbumId == photo.AlbumId  //same album check
                    )
                {
                    lastFeed.FeedPhotos.Add(new FeedPhoto
                    {
                        PhotoId = photo.Id,
                        TagUser = photo.TagUser
                    });
                }
                else if (lastFeed != null && lastFeed.FeedType == FeedType.Photo && //last feed check
                    lastFeed.Time >= DateTime.UtcNow.AddMinutes(-1) && //last upload date check
                    lastFeed.FeedPhotos.Count >= 9 && //maximum number of photos check 
                    lastFeed.FeedPhotos.First().Photo.AlbumId == photo.AlbumId  //same album check
                    )
                {
                    //do nothing
                }
                else
                {
                    var feed = new Feed
                    {
                        FeedType = FeedType.Photo,
                        Time = photo.UploadDate,
                    };

                    feed.FeedUsers.Add(new FeedUser { UserId = photo.UserId });

                    feed.FeedPhotos.Add(new FeedPhoto
                    {
                        PhotoId = photo.Id,
                        TagUser = photo.TagUser
                    });

                    _uow.FeedRepository.InsertGraph(feed);
                }

                _uow.SaveChanges();
            }

            try
            {
                if (photo.TagUser != null)
                {
                    if (photo.TagUser != "")
                    {
                        var users = photo.TagUser.Split(',');
                        foreach (var user in users)
                        {
                            try
                            {
                                int temp_user = int.Parse(user.Trim());
                                _notificationService.CreateTagPhotoPostNotification(photo.UserId, temp_user, photo.Id);
                            }
                            catch (Exception) { }
                        }
                    }
                }
            }
            catch (Exception) { }

            
            


            ResizeAndSaveImages(data, photo.UserId, photo.Id);

            //var userids = photo.TagUser.Split(',');
            //foreach (var taguserids in userids)
            //{
            //    int toId = int.Parse(taguserids.Trim());
            //    //_notificationService.CreateTagPostNotification(post.FromUserId, toId, post.Id);
            //}



            return photo.Id;
        }

        private void ResizeAndSaveImages(Stream img, int userId, int photoId)
        {
            var properties = typeof(PhotoLinks).GetProperties();
            foreach (var p in properties)
            {
                var attr = p.GetCustomAttributes(typeof(PhotoAttribute), false)
                                            .Cast<PhotoAttribute>()
                                            .FirstOrDefault();

                if (attr == null)
                    continue;

                var size = new Size(attr.Width, attr.Height);
                using (var resizedImg = _imageProcessor.ResizeImage(img, size, attr.Quality))
                {
                    var fileName = GenerateFileName(userId, photoId, attr.Suffix);
                    _storageService.SavePhotoBlob(fileName, resizedImg);
                }
            }
        }

        public void RemovePhoto(int photoId, int currentUserId)
        {
            var photo = _uow.PhotoRepository.GetById(photoId);
            if (photo == null)
                return;
            
            if (photo.UserId != currentUserId)
                throw new SecurityException();

            var picWasProfilePic = photo.User.ProfilePhotoId.HasValue && photo.Id == photo.User.ProfilePhotoId;

            if (picWasProfilePic)
            {
                photo.User.ProfilePhotoId = null;
                _cacheService.RemoveUserPhotoUrl(photo.UserId);
            }

            if (photo.User.AccountType == AccountType.Club)
            {
                if (photo.User.ClubDetail.CoverPhotoId.HasValue && photo.Id == photo.User.ClubDetail.CoverPhotoId.Value)
                    photo.User.ClubDetail.CoverPhotoId = null;
            }

            // removing photo id from the feed photo ids and if it's the last one in the collection, will delete the feed.
            var feed = _uow.FeedRepository.GetPhotoFeed(photoId);
            if (feed != null)
            {
                var feedPhotoToRemove = feed.FeedPhotos.FirstOrDefault(f => f.PhotoId == photoId);
                if (feedPhotoToRemove != null)
                    feed.FeedPhotos.Remove(feedPhotoToRemove);

                if (feed.FeedPhotos.Count == 0)
                    _uow.FeedRepository.Remove(feed);
            }

            photo.Categories.Clear();

            _uow.EventRepository.ResetPhotoId(photoId);
            _uow.PhotoRepository.Remove(photo);
            _uow.SaveChanges();

            var properties = typeof(PhotoLinks).GetProperties();
            foreach (var p in properties)
            {
                var attr = p.GetCustomAttributes(typeof(PhotoAttribute), false)
                            .Cast<PhotoAttribute>()
                            .FirstOrDefault();

                if (attr == null)
                    continue;

                var filename = GenerateFileName(photo.UserId, photo.Id, attr.Suffix);
                _storageService.RemoveBlob(filename);
            }
        }

        public void UpdatePhoto(Photo updatedPhoto, int currentUserId, IEnumerable<int> categories)
        {
            if (updatedPhoto.Id == 0)
                throw new ArgumentException();

            var photo = _uow.PhotoRepository.GetById(updatedPhoto.Id);
            if (photo == null)
                throw new NotFoundException();

            if (photo.UserId != currentUserId)
                throw new SecurityException();

            if (updatedPhoto.AlbumId.HasValue)
            {
                var album = _uow.AlbumRepository.GetById(updatedPhoto.AlbumId.Value);
                if (album == null)
                    throw new NotFoundException();

                if (album.UserId != currentUserId)
                    throw new SecurityException();

                photo.AlbumId = updatedPhoto.AlbumId;
            }

            photo.Categories.Clear();
            if (categories != null)
            {
                foreach (var c in categories)
                {
                    var cat = _staticDataRepository.GetCategories()
                                                   .SingleOrDefault(cate => cate.Id == c);
                    if (cat != null)
                        photo.Categories.Add(new PhotoCategory { CategoryId = (byte)c });
                }
            }

            photo.Description = updatedPhoto.Description;

            _uow.SaveChanges();
        }

        public void CropPhoto(Photo photo, int currentUserId, Rectangle cropArea)
        {
            if (photo.UserId != currentUserId)
                throw new SecurityException();

            var fileName = GenerateFileName(currentUserId, photo.Id, suffix: null); //suffix should be null to get the original one
            using (var file = _storageService.GetBlob(fileName))
            {
                using (var bmp = new Bitmap(file))
                using (var croppedBmp = bmp.Clone(cropArea, bmp.PixelFormat))
                {
                    bmp.Dispose();
                    file.Dispose();

                    using (var ms = new MemoryStream())
                    {
                        croppedBmp.Save(ms, ImageFormat.Jpeg);
                        croppedBmp.Dispose();

                        ResizeAndSaveImages(ms, photo.UserId, photo.Id);
                    }
                }
            }
        }

        public PhotoLinks GetUserDisplayPhoto(int userId)
        {
            var cache = _cacheService.GetUserPhotoUrl(userId);
            if (cache != null)
                return cache;

            var photoId = _uow.UserRepository.GetUserPhotoId(userId);

            var img = new PhotoLinks();

            if (!photoId.HasValue)
            {
                var gender = _uow.UserRepository.GetUserGender(userId);
                img = _defaultImageHelper.GetUserDefaultImage(gender);
            }
            else
            {
                img = GeneratePhotoUrl(userId, photoId.Value);
            }

            _cacheService.AddUserPhotoUrl(userId, img);

            return img;
        }
    }
}