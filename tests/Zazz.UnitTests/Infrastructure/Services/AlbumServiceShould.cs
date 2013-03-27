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
        private Mock<IUoW> _uow;
        private AlbumService _sut;
        private Album _album;
        private int _userId;
        private Mock<IPhotoService> _photoService;
        private int _photoId;

        [SetUp]
        public void Init()
        {
            _uow = new Mock<IUoW>();
            _photoService = new Mock<IPhotoService>();
            _sut = new AlbumService(_uow.Object, _photoService.Object);
            _album = new Album();
            _userId = 12;
            _photoId = 1;

            _uow.Setup(x => x.SaveChanges());
        }

        [Test]
        public async Task InsertAndSave_OnCreateAlbum()
        {
            //Arrange
            _uow.Setup(x => x.AlbumRepository.InsertGraph(_album));

            //Act
            await _sut.CreateAlbumAsync(_album);

            //Assert
            _uow.Verify(x => x.AlbumRepository.InsertGraph(_album), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Once());
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
            _uow.Setup(x => x.AlbumRepository.GetOwnerIdAsync(_album.Id))
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
            _uow.Verify(x => x.AlbumRepository.GetOwnerIdAsync(_album.Id), Times.Once());
        }

        [Test]
        public async Task UpdateAndSave_OnUpdateAlbum()
        {
            //Arrange
            _album.Id = 144;
            _uow.Setup(x => x.AlbumRepository.GetOwnerIdAsync(_album.Id))
                .Returns(() => Task.Run(() => _userId));
            _uow.Setup(x => x.AlbumRepository.InsertOrUpdate(_album));

            //Act
            await _sut.UpdateAlbumAsync(_album, _userId);

            //Assert
            _uow.Verify(x => x.AlbumRepository.InsertOrUpdate(_album), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Once());
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
            _uow.Setup(x => x.AlbumRepository.GetOwnerIdAsync(_album.Id))
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
            _uow.Verify(x => x.AlbumRepository.GetOwnerIdAsync(_album.Id), Times.Once());
        }

        [Test]
        public async Task DeleteAndSave_OnDeleteAlbum()
        {
            //Arrange
            var photoIds = new[] {1, 2, 3, 4};
            
            _album.Id = 123;
            var dirPath = "c:\test";
            _album.UserId = _userId;
            _uow.Setup(x => x.AlbumRepository.GetOwnerIdAsync(_album.Id))
                .Returns(() => Task.Run(() => _userId));
            _uow.Setup(x => x.AlbumRepository.RemoveAsync(_album.Id))
                .Returns(() => Task.Run(() => { }));
            _uow.Setup(x => x.AlbumRepository.GetAlbumPhotoIds(_album.Id))
                .Returns(photoIds);
            _photoService.Setup(x => x.RemovePhotoAsync(It.IsInRange(1, 4, Range.Inclusive), _userId))
                         .Returns(() => Task.Run(() => { }));

            _photoService.Setup(x => x.GeneratePhotoFilePath(_userId, 0))
                         .Returns(dirPath);

            //Act
            await _sut.DeleteAlbumAsync(_album.Id, _userId);

            //Assert
            _uow.Verify(x => x.AlbumRepository.RemoveAsync(_album.Id), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Once());
            _photoService.Verify(x => x.RemovePhotoAsync(It.IsInRange(1, 4, Range.Inclusive), _userId),
                                 Times.Exactly(photoIds.Length));
        }
    }
}