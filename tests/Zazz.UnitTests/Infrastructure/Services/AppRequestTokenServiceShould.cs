using System;
using System.Text;
using Moq;
using NUnit.Framework;
using Zazz.Core.Exceptions;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Infrastructure.Services;

namespace Zazz.UnitTests.Infrastructure.Services
{
    [TestFixture]
    public class AppRequestTokenServiceShould
    {
        private MockRepository _mockRepo;
        private Mock<IUoW> _uow;
        private Mock<ICryptoService> _cryptoService;
        private AppRequestTokenService _sut;
        private int _appId;

        [SetUp]
        public void Init()
        {
            _mockRepo = new MockRepository(MockBehavior.Strict);
            _uow = _mockRepo.Create<IUoW>();
            _cryptoService = _mockRepo.Create<ICryptoService>();

            _sut = new AppRequestTokenService(_uow.Object, _cryptoService.Object);
            _appId = 1;
        }

        [Test]
        public void ThrowIfAppIdIsInvalid_OnCreate()
        {
            //Arrange
            //Act
            Assert.Throws<ArgumentOutOfRangeException>(() => _sut.Create(0));

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void CreateANewTokenAndItShouldExpireIn1Hour_OnCreate()
        {
            //Arrange
            var token = "token";
            var tokenBuffer = Encoding.UTF8.GetBytes(token);

            _cryptoService.Setup(x => x.GenerateKey(64 * 8, false))
                          .Returns(tokenBuffer);

            _uow.Setup(x => x.AppRequestTokenRepository
                             .InsertGraph(It.Is<AppRequestToken>(a => a.AppId == _appId &&
                                                                      a.ExpirationTime > DateTime.UtcNow &&
                                                                      a.ExpirationTime < DateTime.UtcNow.AddHours(2) &&
                                                                      a.Token.Equals(tokenBuffer))));

            _uow.Setup(x => x.SaveChanges());

            //Act
            var result = _sut.Create(_appId);

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void ThrowIfRequestIdIsInvalid_OnGet()
        {
            //Arrange
            var requestId = 0L;

            //Act
            Assert.Throws<ArgumentOutOfRangeException>(() => _sut.Get(requestId));

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void ThrowNotFoundIfRequestWasNotFound_OnGet()
        {
            //Arrange
            var requestId = 44444L;
            _uow.Setup(x => x.AppRequestTokenRepository.GetById(requestId))
                .Returns(() => null);


            //Act
            Assert.Throws<NotFoundException>(() => _sut.Get(requestId));

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void ReturnToken_OnGet()
        {
            //Arrange
            var requestId = 44444L;
            var token = new AppRequestToken
                        {
                            AppId = _appId,
                            ExpirationTime = DateTime.UtcNow.AddDays(1),
                            Id = requestId,
                            Token = new byte[1]
                        };

            _uow.Setup(x => x.AppRequestTokenRepository.GetById(requestId))
                .Returns(token);


            //Act
            var result = _sut.Get(requestId);

            //Assert
            Assert.AreSame(token, result);

            _mockRepo.VerifyAll();
        }

        [Test]
        public void ThrowIfRequestIdIsInvalid_OnRemove()
        {
            //Arrange
            var requestId = 0L;

            //Act
            Assert.Throws<ArgumentOutOfRangeException>(() => _sut.Remove(requestId));

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void CallRemoveOnRepoAndSave_OnRemove()
        {
            //Arrange
            var requestId = 143223L;
            _uow.Setup(x => x.AppRequestTokenRepository.Remove(requestId));
            _uow.Setup(x => x.SaveChanges());

            //Act
            _sut.Remove(requestId);

            //Assert
            _mockRepo.VerifyAll();
        }
    }
}