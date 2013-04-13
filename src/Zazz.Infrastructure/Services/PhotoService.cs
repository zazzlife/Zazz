﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using Zazz.Core.Interfaces;
using Zazz.Core.Models;
using Zazz.Core.Models.Data;

namespace Zazz.Infrastructure.Services
{
    public class PhotoService : IPhotoService
    {
        private readonly IUoW _uow;
        private readonly IFileService _fileService;
        private readonly string _rootPath;
        private readonly ICacheService _cacheService;

        private const string VERY_SMALL_IMAGE_SUFFIX = "vs";
        private const string SMALL_IMAGE_SUFFIX = "s";
        private const string MEDIUM_IMAGE_SUFFIX = "m";

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

        public Photo GetPhoto(int id)
        {
            return _uow.PhotoRepository.GetById(id);
        }

        public PhotoLinks GeneratePhotoUrl(int userId, int photoId)
        {
            return new PhotoLinks
                   {
                       VerySmallLink = String.Format("/picture/user/{0}/{1}-{2}.jpg",
                                                     userId, photoId, VERY_SMALL_IMAGE_SUFFIX),
                       SmallLink = String.Format("/picture/user/{0}/{1}-{2}.jpg",
                                                 userId, photoId, SMALL_IMAGE_SUFFIX),
                       MediumLink = String.Format("/picture/user/{0}/{1}-{2}.jpg",
                                                  userId, photoId, MEDIUM_IMAGE_SUFFIX),
                       OriginalLink = String.Format("/picture/user/{0}/{1}.jpg", userId, photoId)
                   };
        }

        public PhotoLinks GeneratePhotoFilePath(int userId, int photoId)
        {
            return new PhotoLinks
                   {
                       VerySmallLink = String.Format(@"{0}\picture\user\{1}\{2}-{3}.jpg",
                                                     _rootPath, userId, photoId, VERY_SMALL_IMAGE_SUFFIX),
                       SmallLink = String.Format(@"{0}\picture\user\{1}\{2}-{3}.jpg",
                                                 _rootPath, userId, photoId, SMALL_IMAGE_SUFFIX),
                       MediumLink = String.Format(@"{0}\picture\user\{1}\{2}-{3}.jpg",
                                                  _rootPath, userId, photoId, MEDIUM_IMAGE_SUFFIX),
                       OriginalLink = String.Format(@"{0}\picture\user\{1}\{2}.jpg", _rootPath, userId, photoId)
                   };
        }

        public string GetPhotoDescription(int photoId)
        {
            return _uow.PhotoRepository.GetDescription(photoId);
        }

        public int SavePhoto(Photo photo, Stream data, bool showInFeed)
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

            if (data != Stream.Null)
                ResizeAndSaveImages(Image.FromStream(data), photo.UserId, photo.Id);

            return photo.Id;
        }

        private void ResizeAndSaveImages(Image sourceImage, int userId, int photoId)
        {
            using (sourceImage)
            {
                var pathes = GeneratePhotoFilePath(userId, photoId);
                var images = new Dictionary<string, Size>
                         {
                             {pathes.VerySmallLink, new Size(55, 55)},
                             {pathes.SmallLink, new Size(175, 175)},
                             {pathes.MediumLink, new Size(500, 500)},
                             {pathes.OriginalLink, new Size(1600, 1600)}
                         };

                foreach (var image in images)
                {
                    if (sourceImage.Width > image.Value.Width || sourceImage.Height > image.Value.Height)
                    {
                        // Figure out the ratio
                        double ratioX = (double)image.Value.Width / (double)sourceImage.Width;
                        double ratioY = (double)image.Value.Height / (double)sourceImage.Height;

                        // use whichever multiplier is smaller
                        var ratio = ratioX < ratioY ? ratioX : ratioY;

                        // now we can get the new height and width
                        var newHeight = Convert.ToInt32(sourceImage.Height * ratio);
                        var newWidth = Convert.ToInt32(sourceImage.Width * ratio);

                        using (var b = new Bitmap(newWidth, newHeight))
                        using (var g = Graphics.FromImage(b))
                        {
                            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            g.SmoothingMode = SmoothingMode.HighQuality;
                            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                            g.CompositingQuality = CompositingQuality.HighQuality;

                            ImageCodecInfo jgpEncoder = GetEncoder(ImageFormat.Jpeg);
                            EncoderParameters encoderParams = new EncoderParameters(1);
                            EncoderParameter encoderParam = new EncoderParameter(Encoder.Quality, 60L);

                            encoderParams.Param[0] = encoderParam;

                            g.DrawImage(sourceImage, 0, 0, newWidth, newHeight);
                            b.Save(image.Key, jgpEncoder, encoderParams);
                        }
                    }
                    else
                    {
                        // resize is not needed because the image is already smaller
                        sourceImage.Save(image.Key);
                    }
                }
            }
        }

