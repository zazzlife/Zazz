using System;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Zazz.Core.Exceptions;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Services;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;
using Zazz.Infrastructure.Services;

namespace Zazz.UnitTests.Infrastructure.Services
{
    [TestFixture]
    public class UserServiceShould
    {
        private Mock<IUoW> _uow;
        private UserService _sut;
        private Mock<ICacheService> _cacheService;
        private Mock<ICryptoService> _cryptoService;
        private Mock<IPhotoService> _photoService;
        private MockRepository _mockRepo;

        [SetUp]
        public void Init()
        {
            _mockRepo = new MockRepository(MockBehavior.Strict);

            _cacheService = _mockRepo.Create<ICacheService>();
            _cryptoService = _mockRepo.Create<ICryptoService>();
            _uow = _mockRepo.Create<IUoW>();
            _photoService = _mockRepo.Create<IPhotoService>();
            _sut = new UserService(_uow.Object, _cacheService.Object, _cryptoService.Object, _photoService.Object);
        }

        [Test]
        public void CallGetIdByUserNameWhenItNotExistsInCacheThenAddItToCache_OnGetUserId()
        {
            //Arrange
            var username = "soroush";
            var id = 12;
            _uow.Setup(x => x.UserRepository.GetIdByUsername(username))
                .Returns(id);
            _cacheService.Setup(x => x.GetUserId(username))
                         .Returns(0);
            _cacheService.Setup(x => x.AddUserId(username, id));

            //Act
            var result = _sut.GetUserId(username);

            //Assert
            Assert.AreEqual(id, result);
            _mockRepo.VerifyAll();
        }

        [Test]
        public void ReturnUserIdFromCacheIfExists_OnGetUserId()
        {
            //Arrange
            var username = "soroush";
            var id = 12;
            _cacheService.Setup(x => x.GetUserId(username))
                         .Returns(id);

            //Act
            var result = _sut.GetUserId(username);

            //Assert
            Assert.AreEqual(id, result);
            _mockRepo.VerifyAll();
        }

        [Test]
        public void ThrowIfUserWasNotFound_OnGetUser()
        {
            //Arrange
            var username = "soroush";
            _uow.Setup(x => x.UserRepository.GetByUsername(username, false, false, false, false))
                .Returns(() => null);

            //Act & Assert
            Assert.Throws<NotFoundException>(() => _sut.GetUser(username));
            _mockRepo.VerifyAll();
        }

