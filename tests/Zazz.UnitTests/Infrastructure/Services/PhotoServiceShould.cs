using System;
using System.Collections.Generic;
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
        private MemoryStream _photo;

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

            _photo = new MemoryStream(new byte[] {1, 2, 3, 4});
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
                () => _sut.SavePhoto(null, _photo, true, Enumerable.Empty<int>()));

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
        public void ThrowIfTheCurrentUserIsNotTheOwner_OnRemovePhoto()
        {
            //Arrange
            var photoId = 124;
            var userId = 12;
            var photo = new Photo
                            {
                                Id = photoId,
                                AlbumId = 123,
                                UserId = 999
                            };

            _uow.Setup(x => x.PhotoRepository.GetById(photoId))
                .Returns(photo);

            //Act
            try
            {
                _sut.RemovePhoto(photoId, userId);
                Assert.Fail("Expected exception wasn't thrown");
            }
            catch (SecurityException)
            {
            }

            //Assert
            _uow.Verify(x => x.PhotoRepository.GetById(photoId), Times.Once());
            _uow.Verify(x => x.EventRepository.ResetPhotoId(photoId), Times.Never());
            _uow.Verify(x => x.UserRepository.ResetPhotoId(photoId), Times.Never());
        }

        [Test]
        public void SetCoverPhotoIdToNullAndNotResetCacheIfThePhotoIsCoverPhotoAndNotProfilePhotoAndResetEventPhotoId_OnRemovePhoto()
        {
            //Arrange

            var feedId = 12;
            var photoId = 124;
            var userId = 12;
            var albumId = 444;
            var coverPhotoId = photoId;
            var profilePhotoId = 2;

            var photo = new Photo
            {
                Id = photoId,
                AlbumId = albumId,
                UserId = userId,
                User = new User
                {
                    AccountType = AccountType.Club,
                    ProfilePhotoId = profilePhotoId,
                    ClubDetail = new ClubDetail()
                    {
                        CoverPhotoId = coverPhotoId,
                    }
                }
            };

            _uow.Setup(x => x.PhotoRepository.GetById(photoId))
                .Returns(photo);

            _uow.Setup(x => x.PhotoRepository.GetOwnerId(photoId))
                .Returns(userId);
            _uow.Setup(x => x.PhotoRepository.Remove(photoId));
            _uow.Setup(x => x.EventRepository.ResetPhotoId(photoId));
            _uow.Setup(x => x.UserRepository.ResetPhotoId(photoId))
                .Returns(false);
            _uow.Setup(x => x.FeedPhotoRepository.RemoveByPhotoIdAndReturnFeedId(photoId))
                .Returns(feedId);
            _uow.Setup(x => x.FeedPhotoRepository.GetCount(feedId))
                .Returns(1);
            _uow.Setup(x => x.FeedRepository.Remove(feedId));

            //Act
            _sut.RemovePhoto(photoId, userId);

            //Assert
            Assert.AreEqual(null, photo.User.ClubDetail.CoverPhotoId);
            Assert.AreEqual(profilePhotoId, photo.User.ProfilePhotoId);
            _uow.Verify(x => x.PhotoRepository.Remove(photo), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Exactly(2));
            _uow.Verify(x => x.EventRepository.ResetPhotoId(photoId), Times.Once());
            _uow.Verify(x => x.UserRepository.ResetPhotoId(photoId), Times.Once());
            _uow.Verify(x => x.FeedPhotoRepository.RemoveByPhotoIdAndReturnFeedId(photoId), Times.Once());
            _uow.Verify(x => x.FeedPhotoRepository.GetCount(feedId), Times.Once());
            _uow.Verify(x => x.FeedRepository.Remove(feedId), Times.Never());
            _cacheService.Verify(x => x.RemoveUserPhotoUrl(userId), Times.Never());
        }

        [Test]
        public void SetProfilePhotoIdToNullAndResetCacheIfThePhotoIsProfilePhotoAndResetEventPhotoId_OnRemovePhoto()
        {
            //Arrange
            var feedId = 12;
            var photoId = 124;
            var userId = 12;
            var albumId = 444;
            var coverPhotoId = 2;
            var profilePhotoId = photoId;

            var photo = new Photo
            {
                Id = photoId,
                AlbumId = albumId,
                UserId = userId,
                User = new User
                {
                    ProfilePhotoId = profilePhotoId,
                    ClubDetail = new ClubDetail()
                    {
                        CoverPhotoId = coverPhotoId,
                    }
                }
            };

            _uow.Setup(x => x.PhotoRepository.GetById(photoId))
                .Returns(photo);

            _uow.Setup(x => x.PhotoRepository.GetOwnerId(photoId))
                .Returns(userId);
            _uow.Setup(x => x.PhotoRepository.Remove(photoId));
            _uow.Setup(x => x.EventRepository.ResetPhotoId(photoId));
            _uow.Setup(x => x.UserRepository.ResetPhotoId(photoId))
                .Returns(true);
            _uow.Setup(x => x.FeedPhotoRepository.RemoveByPhotoIdAndReturnFeedId(photoId))
                .Returns(feedId);
            _uow.Setup(x => x.FeedPhotoRepository.GetCount(feedId))
                .Returns(1);
            _uow.Setup(x => x.FeedRepository.Remove(feedId));

            //Act
            _sut.RemovePhoto(photoId, userId);

            //Assert
            Assert.AreEqual(coverPhotoId, photo.User.ClubDetail.CoverPhotoId);
            Assert.AreEqual(null, photo.User.ProfilePhotoId);
            _uow.Verify(x => x.PhotoRepository.Remove(photo), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Exactly(2));
            _uow.Verify(x => x.EventRepository.ResetPhotoId(photoId), Times.Once());
            _uow.Verify(x => x.UserRepository.ResetPhotoId(photoId), Times.Once());
            _uow.Verify(x => x.FeedPhotoRepository.RemoveByPhotoIdAndReturnFeedId(photoId), Times.Once());
            _uow.Verify(x => x.FeedPhotoRepository.GetCount(feedId), Times.Once());
            _uow.Verify(x => x.FeedRepository.Remove(feedId), Times.Never());
            _cacheService.Verify(x => x.RemoveUserPhotoUrl(userId), Times.Once());
        }

        [Test]
        public void RemoveFileAndDbAndFeedRecordIfThePictureIsTheLastOneOfFeedAndResetEventPhotoIdAndNotTouchCoverAndProfilePhotoIdsIfTheyAreDifferent_OnRemovePhoto()
        {
            //Arrange
            var photoId = 124;
            var userId = 12;
            var albumId = 444;
            var coverPhotoId = 4;
            var profilePhotoId = 2;
            var feedId = 888;

            var photo = new Photo
            {
                Id = photoId,
                AlbumId = albumId,
                UserId = userId,
                User = new User
                {
                    ProfilePhotoId = profilePhotoId,
                    ClubDetail = new ClubDetail()
                    {
                        CoverPhotoId = coverPhotoId,
                    }
                }
            };

            _uow.Setup(x => x.PhotoRepository.GetById(photoId))
                .Returns(photo);

            _uow.Setup(x => x.PhotoRepository.GetOwnerId(photoId))
                .Returns(userId);
            _uow.Setup(x => x.PhotoRepository.Remove(photoId));
            _uow.Setup(x => x.EventRepository.ResetPhotoId(photoId));
            _uow.Setup(x => x.UserRepository.ResetPhotoId(photoId));
            _uow.Setup(x => x.FeedPhotoRepository.RemoveByPhotoIdAndReturnFeedId(photoId))
                .Returns(feedId);
            _uow.Setup(x => x.FeedPhotoRepository.GetCount(feedId))
                .Returns(0);
            _uow.Setup(x => x.FeedRepository.Remove(feedId));

            //Act
            _sut.RemovePhoto(photoId, userId);

            //Assert
            Assert.AreEqual(coverPhotoId, photo.User.ClubDetail.CoverPhotoId);
            Assert.AreEqual(profilePhotoId, photo.User.ProfilePhotoId);
            _uow.Verify(x => x.PhotoRepository.Remove(photo), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Exactly(2));
            _uow.Verify(x => x.EventRepository.ResetPhotoId(photoId), Times.Once());
            _uow.Verify(x => x.UserRepository.ResetPhotoId(photoId), Times.Once());
            _uow.Verify(x => x.FeedPhotoRepository.RemoveByPhotoIdAndReturnFeedId(photoId), Times.Once());
            _uow.Verify(x => x.FeedPhotoRepository.GetCount(feedId), Times.Once());
            _uow.Verify(x => x.FeedRepository.Remove(feedId), Times.Once());
        }

        [Test]
        public void RemoveFileAndDbAndNotDeleteFeedRecordIfThePictureIsNotTheLastOneOfFeedAndResetEventPhotoIdAndNotTouchCoverAndProfilePhotoIdsIfTheyAreDifferent_OnRemovePhoto()
        {
            //Arrange
            var photoId = 124;
            var userId = 12;
            var albumId = 444;
            var coverPhotoId = 4;
            var profilePhotoId = 2;
            var feedId = 888;

            var photo = new Photo
            {
                Id = photoId,
                AlbumId = albumId,
                UserId = userId,
                User = new User
                {
                    ProfilePhotoId = profilePhotoId,
                    ClubDetail = new ClubDetail()
                    {
                        CoverPhotoId = coverPhotoId,
                    }
                }
            };

            _uow.Setup(x => x.PhotoRepository.GetById(photoId))
                .Returns(photo);


            _uow.Setup(x => x.PhotoRepository.GetOwnerId(photoId))
                .Returns(userId);
            _uow.Setup(x => x.PhotoRepository.Remove(photoId));
            _uow.Setup(x => x.EventRepository.ResetPhotoId(photoId));
            _uow.Setup(x => x.UserRepository.ResetPhotoId(photoId));
            _uow.Setup(x => x.FeedPhotoRepository.RemoveByPhotoIdAndReturnFeedId(photoId))
                .Returns(feedId);
            _uow.Setup(x => x.FeedPhotoRepository.GetCount(feedId))
                .Returns(1);
            _uow.Setup(x => x.FeedRepository.Remove(feedId));

            //Act
            _sut.RemovePhoto(photoId, userId);

            //Assert
            Assert.AreEqual(coverPhotoId, photo.User.ClubDetail.CoverPhotoId);
            Assert.AreEqual(profilePhotoId, photo.User.ProfilePhotoId);
            _uow.Verify(x => x.PhotoRepository.Remove(photo), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Exactly(2));
            _uow.Verify(x => x.EventRepository.ResetPhotoId(photoId), Times.Once());
            _uow.Verify(x => x.UserRepository.ResetPhotoId(photoId), Times.Once());
            _uow.Verify(x => x.FeedPhotoRepository.RemoveByPhotoIdAndReturnFeedId(photoId), Times.Once());
            _uow.Verify(x => x.FeedPhotoRepository.GetCount(feedId), Times.Once());
            _uow.Verify(x => x.FeedRepository.Remove(feedId), Times.Never());
        }

        [Test]
        public void NotTryToRemoveFeedWhenTheFeedDoesntExists_OnRemovePhoto()
        {
            //Arrange
            var photoId = 124;
            var userId = 12;
            var albumId = 444;
            var coverPhotoId = 4;
            var profilePhotoId = 2;

            var photo = new Photo
            {
                Id = photoId,
                AlbumId = albumId,
                UserId = userId,
                User = new User
                {
                    ProfilePhotoId = profilePhotoId,
                    ClubDetail = new ClubDetail()
                    {
                        CoverPhotoId = coverPhotoId,
                    }
                }
            };

            _uow.Setup(x => x.PhotoRepository.GetById(photoId))
                .Returns(photo);


            _uow.Setup(x => x.PhotoRepository.GetOwnerId(photoId))
                .Returns(userId);
            _uow.Setup(x => x.PhotoRepository.Remove(photoId));
            _uow.Setup(x => x.EventRepository.ResetPhotoId(photoId));
            _uow.Setup(x => x.UserRepository.ResetPhotoId(photoId));
            _uow.Setup(x => x.FeedPhotoRepository.RemoveByPhotoIdAndReturnFeedId(photoId))
                .Returns(0);
            _uow.Setup(x => x.FeedPhotoRepository.GetCount(It.IsAny<int>()))
                .Returns(0);
            _uow.Setup(x => x.FeedRepository.Remove(It.IsAny<int>()));

            //Act
            _sut.RemovePhoto(photoId, userId);

            //Assert
            Assert.AreEqual(coverPhotoId, photo.User.ClubDetail.CoverPhotoId);
            Assert.AreEqual(profilePhotoId, photo.User.ProfilePhotoId);
            _uow.Verify(x => x.PhotoRepository.Remove(photo), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Exactly(2));
            _uow.Verify(x => x.EventRepository.ResetPhotoId(photoId), Times.Once());
            _uow.Verify(x => x.UserRepository.ResetPhotoId(photoId), Times.Once());
            _uow.Verify(x => x.FeedPhotoRepository.RemoveByPhotoIdAndReturnFeedId(photoId), Times.Once());
            _uow.Verify(x => x.FeedPhotoRepository.GetCount(It.IsAny<int>()), Times.Never());
            _uow.Verify(x => x.FeedRepository.Remove(It.IsAny<int>()), Times.Never());
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
            _uow.Setup(x => x.PhotoRepository.GetOwnerId(photo.Id))
                .Returns(photo.UserId);

            //Act
            Assert.Throws<ArgumentException>(() => _sut.UpdatePhoto(photo, 1234));

            //Assert
            _uow.Verify(x => x.PhotoRepository.GetOwnerId(photo.Id), Times.Never());
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
            Assert.Throws<NotFoundException>(() => _sut.UpdatePhoto(photo, userId));

            //Assert
            _uow.Verify(x => x.PhotoRepository.GetById(photo.Id), Times.Once());
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
            Assert.Throws<SecurityException>(() => _sut.UpdatePhoto(photo, userId));

            //Assert
            _uow.Verify(x => x.PhotoRepository.GetById(photo.Id), Times.Once());
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

            //Act
            _sut.UpdatePhoto(photo, userId);

            //Assert
            _uow.Verify(x => x.PhotoRepository.GetById(photo.Id), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Once());
        }

        [Test]
        public void UpdateDescription_OnUpdatePhoto()
        {
            //Arrange
            var photoId = 124;
            var userId = 12;

            var oldPhoto = new Photo
                           {
                               Id = photoId,
                               Description = "old desc",
                               UserId = userId
                           };

            var newPhoto = new Photo
                           {
                               Id = photoId,
                               Description = "new Desc",
                               UserId = userId,
                               AlbumId = 12
                           };
            _stringHelper.Setup(x => x.ExtractTags(It.IsAny<string>()))
                         .Returns(Enumerable.Empty<string>);
            _uow.Setup(x => x.PhotoRepository.GetById(newPhoto.Id))
                .Returns(oldPhoto);

            //Act
            _sut.UpdatePhoto(newPhoto, userId);

            //Assert
            Assert.AreEqual(oldPhoto.Description, newPhoto.Description);
            Assert.AreEqual(oldPhoto.AlbumId, newPhoto.AlbumId);
        }

        [Test]
        public void UpdateTags_OnUpdatePhoto()
        {
            //Arrange
            var currentUser = 444;
            var tag1 = "#tag1";
            var tag2 = "#tag2";
            var notAvailableTag = "#tag3";

            var tagObject1 = new Category { Id = 1 };
            var tagObject2 = new Category { Id = 2 };

            var photo = new Photo
            {
                Id = 1234,
                UserId = currentUser,
                Description = String.Format("some text + {0} and {1} and {2}", tag1, tag2, notAvailableTag)
            };

            _stringHelper.Setup(x => x.ExtractTags(photo.Description))
                         .Returns(new List<string> { tag1, tag2, notAvailableTag });
            _staticDataRepo.Setup(x => x.GetCategoryIfExists(tag1.Replace("#", "")))
                           .Returns(tagObject1);
            _staticDataRepo.Setup(x => x.GetCategoryIfExists(tag2.Replace("#", "")))
                           .Returns(tagObject2);
            _staticDataRepo.Setup(x => x.GetCategoryIfExists(notAvailableTag.Replace("#", "")))
                           .Returns(() => null);

            _uow.Setup(x => x.PhotoRepository.GetById(photo.Id))
                .Returns(photo);

            //Act
            _sut.UpdatePhoto(photo, currentUser);

            //Assert
            Assert.AreEqual(2, photo.Categories.Count);
            Assert.IsTrue(photo.Categories.Any(t => t.CategoryId == tagObject1.Id));
            Assert.IsTrue(photo.Categories.Any(t => t.CategoryId == tagObject2.Id));
        }

        [Test]
        public void NotAddDuplicateTags_OnUpdatePhoto()
        {
            //Arrange
            var currentUser = 444;
            var tag1 = "#tag1";
            var duplicateTag1 = tag1;
            var tag2 = "#tag2";
            var notAvailableTag = "#tag3";

            var tagObject1 = new Category { Id = 1 };
            var tagObject2 = new Category { Id = 2 };

            var photo = new Photo
            {
                Id = 1234,
                UserId = currentUser,
                Description = String.Format("some text + {0} and {1} and {2}", tag1, tag2, notAvailableTag)
            };

            _stringHelper.Setup(x => x.ExtractTags(photo.Description))
                         .Returns(new List<string> { tag1, tag2, duplicateTag1, notAvailableTag });
            _staticDataRepo.Setup(x => x.GetCategoryIfExists(tag1.Replace("#", "")))
                           .Returns(tagObject1);
            _staticDataRepo.Setup(x => x.GetCategoryIfExists(tag2.Replace("#", "")))
                           .Returns(tagObject2);
            _staticDataRepo.Setup(x => x.GetCategoryIfExists(notAvailableTag.Replace("#", "")))
                           .Returns(() => null);

            _uow.Setup(x => x.PhotoRepository.GetById(photo.Id))
                .Returns(photo);

            //Act
            _sut.UpdatePhoto(photo, currentUser);

            //Assert
            Assert.AreEqual(2, photo.Categories.Count);
            Assert.IsTrue(photo.Categories.Any(t => t.CategoryId == tagObject1.Id));
            Assert.IsTrue(photo.Categories.Any(t => t.CategoryId == tagObject2.Id));
        }

        [Test]
        public void IgnoreCaseOnDuplicateTagsCheck_OnUpdatePhoto()
        {
            //Arrange
            var currentUser = 444;
            var tag1 = "#tag1";
            var duplicateTag1 = "#TAG1";
            var tag2 = "#tag2";
            var notAvailableTag = "#tag3";

            var tagObject1 = new Category { Id = 1 };
            var tagObject2 = new Category { Id = 2 };

            var photo = new Photo
            {
                Id = 1234,
                UserId = currentUser,
                Description = String.Format("some text + {0} and {1} and {2}", tag1, tag2, notAvailableTag)
            };

            _stringHelper.Setup(x => x.ExtractTags(photo.Description))
                         .Returns(new List<string> { tag1, tag2, duplicateTag1, notAvailableTag });
            _staticDataRepo.Setup(x => x.GetCategoryIfExists(tag1.Replace("#", "")))
                           .Returns(tagObject1);
            _staticDataRepo.Setup(x => x.GetCategoryIfExists(duplicateTag1.Replace("#", "")))
                           .Returns(tagObject1);
            _staticDataRepo.Setup(x => x.GetCategoryIfExists(tag2.Replace("#", "")))
                           .Returns(tagObject2);
            _staticDataRepo.Setup(x => x.GetCategoryIfExists(notAvailableTag.Replace("#", "")))
                           .Returns(() => null);

            _uow.Setup(x => x.PhotoRepository.GetById(photo.Id))
                .Returns(photo);

            //Act
            _sut.UpdatePhoto(photo, currentUser);

            //Assert
            Assert.AreEqual(2, photo.Categories.Count);
            Assert.IsTrue(photo.Categories.Any(t => t.CategoryId == tagObject1.Id));
            Assert.IsTrue(photo.Categories.Any(t => t.CategoryId == tagObject2.Id));
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
            _photo.Dispose();
        }
    }
}