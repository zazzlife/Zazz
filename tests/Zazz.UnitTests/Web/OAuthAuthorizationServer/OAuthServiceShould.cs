using System;
using System.Linq;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Web.OAuthAuthorizationServer;

namespace Zazz.UnitTests.Web.OAuthAuthorizationServer
{
    [TestFixture]
    public class OAuthServiceShould
    {
        private MockRepository _mocRepo;
        private Mock<IUoW> _uow;
        private OAuthService _sut;
        private Mock<ICryptoService> _cryptoService;

        [SetUp]
        public void Init()
        {
            _mocRepo = new MockRepository(MockBehavior.Strict);
            _uow = _mocRepo.Create<IUoW>();
            _cryptoService = _mocRepo.Create<ICryptoService>();

            _sut = new OAuthService(_uow.Object, _cryptoService.Object);
        }

        [Test]
        public void CreateAndSaveNewRefreshToken_OnCreateOAuthCredentials()
        {
            //Arrange
            var user = new User
                       {
                           Id = 22,
                       };

            var client = new OAuthClient { Id = 444 };
            var scopes = new List<OAuthScope> { new OAuthScope { Id = 1, Name = "full" } };

            var verifyCodeBytes = new byte[] {1, 2, 3, 4, 5, 6};
            var verifyCode = Convert.ToBase64String(verifyCodeBytes);

            _cryptoService.Setup(x => x.GenerateKey(1024, false))
                          .Returns(verifyCodeBytes);

            _uow.Setup(x => x.OAuthRefreshTokenRepository.InsertGraph(It.Is<OAuthRefreshToken>(
                t => t.CreatedDate.Date == DateTime.UtcNow.Date &&
                     t.OAuthClientId == client.Id &&
                     t.UserId == user.Id &&
                     t.VerificationCode == verifyCode)));

            _uow.Setup(x => x.SaveChanges());

            //Act
            var result = _sut.CreateOAuthCredentials(user, client, scopes);
            
            //Assert
            var accessToken = result.AccessToken;
            var refreshToken = result.RefreshToken;

            Assert.AreEqual(client.Id, accessToken.ClientId);
            Assert.AreEqual(user.Id, accessToken.UserId);
            Assert.IsTrue(accessToken.ExpirationDate > DateTime.UtcNow &&
                          accessToken.ExpirationDate < DateTime.UtcNow.AddHours(2));
            Assert.AreEqual(JWT.ACCESS_TOKEN_TYPE, accessToken.TokenType);
            CollectionAssert.AreEqual(scopes.Select(s => s.Name).ToList(), accessToken.Scopes);


            Assert.IsNotNull(refreshToken.TokenId);
            Assert.AreEqual(client.Id, refreshToken.ClientId);
            Assert.AreEqual(user.Id, refreshToken.UserId);
            Assert.AreEqual(JWT.REFRESH_TOKEN_TYPE, refreshToken.TokenType);
            CollectionAssert.AreEqual(scopes.Select(s => s.Name).ToList(), refreshToken.Scopes);
            Assert.AreEqual(verifyCode, refreshToken.VerificationCode);

            _mocRepo.VerifyAll();
        }


    }
}