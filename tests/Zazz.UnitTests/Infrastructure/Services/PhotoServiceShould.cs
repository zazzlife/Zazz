using System;
using System.IO;
using System.Security;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Infrastructure.Services;

namespace Zazz.UnitTests.Infrastructure.Services
{
    [TestFixture]
    public class PhotoServiceShould
    {
        private Mock<IUoW> _uow;
        private Mock<IFileService> _fs;
        private PhotoService _sut;
        private string _tempRootPath;

        [SetUp]
        public void Init()
        {
            _uow = new Mock<IUoW>();
            _fs = new Mock<IFileService>();
            _tempRootPath = Path.GetTempPath();
            _sut = new PhotoService(_uow.Object, _fs.Object, _tempRootPath);
            _uow.Setup(x => x.SaveAsync())
                .Returns(() => Task.Run(() => { }));
        }

        [TestCase("/picture/user/12/2/333.jpg", 12, 2, 333)]
        [TestCase("/picture/user/800/9000/1203200.jpg", 800, 9000, 1203200)]
        [TestCase("/picture/user/102/20/3330.jpg", 102, 20, 3330)]
        public void GenerateCorrectPath_OnGeneratePhotoUrl(string expected, int userId, int albumId, int photoId)
        {
            //Arrange
            //Act
            var result = _sut.GeneratePhotoUrl(userId, albumId, photoId);

            //Assert
            Assert.AreEqual(expected, result);
        }

