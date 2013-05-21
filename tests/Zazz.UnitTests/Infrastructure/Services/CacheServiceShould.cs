using Moq;
using NUnit.Framework;
using Zazz.Core.Interfaces;
using Zazz.Core.Models;
using Zazz.Infrastructure.Services;

namespace Zazz.UnitTests.Infrastructure.Services
{
    [TestFixture]
    public class CacheServiceShould
    {
        private Mock<ICacheSystem<string, int>> _userIdCache;
        private Mock<ICacheSystem<int, string>> _displayNameCache;
        private Mock<ICacheSystem<int, PhotoLinks>> _photoUrlCache;
        private CacheService _sut;
        private string _username;
        private int _userId;
        private string _displayName;
        private PhotoLinks _photoUrl;
        private Mock<ICacheSystem<int, byte[]>> _passwordCache;
        private byte[] _password;

        [SetUp]
        public void Init()
        {
            _userIdCache = new Mock<ICacheSystem<string, int>>(MockBehavior.Strict);
            _displayNameCache = new Mock<ICacheSystem<int, string>>(MockBehavior.Strict);
            _photoUrlCache = new Mock<ICacheSystem<int, PhotoLinks>>(MockBehavior.Strict);
            _passwordCache = new Mock<ICacheSystem<int, byte[]>>(MockBehavior.Strict);

            _sut = new CacheService();
            CacheService.UserIdCache = _userIdCache.Object;
            CacheService.DisplayNameCache = _displayNameCache.Object;
            CacheService.PhotoUrlCache = _photoUrlCache.Object;
            CacheService.PasswordCache = _passwordCache.Object;

            _username = "soroush";
            _userId = 1;
            _displayName = "display name";
            _password = new byte[] {1, 2, 3};
            _photoUrl = new PhotoLinks
                        {
                            OriginalLink = "photo",
                            VerySmallLink = "vs",
                            MediumLink = "m",
                            SmallLink = "s"
                        };
        }

        [Test]
        public void AddToUserIdCache_OnAddUserId()
        {
            //Arrange
            _userIdCache.Setup(x => x.Add(_username, _userId));

            //Act
            _sut.AddUserId(_username, _userId);

            //Assert
            _userIdCache.Verify(x => x.Add(_username, _userId), Times.Once());
        }

        [Test]
        public void ReturnValueFromUserIdCache_OnGetUserId()
        {
            //Arrange
            _userIdCache.Setup(x => x.TryGet(_username))
                        .Returns(_userId);

            //Act
            var result = _sut.GetUserId(_username);

            //Assert
            Assert.AreEqual(_userId, result);
            _userIdCache.Verify(x => x.TryGet(_username), Times.Once());
        }

        [Test]
        public void AddToDisplayNameCache_OnAddDisplayName()
        {
            //Arrange
            _displayNameCache.Setup(x => x.Add(_userId, _displayName));

            //Act
            _sut.AddUserDiplayName(_userId, _displayName);

            //Assert
            _displayNameCache.Verify(x => x.Add(_userId, _displayName), Times.Once());
        }

        [Test]
        public void ReturnValueFromDisplayNameCache_OnAddDisplayName()
        {
            //_displayNameCache
            _displayNameCache.Setup(x => x.TryGet(_userId))
                        .Returns(_displayName);

            //Act
            var result = _sut.GetUserDisplayName(_userId);

            //Assert
            Assert.AreEqual(_displayName, result);
            _displayNameCache.Verify(x => x.TryGet(_userId), Times.Once());
        }

        [Test]
        public void AddToPhotoUrlCache_OnAddPhotoUrl()
        {
            //Arrange
            _photoUrlCache.Setup(x => x.Add(_userId, _photoUrl));

            //Act
            _sut.AddUserPhotoUrl(_userId, _photoUrl);

            //Assert
            _photoUrlCache.Verify(x => x.Add(_userId, _photoUrl), Times.Once());
        }

        [Test]
        public void ReturnValueFromPhotoUrlCache_OnAddPhotoUrl()
        {
            //_displayNameCache
            _photoUrlCache.Setup(x => x.TryGet(_userId))
                        .Returns(_photoUrl);

            //Act
            var result = _sut.GetUserPhotoUrl(_userId);

            //Assert
            Assert.AreEqual(_photoUrl.MediumLink, result.MediumLink);
            Assert.AreEqual(_photoUrl.OriginalLink, result.OriginalLink);
            Assert.AreEqual(_photoUrl.SmallLink, result.SmallLink);
            Assert.AreEqual(_photoUrl.VerySmallLink, result.VerySmallLink);
            _photoUrlCache.Verify(x => x.TryGet(_userId), Times.Once());
        }

        [Test]
        public void AddToPasswordCache_OnAddPassword()
        {
            //Arrange
            _passwordCache.Setup(x => x.Add(_userId, _password));

            //Act
            _sut.AddUserPassword(_userId, _password);

            //Assert
            _passwordCache.Verify(x => x.Add(_userId, _password), Times.Once());
        }

        [Test]
        public void ReturnValueFromPasswordCache_OnGetPassword()
        {
            //Arrange
            _passwordCache.Setup(x => x.TryGet(_userId))
                          .Returns(_password);

            //Act
            var userPassword = _sut.GetUserPassword(_userId);

            //Assert
            CollectionAssert.AreEqual(_password, userPassword);
            _passwordCache.Verify(x => x.TryGet(_userId), Times.Once());
        }

        [Test]
        public void ShouldRemoveUserDisplayName_OnRemoveUserDisplayName()
        {
            //Arrange
            _photoUrlCache.Setup(x => x.Remove(_userId));
            _displayNameCache.Setup(x => x.Remove(_userId));

            //Act
            _sut.RemoveUserDisplayName(_userId);

            //Assert
            _userIdCache.Verify(x => x.Remove(_username), Times.Never());
            _photoUrlCache.Verify(x => x.Remove(_userId), Times.Never());
            _displayNameCache.Verify(x => x.Remove(_userId), Times.Once());
        }

        [Test]
        public void ShouldRemoveUserPhotoUrl_OnRemoveUserPhotoUrl()
        {
            //Arrange
            _photoUrlCache.Setup(x => x.Remove(_userId));
            _displayNameCache.Setup(x => x.Remove(_userId));

            //Act
            _sut.RemoveUserPhotoUrl(_userId);

            //Assert
            _userIdCache.Verify(x => x.Remove(_username), Times.Never());
            _photoUrlCache.Verify(x => x.Remove(_userId), Times.Once());
            _displayNameCache.Verify(x => x.Remove(_userId), Times.Never());
        }
    }
}