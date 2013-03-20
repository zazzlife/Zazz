using System;
using System.Collections.Generic;
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
    public class AlbumServiceShould
    {
        private Mock<IUoW> _uoW;
        private AlbumService _sut;
        private Album _album;
        private int _userId;
        private Mock<IPhotoService> _photoService;
        private int _photoId;
        private Mock<IFileService> _fileService;

        [SetUp]
        public void Init()
        {
            _uoW = new Mock<IUoW>();
            _photoService = new Mock<IPhotoService>();
            _fileService = new Mock<IFileService>();
            _sut = new AlbumService(_uoW.Object, _photoService.Object, _fileService.Object);
            _album = new Album();
            _userId = 12;
            _photoId = 1;

            _uoW.Setup(x => x.SaveAsync())
                .Returns(() => Task.Run(() => { }));
        }

        [Test]
        public void GenerateCorrectAlbumPath_OnGenerateAlbumPath()
        {
            //Arrange
            var albumId = 2;

            var sample = String.Format(@"C:\Zazz.Web\picture\user\{0}\{1}\0.jpg", _userId, albumId);
            var expected = String.Format(@"C:\Zazz.Web\picture\user\{0}\{1}", _userId, albumId);
            var sut = new AlbumService(_uoW.Object, _photoService.Object, new FileService());
            _photoService.Setup(x => x.GeneratePhotoFilePath(_userId, albumId, 0))
                         .Returns(sample);

            //Act
            var result = sut.GenerateAlbumPath(_userId, albumId);

            //Assert
            Assert.AreEqual(expected, result);
        }



        [Test]
        public async Task InsertAndSave_OnCreateAlbum()
        {
            //Arrange
            _uoW.Setup(x => x.AlbumRepository.InsertGraph(_album));

            //Act
            await _sut.CreateAlbumAsync(_album);

            //Assert
            _uoW.Verify(x => x.AlbumRepository.InsertGraph(_album), Times.Once());
            _uoW.Verify(x => x.SaveAsync(), Times.Once());
        }

        [Test]
        public async Task ThrowIfAlbumIdIs0_OnUpdateAlbum()
        {
            //Arrange
            //Act
            try
            {
                await _sut.UpdateAlbumAsync(_album, _userId);
                Assert.Fail("Expected exception wasn't thrown");
            }
            catch (ArgumentException)
            {
            }
        }

        [Test]
        public async Task CheckForOwnerIdAndThrowIfDoesntMatch_OnUpdateAlbum()
        {
            //Arrange
            _album.Id = 144;
            _uoW.Setup(x => x.AlbumRepository.GetOwnerIdAsync(_album.Id))
                .Returns(() => Task.Run(() => 155));

            //Act & Assert
            try
            {
                await _sut.UpdateAlbumAsync(_album, _userId);
                Assert.Fail("Expected exception wasn't thrown");
            }
            catch (SecurityException)
            {
            }
            _uoW.Verify(x => x.AlbumRepository.GetOwnerIdAsync(_album.Id), Times.Once());
        }

        [Test]
        public async Task UpdateAndSave_OnUpdateAlbum()
        {
            //Arrange
            _album.Id = 144;
            _uoW.Setup(x => x.AlbumRepository.GetOwnerIdAsync(_album.Id))
                .Returns(() => Task.Run(() => _userId));
            _uoW.Setup(x => x.AlbumRepository.InsertOrUpdate(_album));

            //Act
            await _sut.UpdateAlbumAsync(_album, _userId);

            //Assert
            _uoW.Verify(x => x.AlbumRepository.InsertOrUpdate(_album), Times.Once());
            _uoW.Verify(x => x.SaveAsync(), Times.Once());
        }

        [Test]
        public async Task ThrowIfAlbumIdIs0_OnDeleteAlbum()
        {
            //Arrange
            //Act
            try
            {
                await _sut.DeleteAlbumAsync(0, _userId);
                Assert.Fail("Expected exception wasn't thrown");
            }
            catch (ArgumentException)
            {
            }
        }

        [Test]
        public async Task CheckForOwnerIdAndThrowIfDoesntMatch_OnDeleteAlbum()
        {
            //Arrange
            _album.Id = 123;
            _album.UserId = 400;
            _uoW.Setup(x => x.AlbumRepository.GetOwnerIdAsync(_album.Id))
                .Returns(() => Task.Run(() => _album.UserId));

            //Act & Assert
            try
            {
                await _sut.DeleteAlbumAsync(_album.Id, _userId);
                Assert.Fail("Expected exception wasn't thrown");
            }
            catch (SecurityException)
            {
            }
            _uoW.Verify(x => x.AlbumRepository.GetOwnerIdAsync(_album.Id), Times.Once());
        }

        [Test]
        public async Task RemoveAllAlbumDirectory_OnDeleteAlbum()
        {
            //Arrange
            _album.Id = 123;
            var dirPath = "c:\test";
            _album.UserId = _userId;
            _uoW.Setup(x => x.AlbumRepository.GetOwnerIdAsync(_album.Id))
               .Returns(() => Task.Run(() => _userId));
            _uoW.Setup(x => x.AlbumRepository.RemoveAsync(_album.Id))
                .Returns(() => Task.Run(() => { }));
            _photoService.Setup(x => x.GeneratePhotoFilePath(_userId, _album.Id, 0))
                         .Returns(dirPath);
            _fileService.Setup(x => x.RemoveFileNameFromPath(dirPath))
                        .Returns(dirPath);
            _fileService.Setup(x => x.RemoveDirectory(dirPath));

            //Act
            await _sut.DeleteAlbumAsync(_album.Id, _userId);

            //Assert
            _photoService.Verify(x => x.GeneratePhotoFilePath(_userId, _album.Id, 0), Times.Once());
            _fileService.Verify(x => x.RemoveDirectory(dirPath), Times.Once());

        }

        [Test]
        public async Task DeleteAndSave_OnDeleteAlbum()
        {
            //Arrange
            var photoIds = new[] {1, 2, 3, 4};
            
            _album.Id = 123;
            var dirPath = "c:\test";
            _album.UserId = _userId;
            _uoW.Setup(x => x.AlbumRepository.GetOwnerIdAsync(_album.Id))
                .Returns(() => Task.Run(() => _userId));
            _uoW.Setup(x => x.AlbumRepository.RemoveAsync(_album.Id))
                .Returns(() => Task.Run(() => { }));
            _uoW.Setup(x => x.AlbumRepository.GetAlbumPhotoIds(_album.Id))
                .Returns(photoIds);

            _uoW.Setup(x => x.UserRepository.ResetPhotoId(It.IsAny<int>()));
            _uoW.Setup(x => x.FeedRepository.RemovePhotoFeed(It.IsAny<int>()));
            _uoW.Setup(x => x.PostRepository.ResetPhotoId(It.IsAny<int>()));

            _photoService.Setup(x => x.GeneratePhotoFilePath(_userId, _album.Id, 0))
                         .Returns(dirPath);
            _fileService.Setup(x => x.RemoveFileNameFromPath(dirPath))
                        .Returns(dirPath);
            _fileService.Setup(x => x.RemoveDirectory(dirPath));

            //Act
            await _sut.DeleteAlbumAsync(_album.Id, _userId);

            //Assert
            _uoW.Verify(x => x.AlbumRepository.RemoveAsync(_album.Id), Times.Once());
            _uoW.Verify(x => x.SaveAsync(), Times.Once());
            _uoW.Verify(x => x.UserRepository.ResetPhotoId(It.IsInRange(1, 4, Range.Inclusive)),
                        Times.Exactly(photoIds.Length));
            _uoW.Verify(x => x.FeedRepository.RemovePhotoFeed(It.IsInRange(1, 4, Range.Inclusive)),
                        Times.Exactly(photoIds.Length));
            _uoW.Verify(x => x.PostRepository.ResetPhotoId(It.IsInRange(1, 4, Range.Inclusive)),
                        Times.Exactly(photoIds.Length));
        }
    }
}