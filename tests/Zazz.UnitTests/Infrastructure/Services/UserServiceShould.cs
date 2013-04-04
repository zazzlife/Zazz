using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Infrastructure.Services;

namespace Zazz.UnitTests.Infrastructure.Services
{
    [TestFixture]
    public class UserServiceShould
    {
        private Mock<IUoW> _uow;
        private UserService _sut;
        private Mock<ICacheService> _cacheService;

        [SetUp]
        public void Init()
        {
            _cacheService = new Mock<ICacheService>();
            _uow = new Mock<IUoW>();
            _sut = new UserService(_uow.Object, _cacheService.Object);
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
            _uow.Verify(x => x.UserRepository.GetIdByUsername(username), Times.Once());
            _cacheService.Verify(x => x.GetUserId(username), Times.Once());
            _cacheService.Verify(x => x.AddUserId(username, id), Times.Once());
        }

        [Test]
        public void ReturnUserIdFromCacheIfExists_OnGetUserId()
        {
            //Arrange
            var username = "soroush";
            var id = 12;
            _uow.Setup(x => x.UserRepository.GetIdByUsername(username))
                .Returns(id);
            _cacheService.Setup(x => x.GetUserId(username))
                         .Returns(id);
            _cacheService.Setup(x => x.AddUserId(username, id));

            //Act
            var result = _sut.GetUserId(username);

            //Assert
            Assert.AreEqual(id, result);
            _uow.Verify(x => x.UserRepository.GetIdByUsername(It.IsAny<string>()), Times.Never());
            _cacheService.Verify(x => x.GetUserId(username), Times.Once());
            _cacheService.Verify(x => x.AddUserId(username, id), Times.Never());
        }

        [Test]
        public async Task CallGetByUsername_OnGetUser()
        {
            //Arrange
            var username = "soroush";
            var user = new User();
            _uow.Setup(x => x.UserRepository.GetByUsernameAsync(username))
                .Returns(() => Task.Run(() => user));


            //Act
            var result = await _sut.GetUserAsync(username);

            //Assert
            Assert.AreSame(user, result);
            _uow.Verify(x => x.UserRepository.GetByUsernameAsync(username), Times.Once());

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
            var result = _sut.GetUserAccountType(id);

            //Assert
            Assert.AreEqual(accountType, result);
            _uow.Verify(x => x.UserRepository.GetUserAccountType(id), Times.Once());
        }

