using System;
using System.Collections.Generic;
using System.Security;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Zazz.Core.Exceptions;
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
            _album = new Album
                     {
                         Name = "name"
                     };

            _userId = 12;
            _photoId = 1;

            _uow.Setup(x => x.SaveChanges());
        }

        [Test]
        public void InsertAndSave_OnCreateAlbum()
        {
            //Arrange
            _uow.Setup(x => x.AlbumRepository.InsertGraph(_album));

            //Act
            _sut.CreateAlbum(_album);

            //Assert
            _uow.Verify(x => x.AlbumRepository.InsertGraph(_album), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Once());
        }

        [Test]
        public void ThrowNotFoundIfAlbumNotFOund_OnUpdateAlbum()
        {
            //Arrange
            var albumId = 22;
            _uow.Setup(x => x.AlbumRepository.GetById(albumId))
                .Returns(() => null);

            //Act
            Assert.Throws<NotFoundException>(() => _sut.UpdateAlbum(albumId, "new name", _userId));

            //Assert
            _uow.Verify(x => x.AlbumRepository.GetById(albumId), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Never());
        }

        [Test]
        public void CheckForOwnerIdAndThrowIfDoesntMatch_OnUpdateAlbum()
        {
            //Arrange
            _album.Id = 144;
            _album.UserId = 1900;
            _uow.Setup(x => x.AlbumRepository.GetById(_album.Id))
                .Returns(_album);

            //Act & Assert
            Assert.Throws<SecurityException>(() => _sut.UpdateAlbum(_album.Id, "new name", _userId));
            _uow.Verify(x => x.AlbumRepository.GetById(_album.Id), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Never());
        }

        [Test]
        public void UpdateAndSave_OnUpdateAlbum()
        {
            //Arrange
            _album.Id = 144;
            _album.UserId = _userId;
            _uow.Setup(x => x.AlbumRepository.GetById(_album.Id))
                .Returns(_album);
            var newName = "new name";

            //Act
            _sut.UpdateAlbum(_album.Id, newName, _userId);

            //Assert
            Assert.AreEqual(newName, _album.Name);
            _uow.Verify(x => x.SaveChanges(), Times.Once());
        }

        [Test]
        public void ThrowIfAlbumIdIs0_OnDeleteAlbum()
        {
            //Arrange
            //Act
            try
            {
                _sut.DeleteAlbum(0, _userId);
                Assert.Fail("Expected exception wasn't thrown");
            }
            catch (ArgumentException)
            {
            }
        }

        [Test]
        public void CheckForOwnerIdAndThrowIfDoesntMatch_OnDeleteAlbum()
        {
            //Arrange
            _album.Id = 123;
            _album.UserId = 400;
            _uow.Setup(x => x.AlbumRepository.GetOwnerId(_album.Id))
                .Returns(_album.UserId);

            //Act & Assert
            try
            {
                _sut.DeleteAlbum(_album.Id, _userId);
                Assert.Fail("Expected exception wasn't thrown");
            }
            catch (SecurityException)
            {
            }
            _uow.Verify(x => x.AlbumRepository.GetOwnerId(_album.Id), Times.Once());
        }

        [Test]
        public void DeleteAndSave_OnDeleteAlbum()
        {
            //Arrange
            var photoIds = new[] {1, 2, 3, 4};
            
            _album.Id = 123;
            _album.UserId = _userId;
            _uow.Setup(x => x.AlbumRepository.GetOwnerId(_album.Id))
                .Returns(_userId);
            _uow.Setup(x => x.AlbumRepository.Remove(_album.Id));
            _uow.Setup(x => x.AlbumRepository.GetAlbumPhotoIds(_album.Id))
                .Returns(photoIds);
            _photoService.Setup(x => x.RemovePhoto(It.IsInRange(1, 4, Range.Inclusive), _userId));

            //Act
            _sut.DeleteAlbum(_album.Id, _userId);

            //Assert
            _uow.Verify(x => x.AlbumRepository.Remove(_album.Id), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Once());
            _photoService.Verify(x => x.RemovePhoto(It.IsInRange(1, 4, Range.Inclusive), _userId),
                                 Times.Exactly(photoIds.Length));
        }
    }
}