        [TestCase(12, 2, 333)]
        [TestCase(800, 9000, 1203200)]
        [TestCase(102, 20, 3330)]
        public void GenerateCorrectPath_OnGeneratePhotoFilePath(int userId, int albumId, int photoId)
        {
            //Arrange
            var expected = String.Format(@"{0}\picture\user\{1}\{2}\{3}.jpg", _tempRootPath, userId, albumId, photoId);

            //Act
            var result = _sut.GeneratePhotoFilePath(userId, albumId, photoId);

            //Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public async Task CallGetDescriptionFromRepo_OnGetPhotoDescriptionAsync()
        {
            //Arrange
            var id = 123;
            _uow.Setup(x => x.PhotoRepository.GetDescriptionAsync(id))
                .Returns(() => Task.Run(() => "description"));

            //Act
            var result = await _sut.GetPhotoDescriptionAsync(id);

            //Assert
            _uow.Verify(x => x.PhotoRepository.GetDescriptionAsync(id), Times.Once());
        }

        [Test]
        public async Task SavePhotoToDiskAndDB_OnSavePhoto()
        {
            //Arrange
            var photo = new Photo
                            {
                                Id = 1234,
                                AlbumId = 12,
                                Description = "desc",
                                UploaderId = 17
                            };
            _uow.Setup(x => x.PhotoRepository.InsertGraph(photo));
            var path = _sut.GeneratePhotoFilePath(photo.UploaderId, photo.AlbumId, photo.Id);
            using (var ms = new MemoryStream())
            {
                _fs.Setup(x => x.SaveFileAsync(path, ms))
                   .Returns(() => Task.Run(() => { }));

                //Act

                await _sut.SavePhotoAsync(photo, ms);

                //Assert
                _uow.Verify(x => x.PhotoRepository.InsertGraph(photo), Times.Once());
                _uow.Verify(x => x.SaveAsync());
                _fs.Verify(x => x.SaveFileAsync(path, ms));
                Assert.AreEqual(DateTime.UtcNow.Date, photo.UploadDate.Date);
            }
        }

        [Test]
        public async Task ThrowIfTheCurrentUserIsNotTheOwner_OnRemovePhoto()
        {
            //Arrange
            var photoId = 124;
            var userId = 12;
            var photo = new Photo
                            {
                                Id = photoId,
                                AlbumId = 123,
                                UploaderId = 999
                            };

            _uow.Setup(x => x.PhotoRepository.GetByIdAsync(photoId))
                .Returns(() => Task.Run(() => photo));

            //Act
            try
            {
                await _sut.RemovePhotoAsync(photoId, userId);
                Assert.Fail("Expected exception wasn't thrown");
            }
            catch (SecurityException)
            {
            }

            //Assert
            _uow.Verify(x => x.PhotoRepository.GetByIdAsync(photoId), Times.Once());
        }

        [Test]
        public async Task RemoveFileAndDbRecord_OnRemovePhoto()
        {
            //Arrange
            var photoId = 124;
            var userId = 12;
            var albumId = 444;

            var photo = new Photo
                            {
                                Id = photoId,
                                AlbumId = albumId,
                                UploaderId = userId
                            };
            
            _uow.Setup(x => x.PhotoRepository.GetByIdAsync(photoId))
                .Returns(() => Task.Run(() => photo));

            var filePath = _sut.GeneratePhotoFilePath(userId, albumId, photoId);

            _uow.Setup(x => x.PhotoRepository.GetOwnerIdAsync(photoId))
                .Returns(() => Task.Run(() => userId));
            _uow.Setup(x => x.PhotoRepository.RemoveAsync(photoId))
                .Returns(() => Task.Run(() => { }));
            _fs.Setup(x => x.RemoveFile(filePath));

            //Act
            await _sut.RemovePhotoAsync(photoId, userId);

            //Assert
            _uow.Verify(x => x.PhotoRepository.Remove(photo), Times.Once());
            _uow.Verify(x => x.SaveAsync(), Times.Once());
            _fs.Verify(x => x.RemoveFile(filePath), Times.Once());
        }

        [Test]
        public async Task ThrowIfUserPhotoIdIs0_OnUpdatePhoto()
        {
            //Arrange

            var photo = new Photo
            {
                Id = 0,
                UploaderId = 124,
                AlbumId = 123
            };
            _uow.Setup(x => x.PhotoRepository.GetOwnerIdAsync(photo.Id))
                .Returns(() => Task.Run(() => photo.UploaderId));

            //Act
            try
            {
                await _sut.UpdatePhotoAsync(photo, 1234);
                Assert.Fail("Expected exception wasn't thrown");
            }
            catch (ArgumentException)
            {
            }

            //Assert
            _uow.Verify(x => x.PhotoRepository.GetOwnerIdAsync(photo.Id), Times.Never());
        }

        [Test]
        public async Task ThrowIfUserIdIsNotSameAsOwnerId_OnUpdatePhoto()
        {
            //Arrange
            var photoId = 124;
            var userId = 12;
            var albumId = 444;

            var photo = new Photo
            {
                Id = photoId,
                AlbumId = albumId,
                UploaderId = 890
            };

            _uow.Setup(x => x.PhotoRepository.GetOwnerIdAsync(photo.Id))
                .Returns(() => Task.Run(() => photo.UploaderId));

            //Act
            try
            {
                await _sut.UpdatePhotoAsync(photo, userId);
                Assert.Fail("Expected exception wasn't thrown");
            }
            catch (SecurityException)
            {
            }

            //Assert
            _uow.Verify(x => x.PhotoRepository.GetOwnerIdAsync(photo.Id), Times.Once());
        }

        [Test]
        public async Task UpdateAndSave_OnUpdatePhoto()
        {
            //Arrange
            var photoId = 124;
            var userId = 12;
            var albumId = 444;

            var photo = new Photo
            {
                Id = photoId,
                AlbumId = albumId,
                UploaderId = userId
            };

            _uow.Setup(x => x.PhotoRepository.GetOwnerIdAsync(photo.Id))
                .Returns(() => Task.Run(() => photo.UploaderId));
            _uow.Setup(x => x.PhotoRepository.InsertOrUpdate(photo));

            //Act
            await _sut.UpdatePhotoAsync(photo, userId);

            //Assert
            _uow.Verify(x => x.PhotoRepository.GetOwnerIdAsync(photo.Id), Times.Once());
            _uow.Verify(x => x.PhotoRepository.InsertOrUpdate(photo), Times.Once());
            _uow.Verify(x => x.SaveAsync(), Times.Once());
        }


    }
}