        [Test]
        public void ReturnCachedDisplayNameIfExists_OnGetUserDisplayNameWithId()
        {
            //Arrange
            var user = new User { Id = 12, Username = "username", UserDetail = new UserDetail { FullName = "Full name" } };

            _uow.Setup(x => x.UserRepository.GetUserFullName(user.Id))
                .Returns(user.UserDetail.FullName);
            _uow.Setup(x => x.UserRepository.GetUserName(user.Id))
                .Returns(user.Username);
            _cacheService.Setup(x => x.GetUserDisplayName(user.Id))
                         .Returns(user.UserDetail.FullName);
            _cacheService.Setup(x => x.AddUserDiplayName(user.Id, user.UserDetail.FullName));

            //Act
            var result = _sut.GetUserDisplayName(user.Id);

            //Assert
            Assert.AreEqual(user.UserDetail.FullName, result);
            _uow.Verify(x => x.UserRepository.GetUserFullName(It.IsAny<int>()), Times.Never());
            _uow.Verify(x => x.UserRepository.GetUserName(It.IsAny<int>()), Times.Never());
            _cacheService.Verify(x => x.GetUserDisplayName(user.Id), Times.Once());
            _cacheService.Verify(x => x.AddUserDiplayName(It.IsAny<int>(), It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void ReturnCachedDisplayNameIfExists_OnGetUserDisplayNameWithUsername()
        {
            //Arrange
            var user = new User { Id = 12, Username = "username", UserDetail = new UserDetail { FullName = "Full name" } };

            _uow.Setup(x => x.UserRepository.GetUserFullName(user.Id))
                .Returns(user.UserDetail.FullName);
            _uow.Setup(x => x.UserRepository.GetUserName(user.Id))
                .Returns(user.Username);
            _cacheService.Setup(x => x.GetUserDisplayName(user.Id))
                         .Returns(user.UserDetail.FullName);
            _cacheService.Setup(x => x.AddUserDiplayName(user.Id, user.UserDetail.FullName));
            _uow.Setup(x => x.UserRepository.GetIdByUsername(user.Username))
                .Returns(user.Id);

            //Act
            var result = _sut.GetUserDisplayName(user.Username);

            //Assert
            Assert.AreEqual(user.UserDetail.FullName, result);
            _uow.Verify(x => x.UserRepository.GetIdByUsername(user.Username),Times.Once());
            _uow.Verify(x => x.UserRepository.GetUserFullName(It.IsAny<int>()), Times.Never());
            _uow.Verify(x => x.UserRepository.GetUserName(It.IsAny<int>()), Times.Never());
            _cacheService.Verify(x => x.GetUserDisplayName(user.Id), Times.Once());
            _cacheService.Verify(x => x.AddUserDiplayName(It.IsAny<int>(), It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void ReturnFullNameWhenIsNotEmptyAndAddToCacheWhenNotExists_OnGetUserDisplayNameWithId()
        {
            //Arrange
            var user = new User { Id = 12, Username = "username", UserDetail = new UserDetail { FullName = "Full name" } };

            _uow.Setup(x => x.UserRepository.GetUserFullName(user.Id))
                .Returns(user.UserDetail.FullName);
            _uow.Setup(x => x.UserRepository.GetUserName(user.Id))
                .Returns(user.Username);
            _cacheService.Setup(x => x.GetUserDisplayName(user.Id))
                         .Returns(() => null);
            _cacheService.Setup(x => x.AddUserDiplayName(user.Id, user.UserDetail.FullName));

            //Act
            var result = _sut.GetUserDisplayName(user.Id);

            //Assert
            Assert.AreEqual(user.UserDetail.FullName, result);
            _uow.Verify(x => x.UserRepository.GetUserFullName(user.Id), Times.Once());
            _uow.Verify(x => x.UserRepository.GetUserName(It.IsAny<int>()), Times.Never());
            _cacheService.Verify(x => x.GetUserDisplayName(user.Id), Times.Once());
            _cacheService.Verify(x => x.AddUserDiplayName(user.Id, user.UserDetail.FullName), Times.Once());
        }

        [Test]
        public void ReturnUserNameWhenFullNameIsEmptyAddToCacheWhenNotExists_OnGetUserDisplayNameWithId()
        {
            //Arrange
            var user = new User { Id = 12, Username = "username", UserDetail = new UserDetail { FullName = "Full name" } };

            _uow.Setup(x => x.UserRepository.GetUserFullName(user.Id))
                .Returns(() => null);
            _uow.Setup(x => x.UserRepository.GetUserName(user.Id))
                .Returns(user.Username);
            _cacheService.Setup(x => x.GetUserDisplayName(user.Id))
                         .Returns(() => null);
            _cacheService.Setup(x => x.AddUserDiplayName(user.Id, user.Username));

            //Act
            var result = _sut.GetUserDisplayName(user.Id);

            //Assert
            Assert.AreEqual(user.Username, result);
            _uow.Verify(x => x.UserRepository.GetUserFullName(user.Id), Times.Once());
            _uow.Verify(x => x.UserRepository.GetUserName(user.Id), Times.Once());
            _cacheService.Verify(x => x.GetUserDisplayName(user.Id), Times.Once());
            _cacheService.Verify(x => x.AddUserDiplayName(user.Id, user.Username), Times.Once());
        }

        [Test]
        public void ReturnFullNameWhenIsNotEmptyAddToCacheWhenNotExists_OnGetUserDisplayNameWithUsername()
        {
            //Arrange
            var user = new User { Id = 12, Username = "username", UserDetail = new UserDetail { FullName = "Full name" } };

            _uow.Setup(x => x.UserRepository.GetIdByUsername(user.Username))
                .Returns(user.Id);
            _uow.Setup(x => x.UserRepository.GetUserName(user.Id))
                .Returns(user.Username);
            _uow.Setup(x => x.UserRepository.GetUserFullName(user.Id))
                .Returns(user.UserDetail.FullName);
            _cacheService.Setup(x => x.GetUserDisplayName(user.Id))
                         .Returns(() => null);
            _cacheService.Setup(x => x.AddUserDiplayName(user.Id, user.UserDetail.FullName));

            //Act
            var result = _sut.GetUserDisplayName(user.Username);

            //Assert
            Assert.AreEqual(user.UserDetail.FullName, result);
            _uow.Verify(x => x.UserRepository.GetIdByUsername(user.Username), Times.Once());
            _uow.Verify(x => x.UserRepository.GetUserFullName(user.Id), Times.Once());
            _uow.Verify(x => x.UserRepository.GetUserName(It.IsAny<int>()), Times.Never());
            _cacheService.Verify(x => x.GetUserDisplayName(user.Id), Times.Once());
            _cacheService.Verify(x => x.AddUserDiplayName(user.Id, user.UserDetail.FullName), Times.Once());
        }

        [Test]
        public void ReturnUserNameWhenFullNameIsEmptyAddToCacheWhenNotExists_OnGetUserDisplayNameWithUsername()
        {
            //Arrange
            var user = new User { Id = 12, Username = "username", UserDetail = new UserDetail { FullName = "Full name" } };

            _uow.Setup(x => x.UserRepository.GetUserFullName(user.Id))
                .Returns(() => null);
            _uow.Setup(x => x.UserRepository.GetUserName(user.Id))
                .Returns(user.Username);
            _uow.Setup(x => x.UserRepository.GetIdByUsername(user.Username))
                .Returns(user.Id);
            _cacheService.Setup(x => x.GetUserDisplayName(user.Id))
                         .Returns(() => null);
            _cacheService.Setup(x => x.AddUserDiplayName(user.Id, user.Username));

            //Act
            var result = _sut.GetUserDisplayName(user.Username);

            //Assert
            Assert.AreEqual(user.Username, result);
            _uow.Verify(x => x.UserRepository.GetIdByUsername(user.Username), Times.Once());
            _uow.Verify(x => x.UserRepository.GetUserFullName(user.Id), Times.Once());
            _uow.Verify(x => x.UserRepository.GetUserName(user.Id), Times.Once());
            _cacheService.Verify(x => x.GetUserDisplayName(user.Id), Times.Once());
            _cacheService.Verify(x => x.AddUserDiplayName(user.Id, user.Username), Times.Once());
        }
    }
}