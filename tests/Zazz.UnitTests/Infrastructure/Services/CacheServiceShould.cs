using Moq;
using NUnit.Framework;
using Zazz.Core.Interfaces;
using Zazz.Infrastructure.Services;

namespace Zazz.UnitTests.Infrastructure.Services
{
    [TestFixture]
    public class CacheServiceShould
    {
        private Mock<ICacheSystem<string, int>> _userIdCache;
        private Mock<ICacheSystem<int, string>> _displayNameCache;
        private Mock<ICacheSystem<int, string>> _photoUrlCache;
        private CacheService _sut;
        private string _username;
        private int _userId;
        private string _displayName;
        private string _photoUrl;

        [SetUp]
        public void Init()
        {
            _userIdCache = new Mock<ICacheSystem<string, int>>();
            _displayNameCache = new Mock<ICacheSystem<int, string>>();
            _photoUrlCache = new Mock<ICacheSystem<int, string>>();

            _sut = new CacheService(_userIdCache.Object, _displayNameCache.Object, _photoUrlCache.Object);

            _username = "soroush";
            _userId = 1;
            _displayName = "display name";
            _photoUrl = "photo";
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
            Assert.AreEqual(_photoUrl, result);
            _photoUrlCache.Verify(x => x.TryGet(_userId), Times.Once());
        }

        [Test]
        public void ShouldRemoveToAllCachingSystems_OnRemoveUserCache()
        {
            //Arrange
            _userIdCache.Setup(x => x.Remove(_username));
            _photoUrlCache.Setup(x => x.Remove(_userId));
            _displayNameCache.Setup(x => x.Remove(_userId));

            //Act
            _sut.RemoveUserCache(_username, _userId);

            //Assert
            _userIdCache.Verify(x => x.Remove(_username), Times.Once());
            _photoUrlCache.Verify(x => x.Remove(_userId), Times.Once());
            _displayNameCache.Verify(x => x.Remove(_userId), Times.Once());
        }


    }
}