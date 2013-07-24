using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Zazz.Core.Exceptions;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Repositories;
using Zazz.Core.Interfaces.Services;
using Zazz.Core.Models;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.DTOs;
using Zazz.Core.Models.Data.Enums;
using Zazz.Infrastructure;
using Zazz.Infrastructure.Helpers;
using Zazz.Infrastructure.Services;

namespace Zazz.UnitTests.Infrastructure.Services
{
    [TestFixture]
    public class PhotoServiceShould
    {
        private Mock<IUoW> _uow;
        private PhotoService _sut;
        private Mock<ICacheService> _cacheService;
        private Mock<IStringHelper> _stringHelper;
        private Mock<IStaticDataRepository> _staticDataRepo;
        private Mock<IDefaultImageHelper> _defaultImageHelper;
        private Mock<IStorageService> _storageService;
        private Mock<IImageProcessor> _imageProcessor;
        private MockRepository _mockRepo;
        private MemoryStream _photoStream;
        private List<Category> _categories;

        [SetUp]
        public void Init()
        {
            _mockRepo = new MockRepository(MockBehavior.Strict);

            _uow = _mockRepo.Create<IUoW>();
            _cacheService = _mockRepo.Create<ICacheService>();
            _stringHelper = _mockRepo.Create<IStringHelper>();
            _staticDataRepo = _mockRepo.Create<IStaticDataRepository>();
            _imageProcessor = _mockRepo.Create<IImageProcessor>();
            _storageService = _mockRepo.Create<IStorageService>();
            _defaultImageHelper = _mockRepo.Create<IDefaultImageHelper>();

            _sut = new PhotoService(_uow.Object, _cacheService.Object, _stringHelper.Object,
                                    _staticDataRepo.Object, _defaultImageHelper.Object, _imageProcessor.Object,
                                    _storageService.Object);

            _photoStream = new MemoryStream(new byte[] { 1, 2, 3, 4 });

            _categories = new List<Category>
                          {
                              new Category
                              {
                                  Id = 1,
                              },
                              new Category
                              {
                                  Id = 2,
                              },
                              new Category
                              {
                                  Id = 3,
                              }
                          };
        }

        [Test]
        public void ReturnCorrectUrl_OnGeneratePhotoUrl()
        {
            //Arrange
            var baseUrl = "http://test.zazzlife.com/picture/user";
            var userId = 22;
            var photoId = 44;

            _storageService.SetupGet(x => x.BasePhotoUrl)
                           .Returns(baseUrl);

            var expectedVSUrl = String.Format("{0}/{1}/{2}-vs.jpg", baseUrl, userId, photoId);
            var expectedSmallUrl = String.Format("{0}/{1}/{2}-s.jpg", baseUrl, userId, photoId);
            var expectedMediumUrl = String.Format("{0}/{1}/{2}-m.jpg", baseUrl, userId, photoId);
            var expectedOriginalUrl = String.Format("{0}/{1}/{2}.jpg", baseUrl, userId, photoId);

            //Act
            var result = _sut.GeneratePhotoUrl(userId, photoId);

            //Assert
            Assert.AreEqual(expectedVSUrl, result.VerySmallLink);
            Assert.AreEqual(expectedSmallUrl, result.SmallLink);
            Assert.AreEqual(expectedMediumUrl, result.MediumLink);
            Assert.AreEqual(expectedOriginalUrl, result.OriginalLink);
            _mockRepo.VerifyAll();
        }

