using System;
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
using Zazz.Core.Models.Data.Enums;
using Zazz.Infrastructure.Helpers;

namespace Zazz.Infrastructure.Services
{
    public class PhotoService : IPhotoService
    {
        private readonly IUoW _uow;
        private readonly IFileService _fileService;
        private readonly string _rootPath;
        private readonly ICacheService _cacheService;
        private readonly INotificationService _notificationService;
        private readonly ICommentService _commentService;

        private const string VERY_SMALL_IMAGE_SUFFIX = "vs";
        private const string SMALL_IMAGE_SUFFIX = "s";
        private const string MEDIUM_IMAGE_SUFFIX = "m";

        public PhotoService(IUoW uow, IFileService fileService,ICacheService cacheService,
            INotificationService notificationService, ICommentService commentService, string rootPath)
        {
            _uow = uow;
            _fileService = fileService;
            _rootPath = rootPath;
            _cacheService = cacheService;
            _notificationService = notificationService;
            _commentService = commentService;
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
                    lastFeed.FeedPhotos.Add(new FeedPhoto
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

            if (data != Stream.Null)
                ResizeAndSaveImages(Image.FromStream(data), photo.UserId, photo.Id);

            return photo.Id;
        }

        private void ResizeAndSaveImages(Image sourceImage, int userId, int photoId)
        {
            using (sourceImage)
            {
                var pathes = GeneratePhotoFilePath(userId, photoId);
                var images = new[]
                             {
                                 new
                                 {
                                     path = pathes.VerySmallLink,
                                     size = new Size(55, 55),
                                     quality = 95L
                                 },
                                 new
                                 {
                                     path = pathes.SmallLink,
                                     size = new Size(175, 175),
                                     quality = 85L
                                 },
                                 new
                                 {
                                     path = pathes.MediumLink,
                                     size = new Size(500, 500),
                                     quality = 70L
                                 },
                                 new
                                 {
                                     path = pathes.OriginalLink,
                                     size = new Size(1600, 1600),
                                     quality = 60L
                                 }
                             };

                ImageCodecInfo jpg = GetEncoder(ImageFormat.Jpeg);
                EncoderParameters encoderParams = new EncoderParameters(1);

                foreach (var image in images)
                {
                    EncoderParameter encoderParam = new EncoderParameter(Encoder.Quality, image.quality);
                    encoderParams.Param[0] = encoderParam;

                    if (sourceImage.Width > image.size.Width || sourceImage.Height > image.size.Height)
                    {
                        // Figure out the ratio
                        double ratioX = (double)image.size.Width / (double)sourceImage.Width;
                        double ratioY = (double)image.size.Height / (double)sourceImage.Height;

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

                            g.DrawImage(sourceImage, 0, 0, newWidth, newHeight);

                            var path = _fileService.RemoveFileNameFromPath(image.path);
                            _fileService.CreateDirIfNotExists(path); //TODO: instead of checking the path for every image try a better solution.

                            b.Save(image.path, jpg, encoderParams);
                        }
                    }
                    else
                    {
                        // resize is not needed because the image is already smaller
                        sourceImage.Save(image.path, jpg, encoderParams);
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

            _notificationService.RemovePhotoNotifications(photoId);

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
            var picWasProfilePic = _uow.UserRepository.ResetPhotoId(photoId);
            _commentService.RemovePhotoComments(photoId);

            _uow.PhotoRepository.Remove(photo);
            _uow.SaveChanges();

            if (picWasProfilePic)
                _cacheService.RemoveUserPhotoUrl(photo.UserId);

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
    }
}