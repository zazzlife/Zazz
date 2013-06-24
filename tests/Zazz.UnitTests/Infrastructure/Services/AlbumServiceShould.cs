using System;
using System.Collections.Generic;
using System.Security;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Zazz.Core.Exceptions;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.DTOs;
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
        private MockRepository _mockRepo;

        [SetUp]
        public void Init()
        {
            _mockRepo = new MockRepository(MockBehavior.Strict);
            _uow = _mockRepo.Create<IUoW>();
            _photoService = _mockRepo.Create<IPhotoService>();
            _sut = new AlbumService(_uow.Object, _photoService.Object);
            _album = new Album
                     {
                         Name = "name"
                     };

            _userId = 12;
            _photoId = 1;
        }

        [Test]
        public void ThrowNotFoundIfAlbumDoesntExists_OnGetAlbumWithThumbnail()
        {
            //Arrange
            var albumId = 444;
            _uow.Setup(x => x.AlbumRepository.GetAlbumWithThumbnail(albumId))
                .Returns(() => null);

            //Act
            Assert.Throws<NotFoundException>(() => _sut.GetAlbumWithThumbnail(albumId));

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void ReturnResultFromRepo_OnGetAlbumWithThumbnail()
        {
            //Arrange
            var albumWithThumbnailDTO = new AlbumWithThumbnailDTO();
            var albumId = 444;
            _uow.Setup(x => x.AlbumRepository.GetAlbumWithThumbnail(albumId))
                .Returns(albumWithThumbnailDTO);

            //Act
            var result = _sut.GetAlbumWithThumbnail(albumId);

            //Assert
            Assert.AreSame(albumWithThumbnailDTO, result);
            _mockRepo.VerifyAll();
        }

        [Test]
        public void ThrowNotFoundIfAlbumDoesntExists_OnGetAlbum()
        {
            //Arrange
            var albumId = 444;
            _uow.Setup(x => x.AlbumRepository.GetById(albumId, true))
                .Returns(() => null);

            //Act
            Assert.Throws<NotFoundException>(() => _sut.GetAlbum(albumId, true));

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void ReturnResultFromRepo_OnGetAlbum()
        {
            //Arrange
            var albumId = 444;
            _uow.Setup(x => x.AlbumRepository.GetById(albumId, true))
                .Returns(_album);

            //Act
            var result = _sut.GetAlbum(albumId, true);

            //Assert
            Assert.AreSame(_album, result);
            _mockRepo.VerifyAll();
        }

        [Test]
        public void InsertAndSave_OnCreateAlbum()
        {
            //Arrange
            _uow.Setup(x => x.AlbumRepository.InsertGraph(_album));
            _uow.Setup(x => x.SaveChanges());

            //Act
            _sut.CreateAlbum(_album);

            //Assert
            _mockRepo.VerifyAll();
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
            _mockRepo.VerifyAll();
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
            _mockRepo.VerifyAll();
        }

        [Test]
        public void UpdateAndSave_OnUpdateAlbum()
        {
            //Arrange
            _album.Id = 144;
            _album.UserId = _userId;
            _uow.Setup(x => x.AlbumRepository.GetById(_album.Id))
                .Returns(_album);
            _uow.Setup(x => x.SaveChanges());

            var newName = "new name";

            //Act
            _sut.UpdateAlbum(_album.Id, newName, _userId);

            //Assert
            Assert.AreEqual(newName, _album.Name);

            _mockRepo.VerifyAll();

        }

        [Test]
        public void ThrowNotFoundIfAlbumNotFOund_OnDeleteAlbum()
        {
            //Arrange
            var albumId = 22;
            _uow.Setup(x => x.AlbumRepository.GetById(albumId, true))
                .Returns(() => null);

            //Act
            Assert.Throws<NotFoundException>(() => _sut.DeleteAlbum(albumId, _userId));

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void ThrowIfTheCurrentUserIsNotOwner_OnDeleteAlbum()
        {
            //Arrange
            _album.Id = 123;
            _album.UserId = 400;
            _uow.Setup(x => x.AlbumRepository.GetById(_album.Id, true))
                .Returns(_album);

            //Act & Assert
            Assert.Throws<SecurityException>(() => _sut.DeleteAlbum(_album.Id, _userId));
            _mockRepo.VerifyAll();
        }

        [Test]
        public void DeleteAndSave_OnDeleteAlbum()
        {
            //Arrange
            var photoIds = new[] { 1, 2, 3, 4 };

            foreach (var id in photoIds)
            {
                _album.Photos.Add(new Photo { Id = id });
            }

            _album.Id = 123;
            _album.UserId = _userId;
            _uow.Setup(x => x.AlbumRepository.GetById(_album.Id, true))
                .Returns(_album);
            _uow.Setup(x => x.AlbumRepository.Remove(_album));
            _photoService.Setup(x => x.RemovePhoto(It.IsInRange(1, 4, Range.Inclusive), _userId));
            _uow.Setup(x => x.SaveChanges());

            //Act
            _sut.DeleteAlbum(_album.Id, _userId);

            //Assert
            _mockRepo.VerifyAll();
        }
    }
}