        [Test]
        public void CallGetByUsername_OnGetUser()
        {
            //Arrange
            var username = "soroush";
            var user = new User();
            _uow.Setup(x => x.UserRepository.GetByUsername(username,
                It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns(user);

            //Act
            var result = _sut.GetUser(username);

            //Assert
            Assert.AreSame(user, result);
            _mockRepo.VerifyAll();

        }

        [Test]
        public void ThrowIfUserWasNotFound_OnGetUserById()
        {
            //Arrange
            var userId = 22;
            _uow.Setup(x => x.UserRepository.GetById(userId, false, false, false, false))
                .Returns(() => null);

            //Act & Assert
            Assert.Throws<NotFoundException>(() => _sut.GetUser(userId));
            _mockRepo.VerifyAll();
        }

        [Test]
        public void ReturnGetUserAccountTypeFromRepo_OnGetUserAccountType()
        {
            //Arrange
            var id = 123;
            var accountType = AccountType.User;
            _uow.Setup(x => x.UserRepository.GetUserAccountType(id))
                .Returns(accountType);

            //Act
            var result = _sut.GetAccountType(id);

            //Assert
            Assert.AreEqual(accountType, result);
            _mockRepo.VerifyAll();
        }

        [Test]
        public void ReturnCachedDisplayNameIfExists_OnGetUserDisplayNameWithId()
        {
            //Arrange
            var user = new User { Id = 12, Username = "username", UserDetail = new UserDetail { FullName = "Full name" } };

            _cacheService.Setup(x => x.GetUserDisplayName(user.Id))
                         .Returns(user.UserDetail.FullName);

            //Act
            var result = _sut.GetUserDisplayName(user.Id);

            //Assert
            Assert.AreEqual(user.UserDetail.FullName, result);
            _mockRepo.VerifyAll();
        }

        [Test]
        public void ReturnCachedDisplayNameIfExists_OnGetUserDisplayNameWithUsername()
        {
            //Arrange
            var user = new User { Id = 12, Username = "username", UserDetail = new UserDetail { FullName = "Full name" } };

            _cacheService.Setup(x => x.GetUserId(user.Username))
                         .Returns(user.Id);

            _cacheService.Setup(x => x.GetUserDisplayName(user.Id))
                         .Returns(user.UserDetail.FullName);

            //Act
            var result = _sut.GetUserDisplayName(user.Username);

            //Assert
            Assert.AreEqual(user.UserDetail.FullName, result);
            _mockRepo.VerifyAll();
        }

        [Test]
        public void GetDisplayNameAndAddToCache_OnGetUserDisplayNameWithUserId()
        {
            //Arrange
            var user = new User { Id = 12, Username = "username", UserDetail = new UserDetail { FullName = "Full name" } };
            var displayName = "displayName";

            _uow.Setup(x => x.UserRepository.GetDisplayName(user.Id))
                .Returns(displayName);

            _cacheService.Setup(x => x.GetUserDisplayName(user.Id))
                         .Returns(() => null);
            _cacheService.Setup(x => x.AddUserDiplayName(user.Id, displayName));

            //Act
            var result = _sut.GetUserDisplayName(user.Id);

            //Assert
            Assert.AreEqual(displayName, result);
            _mockRepo.VerifyAll();
        }

        [Test]
        public void ThrowIfUserDoesntExists_OnGetUserDisplayNameWithUserId()
        {
            //Arrange
            var user = new User { Id = 12, Username = "username", UserDetail = new UserDetail { FullName = "Full name" } };
            var displayName = "displayName";

            _uow.Setup(x => x.UserRepository.GetDisplayName(user.Id))
                .Returns(() => null);

            _cacheService.Setup(x => x.GetUserDisplayName(user.Id))
                         .Returns(() => null);

            //Act
            Assert.Throws<NotFoundException>(() => _sut.GetUserDisplayName(user.Id));

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void GetDisplayNameAndAddToCache_OnGetUserDisplayNameWithUsername()
        {
            //Arrange
            var user = new User { Id = 12, Username = "username", UserDetail = new UserDetail { FullName = "Full name" } };
            var displayName = "displayName";

            _uow.Setup(x => x.UserRepository.GetDisplayName(user.Id))
                .Returns(displayName);

            _cacheService.Setup(x => x.GetUserId(user.Username))
                         .Returns(user.Id);

            _cacheService.Setup(x => x.GetUserDisplayName(user.Id))
                         .Returns(() => null);
            _cacheService.Setup(x => x.AddUserDiplayName(user.Id, displayName));

            //Act
            var result = _sut.GetUserDisplayName(user.Username);

            //Assert
            Assert.AreEqual(displayName, result);
            _mockRepo.VerifyAll();
        }

        [Test]
        public void ThrowIfUserDoesntExists_OnGetUserDisplayNameWithUsername()
        {
            //Arrange
            var user = new User { Id = 12, Username = "username", UserDetail = new UserDetail { FullName = "Full name" } };
            var displayName = "displayName";

            _uow.Setup(x => x.UserRepository.GetDisplayName(user.Id))
                .Returns(() => null);

            _cacheService.Setup(x => x.GetUserId(user.Username))
                         .Returns(user.Id);

            _cacheService.Setup(x => x.GetUserDisplayName(user.Id))
                         .Returns(() => null);

            //Act
            Assert.Throws<NotFoundException>(() => _sut.GetUserDisplayName(user.Username));

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void ReturnCorrectValue_OnOAuthAccountExists()
        {
            //Arrange
            var userId = 12;
            var provider = OAuthProvider.Facebook;

            _uow.Setup(x => x.LinkedAccountRepository.Exists(userId, provider))
                .Returns(true);

            //Act
            var result = _sut.OAuthAccountExists(userId, provider);

            //Assert
            Assert.IsTrue(result);
            _uow.VerifyAll();
        }

        [Test]
        public void ThrowIfUserNotExists_OnChangeProfilePic()
        {
            //Arrange
            var user = new User
            {
                Id = 32,
                ProfilePhotoId = 222
            };

            var photo = new Photo
            {
                Id = 232,
                UserId = 50
            };

            _uow.Setup(x => x.UserRepository.GetById(user.Id, false, false, false, false))
                .Returns(() => null);

            //Act
            Assert.Throws<NotFoundException>(() => _sut.ChangeProfilePic(user.Id, photo.Id));

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void SetUserProfilePicToNul_OnChangeProfilePic()
        {
            //Arrange
            var user = new User
            {
                Id = 32,
                ProfilePhotoId = 222
            };

            _uow.Setup(x => x.UserRepository.GetById(user.Id, false, false, false, false))
                .Returns(user);

            _uow.Setup(x => x.SaveChanges());

            //Act
            _sut.ChangeProfilePic(user.Id, null);

            //Assert
            Assert.AreEqual(null, user.ProfilePhotoId);
            _mockRepo.VerifyAll();
        }

        [Test]
        public void ThrowIfPhotoNotExists_OnChangeProfilePic()
        {
            //Arrange
            var user = new User
            {
                Id = 32,
                ProfilePhotoId = 222
            };

            var photo = new Photo
            {
                Id = 232,
                UserId = 32
            };

            _uow.Setup(x => x.PhotoRepository.GetById(photo.Id))
                .Returns(() => null);

            _uow.Setup(x => x.UserRepository.GetById(user.Id, false, false, false, false))
                .Returns(user);

            //Act
            Assert.Throws<NotFoundException>(() => _sut.ChangeProfilePic(user.Id, photo.Id));

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void ThrowIfPhotoIsNotFromCurrentUser_OnChangeProfilePic()
        {
            //Arrange
            var user = new User
                       {
                           Id = 32,
                           ProfilePhotoId = 222
                       };

            var photo = new Photo
                        {
                            Id = 232,
                            UserId = 50
                        };

            _uow.Setup(x => x.PhotoRepository.GetById(photo.Id))
                .Returns(photo);

            _uow.Setup(x => x.UserRepository.GetById(user.Id, false, false, false, false))
                .Returns(user);

            //Act
            Assert.Throws<SecurityException>(() => _sut.ChangeProfilePic(user.Id, photo.Id));

            //Assert
            _mockRepo.VerifyAll();
        }


    }
}