using System;
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

        public PhotoService(IUoW uow, ICacheService cacheService,
            IStringHelper stringHelper, IStaticDataRepository staticDataRepository,
            IDefaultImageHelper defaultImageHelper, IImageProcessor imageProcessor, IStorageService storageService)
        {
            _uow = uow;
            _cacheService = cacheService;
            _stringHelper = stringHelper;
            _staticDataRepository = staticDataRepository;
            _defaultImageHelper = defaultImageHelper;
            _imageProcessor = imageProcessor;
            _storageService = storageService;
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
            var baseUrl = _storageService.BasePhotoUrl; //sample: http://test.zazzlife.com/picture/user
            
            var links = new PhotoLinks();
            var type = typeof (PhotoLinks);

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

            photo.UploadDate = DateTime.UtcNow;
            _uow.PhotoRepository.InsertGraph(photo);
            _uow.SaveChanges();

            if (showInFeed)
            {
                // Checking if the last user feed is photo and it is less than 24 hours.

                var lastFeed = _uow.FeedRepository.GetUserLastFeed(photo.UserId);

                if (lastFeed != null && lastFeed.FeedType == FeedType.Photo && //last feed check
                    lastFeed.Time >= DateTime.UtcNow.AddDays(-1) && //last upload date check
                    lastFeed.FeedPhotos.Count < 9 && //maximum number of photos check 
                    lastFeed.FeedPhotos.First().Photo.AlbumId == photo.AlbumId  //same album check
                    )
                {
                    lastFeed.FeedPhotos.Add(new FeedPhoto
                    {
                        PhotoId = photo.Id
                    });
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
                        PhotoId = photo.Id
                    });

                    _uow.FeedRepository.InsertGraph(feed);
                }

                _uow.SaveChanges();
            }

            ResizeAndSaveImages(data, photo.UserId, photo.Id);

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
                var resizedImg = _imageProcessor.ResizeImage(img, size, attr.Quality);
                var fileName = GenerateFileName(userId, photoId, attr.Suffix);
                _storageService.SavePhotoBlob(fileName, resizedImg);
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
            var feedId = _uow.FeedPhotoRepository.RemoveByPhotoIdAndReturnFeedId(photoId);
            _uow.SaveChanges();

            if (feedId != 0)
            {
                var remainingPhotosCount = _uow.FeedPhotoRepository.GetCount(feedId);
                if (remainingPhotosCount == 0)
                {
                    _uow.FeedRepository.Remove(feedId);
                }
            }

            _uow.EventRepository.ResetPhotoId(photoId);
            _uow.PhotoRepository.Remove(photo);
            _uow.SaveChanges();
            

            var properties = typeof(PhotoLinks).GetProperties();
            foreach (var p in properties)
            {
                var attr = p.GetCustomAttributes(typeof (PhotoAttribute), false)
                            .Cast<PhotoAttribute>()
                            .FirstOrDefault();

                if (attr == null)
                    continue;

                var filename = GenerateFileName(photo.UserId, photo.Id, attr.Suffix);
                _storageService.RemoveBlob(filename);
            }
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

        public void UpdatePhoto(Photo updatedPhoto, int currentUserId)
        {
            if (updatedPhoto.Id == 0)
                throw new ArgumentException();

            var photo = _uow.PhotoRepository.GetById(updatedPhoto.Id);
            if (photo == null)
                throw new NotFoundException();

            if (photo.UserId != currentUserId)
                throw new SecurityException();

            photo.Categories.Clear();

            if (!String.IsNullOrEmpty(updatedPhoto.Description))
            {
                var extractedTags = _stringHelper.ExtractTags(updatedPhoto.Description);
                foreach (var t in extractedTags.Distinct(StringComparer.InvariantCultureIgnoreCase))
                {
                    var tag = _staticDataRepository.GetCategoryIfExists(t.Replace("#", ""));
                    if (tag != null)
                    {
                        photo.Categories.Add(new PhotoCategory
                                       {
                                           CategoryId = tag.Id
                                       });
                    }
                }
            }

            photo.Description = updatedPhoto.Description;
            photo.AlbumId = updatedPhoto.AlbumId;

            _uow.SaveChanges();
        }

        public PhotoLinks GetUserImageUrl(int userId)
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
                var photo = _uow.PhotoRepository.GetPhotoWithMinimalData(photoId.Value);
                if (photo == null)
                {
                    var gender = _uow.UserRepository.GetUserGender(userId);
                    img = _defaultImageHelper.GetUserDefaultImage(gender);
                }
                else
                {
                    img = GeneratePhotoUrl(photo.UserId, photo.Id);
                }
            }

            _cacheService.AddUserPhotoUrl(userId, img);

            return img;
        }
    }
}