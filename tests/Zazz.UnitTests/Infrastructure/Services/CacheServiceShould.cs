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
        private MockRepository _mockRepo;

        [SetUp]
        public void Init()
        {
            _mockRepo = new MockRepository(MockBehavior.Strict);

            _userIdCache = _mockRepo.Create<ICacheSystem<string, int>>();
            _displayNameCache = _mockRepo.Create<ICacheSystem<int, string>>();
            _photoUrlCache = _mockRepo.Create<ICacheSystem<int, PhotoLinks>>();

            _sut = new CacheService();
            CacheService.UserIdCache = _userIdCache.Object;
            CacheService.DisplayNameCache = _displayNameCache.Object;
            CacheService.PhotoUrlCache = _photoUrlCache.Object;

            _username = "soroush";
            _userId = 1;
            _displayName = "display name";
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
            _mockRepo.VerifyAll();
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
            _mockRepo.VerifyAll();
        }

        [Test]
        public void AddToDisplayNameCache_OnAddDisplayName()
        {
            //Arrange
            _displayNameCache.Setup(x => x.Add(_userId, _displayName));

            //Act
            _sut.AddUserDiplayName(_userId, _displayName);

            //Assert
            _mockRepo.VerifyAll();
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
            _mockRepo.VerifyAll();
        }

        [Test]
        public void AddToPhotoUrlCache_OnAddPhotoUrl()
        {
            //Arrange
            _photoUrlCache.Setup(x => x.Add(_userId, _photoUrl));

            //Act
            _sut.AddUserPhotoUrl(_userId, _photoUrl);

            //Assert
            _mockRepo.VerifyAll();
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
            _mockRepo.VerifyAll();
        }

        [Test]
        public void ShouldRemoveUserDisplayName_OnRemoveUserDisplayName()
        {
            //Arrange
            _displayNameCache.Setup(x => x.Remove(_userId));

            //Act
            _sut.RemoveUserDisplayName(_userId);

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void ShouldRemoveUserPhotoUrl_OnRemoveUserPhotoUrl()
        {
            //Arrange
            _photoUrlCache.Setup(x => x.Remove(_userId));

            //Act
            _sut.RemoveUserPhotoUrl(_userId);

            //Assert
            _mockRepo.VerifyAll();
        }
        
    }
}