        [Test]
        public void ThrowIfPhotoIsNull_OnSavePhoto()
        {
            //Arrange
            //Act
            Assert.Throws<ArgumentNullException>(
                () => _sut.SavePhoto(null, _photoStream, true, Enumerable.Empty<int>()));

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void ThrowUserIdIs0_OnSavePhoto()
        {
            //Arrange
            var photo = new Photo
            {
                UserId = 0
            };

            //Act
            Assert.Throws<ArgumentException>(
                () => _sut.SavePhoto(photo, _photoStream, true, Enumerable.Empty<int>()));

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void ThrowIfStreamIsEmpty_OnSavePhoto()
        {
            //Arrange
            var photo = new Photo
                        {
                            UserId = 44
                        };
            //Act
            Assert.Throws<ArgumentNullException>(
                () => _sut.SavePhoto(photo, Stream.Null, true, Enumerable.Empty<int>()));

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void NotAddCategoriesIfNull_OnSavePhoto()
        {
            //Arrange
            var photo = new Photo
                        {
                            UserId = 44
                        };
            var resizedImageStream = new MemoryStream(new byte[] { 4, 5, 6 });

            _uow.Setup(x => x.PhotoRepository.InsertGraph(photo));
            _uow.Setup(x => x.SaveChanges());


            _imageProcessor.Setup(x => x.ResizeImage(_photoStream, It.IsAny<Size>(), It.IsAny<long>()))
                           .Returns(resizedImageStream);

            _storageService.Setup(x => x.SavePhotoBlob(It.IsAny<string>(), resizedImageStream));

            //Act
            _sut.SavePhoto(photo, _photoStream, false, null);

            //Assert
            CollectionAssert.IsEmpty(photo.Categories);
            _mockRepo.VerifyAll();
        }

        [Test]
        public void AddCategories_OnSavePhoto()
        {
            //Arrange
            var photo = new Photo
            {
                UserId = 44
            };
            var resizedImageStream = new MemoryStream(new byte[] { 4, 5, 6 });

            var categories = new[] { 1, 2 };

            _staticDataRepo.Setup(x => x.GetCategories())
                           .Returns(_categories);

            _uow.Setup(x => x.PhotoRepository.InsertGraph(photo));
            _uow.Setup(x => x.SaveChanges());


            _imageProcessor.Setup(x => x.ResizeImage(_photoStream, It.IsAny<Size>(), It.IsAny<long>()))
                           .Returns(resizedImageStream);

            _storageService.Setup(x => x.SavePhotoBlob(It.IsAny<string>(), resizedImageStream));

            //Act
            _sut.SavePhoto(photo, _photoStream, false, categories);

            //Assert
            Assert.AreEqual(categories.Length, photo.Categories.Count);
            _mockRepo.VerifyAll();
        }

        [Test]
        public void IgnoreInvalidCategories_OnSavePhoto()
        {
            //Arrange
            var photo = new Photo
            {
                UserId = 44
            };
            var resizedImageStream = new MemoryStream(new byte[] { 4, 5, 6 });

            var categories = new[] { 1, 2, 98, 34 };

            _staticDataRepo.Setup(x => x.GetCategories())
                           .Returns(_categories);

            _uow.Setup(x => x.PhotoRepository.InsertGraph(photo));
            _uow.Setup(x => x.SaveChanges());


            _imageProcessor.Setup(x => x.ResizeImage(_photoStream, It.IsAny<Size>(), It.IsAny<long>()))
                           .Returns(resizedImageStream);

            _storageService.Setup(x => x.SavePhotoBlob(It.IsAny<string>(), resizedImageStream));

            //Act
            _sut.SavePhoto(photo, _photoStream, false, categories);

            //Assert
            Assert.AreEqual(2, photo.Categories.Count);
            _mockRepo.VerifyAll();
        }

        [Test]
        public void CreateANewFeedIfLastFeedIsNull_OnSavePhoto()
        {
            //Arrange
            var photo = new Photo
            {
                UserId = 44
            };
            
            var resizedImageStream = new MemoryStream(new byte[] { 4, 5, 6 });

            _uow.Setup(x => x.PhotoRepository.InsertGraph(photo));
            _uow.Setup(x => x.SaveChanges());

            _uow.Setup(x => x.FeedRepository.GetUserLastFeed(photo.UserId))
                .Returns(() => null);

            _uow.Setup(x => x.FeedRepository.InsertGraph(It.Is<Feed>(f =>
                                                                     f.FeedType == FeedType.Photo &&
                                                                     f.FeedUsers.Any(u => u.UserId == photo.UserId))));

            _imageProcessor.Setup(x => x.ResizeImage(_photoStream, It.IsAny<Size>(), It.IsAny<long>()))
                           .Returns(resizedImageStream);

            _storageService.Setup(x => x.SavePhotoBlob(It.IsAny<string>(), resizedImageStream));

            //Act
            _sut.SavePhoto(photo, _photoStream, true, null);

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void CreateANewFeedIfLastFeedWasPost_OnSavePhoto()
        {
            //Arrange
            var photo = new Photo
            {
                UserId = 44
            };

            var lastFeed = new Feed
                           {
                               Id = 444,
                               FeedType = FeedType.Post,
                               Time = DateTime.UtcNow.AddHours(-1)
                           };

            var resizedImageStream = new MemoryStream(new byte[] { 4, 5, 6 });

            _uow.Setup(x => x.PhotoRepository.InsertGraph(photo));
            _uow.Setup(x => x.SaveChanges());

            _uow.Setup(x => x.FeedRepository.GetUserLastFeed(photo.UserId))
                .Returns(lastFeed);

            _uow.Setup(x => x.FeedRepository.InsertGraph(It.Is<Feed>(f =>
                                                                     f.FeedType == FeedType.Photo &&
                                                                     f.FeedUsers.Any(u => u.UserId == photo.UserId))));

            _imageProcessor.Setup(x => x.ResizeImage(_photoStream, It.IsAny<Size>(), It.IsAny<long>()))
                           .Returns(resizedImageStream);

            _storageService.Setup(x => x.SavePhotoBlob(It.IsAny<string>(), resizedImageStream));

            //Act
            _sut.SavePhoto(photo, _photoStream, true, null);

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void CreateANewFeedIfLastFeedWasEvent_OnSavePhoto()
        {
            //Arrange
            var photo = new Photo
            {
                UserId = 44
            };

            var lastFeed = new Feed
            {
                Id = 444,
                FeedType = FeedType.Event,
                Time = DateTime.UtcNow.AddHours(-1)
            };

            var resizedImageStream = new MemoryStream(new byte[] { 4, 5, 6 });

            _uow.Setup(x => x.PhotoRepository.InsertGraph(photo));
            _uow.Setup(x => x.SaveChanges());

            _uow.Setup(x => x.FeedRepository.GetUserLastFeed(photo.UserId))
                .Returns(lastFeed);

            _uow.Setup(x => x.FeedRepository.InsertGraph(It.Is<Feed>(f =>
                                                                     f.FeedType == FeedType.Photo &&
                                                                     f.FeedUsers.Any(u => u.UserId == photo.UserId))));

            _imageProcessor.Setup(x => x.ResizeImage(_photoStream, It.IsAny<Size>(), It.IsAny<long>()))
                           .Returns(resizedImageStream);

            _storageService.Setup(x => x.SavePhotoBlob(It.IsAny<string>(), resizedImageStream));

            //Act
            _sut.SavePhoto(photo, _photoStream, true, null);

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void CreateANewFeedIfLastFeedWasPhotoAndOverADayAgo_OnSavePhoto()
        {
            //Arrange
            var photo = new Photo
            {
                UserId = 44
            };

            var lastFeed = new Feed
            {
                Id = 444,
                FeedType = FeedType.Photo,
                Time = DateTime.UtcNow.AddDays(-1)
            };

            var resizedImageStream = new MemoryStream(new byte[] { 4, 5, 6 });

            _uow.Setup(x => x.PhotoRepository.InsertGraph(photo));
            _uow.Setup(x => x.SaveChanges());

            _uow.Setup(x => x.FeedRepository.GetUserLastFeed(photo.UserId))
                .Returns(lastFeed);

            _uow.Setup(x => x.FeedRepository.InsertGraph(It.Is<Feed>(f =>
                                                                     f.FeedType == FeedType.Photo &&
                                                                     f.FeedUsers.Any(u => u.UserId == photo.UserId))));

            _imageProcessor.Setup(x => x.ResizeImage(_photoStream, It.IsAny<Size>(), It.IsAny<long>()))
                           .Returns(resizedImageStream);

            _storageService.Setup(x => x.SavePhotoBlob(It.IsAny<string>(), resizedImageStream));

            //Act
            _sut.SavePhoto(photo, _photoStream, true, null);

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void CreateANewFeedIfLastFeedWasPhotoAndItHas9Photos_OnSavePhoto()
        {
            //Arrange
            var photo = new Photo
            {
                UserId = 44
            };

            var lastFeed = new Feed
            {
                Id = 444,
                FeedType = FeedType.Photo,
                Time = DateTime.UtcNow.AddMinutes(-1),
            };

            for (int i = 0; i < 9; i++)
                lastFeed.FeedPhotos.Add(new FeedPhoto());

            var resizedImageStream = new MemoryStream(new byte[] { 4, 5, 6 });

            _uow.Setup(x => x.PhotoRepository.InsertGraph(photo));
            _uow.Setup(x => x.SaveChanges());

            _uow.Setup(x => x.FeedRepository.GetUserLastFeed(photo.UserId))
                .Returns(lastFeed);

            _uow.Setup(x => x.FeedRepository.InsertGraph(It.Is<Feed>(f =>
                                                                     f.FeedType == FeedType.Photo &&
                                                                     f.FeedUsers.Any(u => u.UserId == photo.UserId))));

            _imageProcessor.Setup(x => x.ResizeImage(_photoStream, It.IsAny<Size>(), It.IsAny<long>()))
                           .Returns(resizedImageStream);

            _storageService.Setup(x => x.SavePhotoBlob(It.IsAny<string>(), resizedImageStream));

            //Act
            _sut.SavePhoto(photo, _photoStream, true, null);

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void CreateANewFeedIfLastFeedWasPhotoAndItWasOnADifferentAlbum_OnSavePhoto()
        {
            //Arrange
            var photo = new Photo
            {
                UserId = 44
            };

            var lastFeed = new Feed
            {
                Id = 444,
                FeedType = FeedType.Photo,
                Time = DateTime.UtcNow.AddMinutes(-1),
            };

            lastFeed.FeedPhotos.Add(new FeedPhoto
                                    {
                                        Photo = new Photo
                                                {
                                                    AlbumId = 432
                                                }
                                    });


            var resizedImageStream = new MemoryStream(new byte[] { 4, 5, 6 });

            _uow.Setup(x => x.PhotoRepository.InsertGraph(photo));
            _uow.Setup(x => x.SaveChanges());

            _uow.Setup(x => x.FeedRepository.GetUserLastFeed(photo.UserId))
                .Returns(lastFeed);

            _uow.Setup(x => x.FeedRepository.InsertGraph(It.Is<Feed>(f =>
                                                                     f.FeedType == FeedType.Photo &&
                                                                     f.FeedUsers.Any(u => u.UserId == photo.UserId))));

            _imageProcessor.Setup(x => x.ResizeImage(_photoStream, It.IsAny<Size>(), It.IsAny<long>()))
                           .Returns(resizedImageStream);

            _storageService.Setup(x => x.SavePhotoBlob(It.IsAny<string>(), resizedImageStream));

            //Act
            _sut.SavePhoto(photo, _photoStream, true, null);

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void CreateANewFeedIfLastFeedWasPhotoAndItWasOnADifferentAlbum2_OnSavePhoto()
        {
            //Arrange
            var photo = new Photo
            {
                UserId = 44,
                AlbumId = 2
            };

            var lastFeed = new Feed
            {
                Id = 444,
                FeedType = FeedType.Photo,
                Time = DateTime.UtcNow.AddMinutes(-1),
            };

            lastFeed.FeedPhotos.Add(new FeedPhoto
            {
                Photo = new Photo
                {
                    AlbumId = 432
                }
            });

            var resizedImageStream = new MemoryStream(new byte[] { 4, 5, 6 });

            _uow.Setup(x => x.PhotoRepository.InsertGraph(photo));
            _uow.Setup(x => x.SaveChanges());

            _uow.Setup(x => x.FeedRepository.GetUserLastFeed(photo.UserId))
                .Returns(lastFeed);

            _uow.Setup(x => x.FeedRepository.InsertGraph(It.Is<Feed>(f =>
                                                                     f.FeedType == FeedType.Photo &&
                                                                     f.FeedUsers.Any(u => u.UserId == photo.UserId))));

            _imageProcessor.Setup(x => x.ResizeImage(_photoStream, It.IsAny<Size>(), It.IsAny<long>()))
                           .Returns(resizedImageStream);

            _storageService.Setup(x => x.SavePhotoBlob(It.IsAny<string>(), resizedImageStream));

            //Act
            _sut.SavePhoto(photo, _photoStream, true, null);

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void AddPhotoToCurrentFeedWhenTheLastOneIsPhotoAndItHasLessThan0PhotosAndIsInASameAlbumAndWasCreatedLessThan24HoursAgo_OnSavePhoto()
        {
            //Arrange
            var photo = new Photo
            {
                UserId = 44,
            };

            var lastFeed = new Feed
            {
                Id = 444,
                FeedType = FeedType.Photo,
                Time = DateTime.UtcNow.AddHours(-23),
            };

            for (int i = 0; i < 8; i++)
                lastFeed.FeedPhotos.Add(new FeedPhoto
                                        {
                                            Photo = new Photo()
                                        });

            var resizedImageStream = new MemoryStream(new byte[] { 4, 5, 6 });

            _uow.Setup(x => x.PhotoRepository.InsertGraph(photo));
            _uow.Setup(x => x.SaveChanges());

            _uow.Setup(x => x.FeedRepository.GetUserLastFeed(photo.UserId))
                .Returns(lastFeed);

            _imageProcessor.Setup(x => x.ResizeImage(_photoStream, It.IsAny<Size>(), It.IsAny<long>()))
                           .Returns(resizedImageStream);

            _storageService.Setup(x => x.SavePhotoBlob(It.IsAny<string>(), resizedImageStream));

            //Act
            _sut.SavePhoto(photo, _photoStream, true, null);

            //Assert
            Assert.AreEqual(9, lastFeed.FeedPhotos.Count);
            _mockRepo.VerifyAll();
        }

        [Test]
        public void NotDoAnythingIfPhotoNotExists_OnRemovePhoto()
        {
            //Arrange
            var photo = new Photo
            {
                Id = 3232,
                UserId = 44
            };

            _uow.Setup(x => x.PhotoRepository.GetById(photo.Id))
                .Returns(() => null);

            //Act
            _sut.RemovePhoto(photo.Id, photo.UserId);

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void ThrowIfUserIsNotFromCurrentUser_OnRemovePhoto()
        {
            //Arrange
            var photo = new Photo
            {
                Id = 3232,
                UserId = 44
            };

            _uow.Setup(x => x.PhotoRepository.GetById(photo.Id))
                .Returns(photo);

            //Act
            Assert.Throws<SecurityException>(() => _sut.RemovePhoto(photo.Id, 233));

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void SetProfilePhotoIdNullAndResetCacheIfPhotoIsTheProfilePhoto_OnRemovePhoto()
        {
            //Arrange
            var photo = new Photo
            {
                Id = 3232,
                UserId = 44,
                User = new User
                       {
                           AccountType = AccountType.User,
                           ProfilePhotoId = 3232
                       }
            };

            _uow.Setup(x => x.PhotoRepository.GetById(photo.Id))
                .Returns(photo);

            _cacheService.Setup(x => x.RemoveUserPhotoUrl(photo.UserId));

            _uow.Setup(x => x.FeedRepository.GetPhotoFeed(photo.Id))
                .Returns(() => null);

            _uow.Setup(x => x.EventRepository.ResetPhotoId(photo.Id));
            _uow.Setup(x => x.PhotoRepository.Remove(photo));
            _uow.Setup(x => x.SaveChanges());

            _storageService.Setup(x => x.RemoveBlob(It.IsAny<string>()));

            //Act
            _sut.RemovePhoto(photo.Id, photo.UserId);

            //Assert
            Assert.IsNull(photo.User.ProfilePhotoId);
            _mockRepo.VerifyAll();
        }

        [Test]
        public void SetCoverPhotoIdNullIfPhotoIsTheCoverPhoto_OnRemovePhoto()
        {
            //Arrange
            var photo = new Photo
            {
                Id = 3232,
                UserId = 44,
                User = new User
                {
                    AccountType = AccountType.Club,
                    ProfilePhotoId = 3231,
                    ClubDetail = new ClubDetail
                                 {
                                     CoverPhotoId = 3232
                                 }
                },
            };

            _uow.Setup(x => x.PhotoRepository.GetById(photo.Id))
                .Returns(photo);

            _uow.Setup(x => x.FeedRepository.GetPhotoFeed(photo.Id))
                .Returns(() => null);

            _uow.Setup(x => x.EventRepository.ResetPhotoId(photo.Id));
            _uow.Setup(x => x.PhotoRepository.Remove(photo));
            _uow.Setup(x => x.SaveChanges());

            _storageService.Setup(x => x.RemoveBlob(It.IsAny<string>()));

            //Act
            _sut.RemovePhoto(photo.Id, photo.UserId);

            //Assert
            Assert.IsNull(photo.User.ClubDetail.CoverPhotoId);
            _mockRepo.VerifyAll();
        }

        [Test]
        public void RemoveFeedIfItsTheLastPhoto_OnRemovePhoto()
        {
            //Arrange
            var photo = new Photo
            {
                Id = 3232,
                UserId = 44,
                User = new User
                {
                    AccountType = AccountType.User,
                }
            };

            var feed = new Feed
                       {
                           FeedPhotos = new List<FeedPhoto>
                                        {
                                            new FeedPhoto
                                            {
                                                PhotoId = photo.Id
                                            }
                                        }
                       };

            _uow.Setup(x => x.PhotoRepository.GetById(photo.Id))
                .Returns(photo);

            _uow.Setup(x => x.FeedRepository.GetPhotoFeed(photo.Id))
                .Returns(feed);

            _uow.Setup(x => x.FeedRepository.Remove(feed));

            _uow.Setup(x => x.EventRepository.ResetPhotoId(photo.Id));
            _uow.Setup(x => x.PhotoRepository.Remove(photo));
            _uow.Setup(x => x.SaveChanges());

            _storageService.Setup(x => x.RemoveBlob(It.IsAny<string>()));

            //Act
            _sut.RemovePhoto(photo.Id, photo.UserId);

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void NotRemoveFeedIfItsNotTheLastPhoto_OnRemovePhoto()
        {
            //Arrange
            var photo = new Photo
            {
                Id = 3232,
                UserId = 44,
                User = new User
                {
                    AccountType = AccountType.User,
                }
            };

            var feed = new Feed
            {
                FeedPhotos = new List<FeedPhoto>
                                        {
                                            new FeedPhoto
                                            {
                                                PhotoId = photo.Id
                                            },
                                            new FeedPhoto
                                            {
                                                PhotoId = photo.Id + 1
                                            }
                                        }
            };

            _uow.Setup(x => x.PhotoRepository.GetById(photo.Id))
                .Returns(photo);

            _uow.Setup(x => x.FeedRepository.GetPhotoFeed(photo.Id))
                .Returns(feed);

            _uow.Setup(x => x.EventRepository.ResetPhotoId(photo.Id));
            _uow.Setup(x => x.PhotoRepository.Remove(photo));
            _uow.Setup(x => x.SaveChanges());

            _storageService.Setup(x => x.RemoveBlob(It.IsAny<string>()));

            //Act
            _sut.RemovePhoto(photo.Id, photo.UserId);

            //Assert
            Assert.AreEqual(1, feed.FeedPhotos.Count);
            _mockRepo.VerifyAll();
        }

        [Test]
        public void ThrowIfUserPhotoIdIs0_OnUpdatePhoto()
        {
            //Arrange
            var photo = new Photo
            {
                Id = 0,
                UserId = 124,
                AlbumId = 123
            };

            //Act
            Assert.Throws<ArgumentException>(() => _sut.UpdatePhoto(photo, 1234, null));

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void ThrowIfPhotoDoesntExists_OnUpdatePhoto()
        {
            //Arrange
            var photoId = 124;
            var userId = 12;
            var albumId = 444;

            var photo = new Photo
            {
                Id = photoId,
                AlbumId = albumId,
                UserId = 890
            };

            _uow.Setup(x => x.PhotoRepository.GetById(photo.Id))
                .Returns(() => null);

            //Act
            Assert.Throws<NotFoundException>(() => _sut.UpdatePhoto(photo, userId, null));

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void ThrowIfUserIdIsNotSameAsOwnerId_OnUpdatePhoto()
        {
            //Arrange
            var photoId = 124;
            var userId = 12;
            var albumId = 444;

            var photo = new Photo
            {
                Id = photoId,
                AlbumId = albumId,
                UserId = 890
            };

            _uow.Setup(x => x.PhotoRepository.GetById(photo.Id))
                .Returns(photo);

            //Act
            Assert.Throws<SecurityException>(() => _sut.UpdatePhoto(photo, userId, null));

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void Save_OnUpdatePhoto()
        {
            //Arrange
            var photoId = 124;
            var userId = 12;
            var albumId = 444;

            var photo = new Photo
            {
                Id = photoId,
                AlbumId = albumId,
                UserId = userId
            };

            _uow.Setup(x => x.PhotoRepository.GetById(photo.Id))
                .Returns(photo);

            _uow.Setup(x => x.SaveChanges());

            //Act
            _sut.UpdatePhoto(photo, userId, null);

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void RemoveCategoriesIfNull_OnUpdatePhoto()
        {
            //Arrange
            var photoId = 124;
            var userId = 12;
            var albumId = 444;

            var photo = new Photo
                        {
                            Id = photoId,
                            AlbumId = albumId,
                            UserId = userId,
                            Categories = new List<PhotoCategory>
                                         {
                                             new PhotoCategory(),
                                             new PhotoCategory(),
                                             new PhotoCategory(),
                                             new PhotoCategory(),
                                             new PhotoCategory()
                                         }
                        };

            _uow.Setup(x => x.PhotoRepository.GetById(photo.Id))
                .Returns(photo);

            _uow.Setup(x => x.SaveChanges());

            //Act
            _sut.UpdatePhoto(photo, userId, null);

            //Assert
            Assert.AreEqual(0, photo.Categories.Count);
            _mockRepo.VerifyAll();
        }

        [Test]
        public void UpdateCategories_OnUpdatePhoto()
        {
            //Arrange
            var photoId = 124;
            var userId = 12;
            var albumId = 444;

            var photo = new Photo
            {
                Id = photoId,
                AlbumId = albumId,
                UserId = userId,
                Categories = new List<PhotoCategory>
                             {
                                 new PhotoCategory(),
                                 new PhotoCategory(),
                                 new PhotoCategory(),
                                 new PhotoCategory(),
                                 new PhotoCategory()
                             }
            };

            _uow.Setup(x => x.PhotoRepository.GetById(photo.Id))
                .Returns(photo);

            _staticDataRepo.Setup(x => x.GetCategories())
                           .Returns(_categories);

            _uow.Setup(x => x.SaveChanges());

            //Act
            _sut.UpdatePhoto(photo, userId, new[] {1, 2});

            //Assert
            Assert.AreEqual(2, photo.Categories.Count);
            _mockRepo.VerifyAll();
        }

        [Test]
        public void IgnoreInvalidCategoryIds_OnUpdatePhoto()
        {
            //Arrange
            var photoId = 124;
            var userId = 12;
            var albumId = 444;

            var photo = new Photo
            {
                Id = photoId,
                AlbumId = albumId,
                UserId = userId,
                Categories = new List<PhotoCategory>
                             {
                                 new PhotoCategory(),
                                 new PhotoCategory(),
                                 new PhotoCategory(),
                                 new PhotoCategory(),
                                 new PhotoCategory()
                             }
            };

            _uow.Setup(x => x.PhotoRepository.GetById(photo.Id))
                .Returns(photo);

            _staticDataRepo.Setup(x => x.GetCategories())
                           .Returns(_categories);

            _uow.Setup(x => x.SaveChanges());

            //Act
            _sut.UpdatePhoto(photo, userId, new[] { 1, 2, 75, 28 });

            //Assert
            Assert.AreEqual(2, photo.Categories.Count);
            _mockRepo.VerifyAll();
        }

        [Test]
        public void ReturnDefaultImage_WhenPhotoIdIsNullAndAddToCacheIfItsNotThere_GetUserImageUrl()
        {
            //Arrange
            var userId = 12;
            var gender = Gender.Male;
            var photoId = 123;

            var photo = new PhotoMinimalDTO
                        {
                            AlbumId = 3,
                            Id = photoId,
                            UserId = userId
                        };

            //var expected = _defaultImageHelper.GetUserDefaultImage(gender);

            _uow.Setup(x => x.UserRepository.GetUserPhotoId(userId))
                .Returns(() => null);
            _uow.Setup(x => x.UserRepository.GetUserGender(userId))
                .Returns(gender);
            _uow.Setup(x => x.PhotoRepository.GetPhotoWithMinimalData(photoId))
                .Returns(() => photo);
            _cacheService.Setup(x => x.GetUserPhotoUrl(userId))
                         .Returns(() => null);
            //_cacheService.Setup(x => x.AddUserPhotoUrl(userId, expected));


            //Act
            var result = _sut.GetUserImageUrl(userId);

            //Assert
            //Assert.AreEqual(expected.OriginalLink, result.OriginalLink);
            //Assert.AreEqual(expected.MediumLink, result.MediumLink);
            //Assert.AreEqual(expected.SmallLink, result.SmallLink);
            //Assert.AreEqual(expected.VerySmallLink, result.VerySmallLink);
            _uow.Verify(x => x.UserRepository.GetUserPhotoId(userId), Times.Once());
            _uow.Verify(x => x.UserRepository.GetUserGender(userId), Times.Once());
            _uow.Verify(x => x.PhotoRepository.GetPhotoWithMinimalData(It.IsAny<int>()), Times.Never());
            _cacheService.Verify(x => x.GetUserPhotoUrl(userId), Times.Once());
            _cacheService.Verify(x => x.AddUserPhotoUrl(userId, It.IsAny<PhotoLinks>()), Times.Once());
        }

        [Test]
        public void ReturnDefaultImage_WhenPhotoIdIsNot0ButPhotoIsNullAndAddToCacheIfItsNotThere_GetUserImageUrl()
        {
            //Arrange
            var userId = 12;
            var gender = Gender.Male;
            var photoId = 123;

            var photo = new PhotoMinimalDTO
            {
                AlbumId = 3,
                Id = photoId,
                UserId = userId
            };

            //            var expected = _defaultImageHelper.GetUserDefaultImage(gender);

            _uow.Setup(x => x.UserRepository.GetUserPhotoId(userId))
                .Returns(photoId);
            _uow.Setup(x => x.UserRepository.GetUserGender(userId))
                .Returns(gender);
            _uow.Setup(x => x.PhotoRepository.GetPhotoWithMinimalData(photoId))
                .Returns(() => null);
            _cacheService.Setup(x => x.GetUserPhotoUrl(userId))
                         .Returns(() => null);
            //          _cacheService.Setup(x => x.AddUserPhotoUrl(userId, expected));

            //Act
            var result = _sut.GetUserImageUrl(userId);

            //Assert
            //Assert.AreEqual(expected.OriginalLink, result.OriginalLink);
            //Assert.AreEqual(expected.MediumLink, result.MediumLink);
            //Assert.AreEqual(expected.SmallLink, result.SmallLink);
            //Assert.AreEqual(expected.VerySmallLink, result.VerySmallLink);
            _uow.Verify(x => x.UserRepository.GetUserPhotoId(userId), Times.Once());
            _uow.Verify(x => x.UserRepository.GetUserGender(userId), Times.Once());
            _uow.Verify(x => x.PhotoRepository.GetPhotoWithMinimalData(photoId), Times.Once());
            _cacheService.Verify(x => x.GetUserPhotoUrl(userId), Times.Once());
            _cacheService.Verify(x => x.AddUserPhotoUrl(userId, It.IsAny<PhotoLinks>()), Times.Once());
        }

        [Test]
        public void ReturnUserImage_WhenPhotoIdIsNot0AndPhotoIsNotNullAndAddToCacheIfItsNotThere_GetUserImageUrl()
        {
            //Arrange
            var userId = 12;
            var gender = Gender.Male;
            var photoId = 123;

            var photo = new PhotoMinimalDTO
            {
                AlbumId = 3,
                Id = photoId,
                UserId = userId
            };

            var expected = _sut.GeneratePhotoUrl(userId, photoId);

            _uow.Setup(x => x.UserRepository.GetUserPhotoId(userId))
                .Returns(photoId);
            _uow.Setup(x => x.UserRepository.GetUserGender(userId))
                .Returns(gender);
            _uow.Setup(x => x.PhotoRepository.GetPhotoWithMinimalData(photoId))
                .Returns(() => photo);

            _cacheService.Setup(x => x.GetUserPhotoUrl(userId))
                         .Returns(() => null);
            _cacheService.Setup(x => x.AddUserPhotoUrl(userId, expected));


            //Act
            var result = _sut.GetUserImageUrl(userId);

            //Assert
            Assert.AreEqual(expected.OriginalLink, result.OriginalLink);
            Assert.AreEqual(expected.MediumLink, result.MediumLink);
            Assert.AreEqual(expected.SmallLink, result.SmallLink);
            Assert.AreEqual(expected.VerySmallLink, result.VerySmallLink);
            _uow.Verify(x => x.UserRepository.GetUserPhotoId(userId), Times.Once());
            _uow.Verify(x => x.UserRepository.GetUserGender(It.IsAny<int>()), Times.Never());
            _uow.Verify(x => x.PhotoRepository.GetPhotoWithMinimalData(photoId), Times.Once());
            _cacheService.Verify(x => x.GetUserPhotoUrl(userId), Times.Once());
            _cacheService.Verify(x => x.AddUserPhotoUrl(userId, It.IsAny<PhotoLinks>()), Times.Once());
        }

        [Test]
        public void ReturnUserImageFromCacheIfExists_GetUserImageUrl()
        {
            //Arrange
            var userId = 12;
            var gender = Gender.Male;
            var photoId = 123;

            var photo = new PhotoMinimalDTO
            {
                AlbumId = 3,
                Id = photoId,
                UserId = userId
            };

            var expected = _sut.GeneratePhotoUrl(userId, photoId);

            _uow.Setup(x => x.UserRepository.GetUserPhotoId(userId))
                .Returns(photoId);
            _uow.Setup(x => x.UserRepository.GetUserGender(userId))
                .Returns(gender);
            _uow.Setup(x => x.PhotoRepository.GetPhotoWithMinimalData(photoId))
                .Returns(() => photo);

            _cacheService.Setup(x => x.GetUserPhotoUrl(userId))
                         .Returns(expected);
            _cacheService.Setup(x => x.AddUserPhotoUrl(userId, expected));


            //Act
            var result = _sut.GetUserImageUrl(userId);

            //Assert
            Assert.AreEqual(expected, result);
            _uow.Verify(x => x.UserRepository.GetUserPhotoId(userId), Times.Never());
            _uow.Verify(x => x.UserRepository.GetUserGender(It.IsAny<int>()), Times.Never());
            _uow.Verify(x => x.PhotoRepository.GetPhotoWithMinimalData(photoId), Times.Never());
            _cacheService.Verify(x => x.GetUserPhotoUrl(userId), Times.Once());
            _cacheService.Verify(x => x.AddUserPhotoUrl(userId, expected), Times.Never());
        }

        [TearDown]
        public void Cleanup()
        {
            _photoStream.Dispose();
        }
    }
}