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

        [SetUp]
        public void Init()
        {
            _uoW = new Mock<IUoW>();
            _photoService = new Mock<IPhotoService>();
            _sut = new AlbumService(_uoW.Object, _photoService.Object);
            _album = new Album();
            _userId = 12;
            _photoId = 1;

            _uoW.Setup(x => x.SaveAsync())
                .Returns(() => Task.Run(() => { }));
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
            _uoW.Setup(x => x.AlbumRepository.GetByIdAsync(_album.Id))
                .Returns(() => Task.Run(() => _album));

            //Act & Assert
            try
            {
                await _sut.DeleteAlbumAsync(_album.Id, _userId);
                Assert.Fail("Expected exception wasn't thrown");
            }
            catch (SecurityException)
            {
            }
            _uoW.Verify(x => x.AlbumRepository.GetByIdAsync(_album.Id), Times.Once());
        }

        [Test]
        public async Task RemoveAllImagesFirst_OnDeleteAlbum()
        {
            //Arrange
            _album.Id = 123;
            _album.UserId = _userId;
            _album.Photos = new List<Photo>
                                {
                                    new Photo {Id = _photoId},
                                    new Photo {Id = _photoId},
                                    new Photo {Id = _photoId},
                                };

            _uoW.Setup(x => x.AlbumRepository.GetByIdAsync(_album.Id))
                .Returns(() => Task.Run(() => _album));
            _uoW.Setup(x => x.AlbumRepository.Remove(_album));
            _photoService.Setup(x => x.RemovePhotoAsync(_photoId, _userId))
                         .Returns(() => Task.Run(() => { }));

            //Act
            await _sut.DeleteAlbumAsync(_album.Id, _userId);

            //Assert
            _photoService.Verify(x => x.RemovePhotoAsync(_photoId, _userId), Times.Exactly(_album.Photos.Count));
        }

        [Test]
        public async Task DeleteAndSave_OnDeleteAlbum()
        {
            //Arrange
            _album.Id = 123;
            _album.UserId = _userId;
            _uoW.Setup(x => x.AlbumRepository.GetByIdAsync(_album.Id))
                .Returns(() => Task.Run(() => _album));
            _uoW.Setup(x => x.AlbumRepository.Remove(_album));
            _photoService.Setup(x => x.RemovePhotoAsync(It.IsAny<int>(), _userId))
                         .Returns(() => Task.Run(() => { }));

            //Act
            await _sut.DeleteAlbumAsync(_album.Id, _userId);

            //Assert
            _uoW.Verify(x => x.AlbumRepository.Remove(_album), Times.Once());
            _uoW.Verify(x => x.SaveAsync(), Times.Once());
        }
    }
}