        private ImageCodecInfo GetEncoder(ImageFormat format)
        {

            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

        public void CropPhoto(Photo photo, int currentUserId, Rectangle cropArea)
        {
            if (photo.UserId != currentUserId)
                throw new SecurityException();

            var imgPath = GeneratePhotoFilePath(photo.UserId, photo.Id);

            using (var bmp = new Bitmap(imgPath.OriginalLink))
            using (var croppedBmp = bmp.Clone(cropArea, bmp.PixelFormat))
            {
                bmp.Dispose();
                ResizeAndSaveImages(croppedBmp, photo.UserId, photo.Id);
            }
        }

        public void RemovePhoto(int photoId, int currentUserId)
        {
            var photo = _uow.PhotoRepository.GetById(photoId);
            if (photo.UserId != currentUserId)
                throw new SecurityException();

            var userDetail = photo.User.UserDetail;

            if (photo.Id == userDetail.ProfilePhotoId)
                photo.User.UserDetail.ProfilePhotoId = 0;

            if (photo.Id == userDetail.CoverPhotoId)
                photo.User.UserDetail.CoverPhotoId = 0;

            // removing photo id from the feed photo ids and if it's the last one in the collection, will delete the feed.
            var feedId = _uow.FeedPhotoIdRepository.RemoveByPhotoIdAndReturnFeedId(photoId);
            _uow.SaveChanges();

            var remainingPhotosCount = _uow.FeedPhotoIdRepository.GetCount(feedId);
            if (remainingPhotosCount == 0)
            {
                _uow.FeedRepository.Remove(feedId);
            }

            _uow.EventRepository.ResetPhotoId(photoId);
            _uow.UserRepository.ResetPhotoId(photoId);
            _uow.CommentRepository.RemovePhotoComments(photoId);

            _uow.PhotoRepository.Remove((Photo)photo);
            _uow.SaveChanges();

            var paths = GeneratePhotoFilePath(photo.UserId, photo.Id);
            _fileService.RemoveFile(paths.VerySmallLink);
            _fileService.RemoveFile(paths.SmallLink);
            _fileService.RemoveFile(paths.MediumLink);
            _fileService.RemoveFile(paths.OriginalLink);
        }

        public void UpdatePhoto(Photo photo, int currentUserId)
        {
            if (photo.Id == 0)
                throw new ArgumentException();

            var ownerId = _uow.PhotoRepository.GetOwnerId(photo.Id);
            if (ownerId != currentUserId)
                throw new SecurityException();

            _uow.PhotoRepository.InsertOrUpdate(photo);
            _uow.SaveChanges();
        }

        public PhotoLinks GetUserImageUrl(int userId)
        {
            var cache = _cacheService.GetUserPhotoUrl(userId);
            if (cache != null)
                return cache;

            var photoId = _uow.UserRepository.GetUserPhotoId(userId);

            var img = new PhotoLinks();

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
                    img = new PhotoLinks
                          {
                              OriginalLink = photo.FacebookPicUrl,
                              MediumLink = photo.FacebookPicUrl,
                              SmallLink = photo.FacebookPicUrl,
                              VerySmallLink = photo.FacebookPicUrl
                          };
                }
                else
                {
                    img = GeneratePhotoUrl(photo.UserId, photo.Id);
                }
            }

            _cacheService.AddUserPhotoUrl(userId, img);

            return img;
        }

        public void Dispose()
        {
            _uow.Dispose();
        }
    }
}