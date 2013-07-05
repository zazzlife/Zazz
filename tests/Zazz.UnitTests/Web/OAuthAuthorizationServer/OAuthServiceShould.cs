using System;
using System.Linq;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Zazz.Core.Exceptions;
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
        private Mock<IStaticDataRepository> _staticDataRepo;

        [SetUp]
        public void Init()
        {
            _mocRepo = new MockRepository(MockBehavior.Strict);
            _uow = _mocRepo.Create<IUoW>();
            _cryptoService = _mocRepo.Create<ICryptoService>();
            _staticDataRepo = _mocRepo.Create<IStaticDataRepository>();

            _sut = new OAuthService(_uow.Object, _cryptoService.Object, _staticDataRepo.Object);
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

            var verifyCodeBytes = new byte[] { 1, 2, 3, 4, 5, 6 };
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

        [Test]
        public void ThrowIfTokenDoesntHaveTokenId_OnRefreshAccessToken()
        {
            //Arrange
            var refreshToken = new JWT
                               {
                                   ClientId = 1,
                                   Scopes = new List<string> { "full" },
                                   UserId = 11,
                                   TokenId = null,
                                   TokenType = JWT.REFRESH_TOKEN_TYPE,
                                   VerificationCode = "verificationCode"
                               };

            //Act
            Assert.Throws<InvalidTokenException>(() => _sut.RefreshAccessToken(refreshToken.ToString()));

            //Assert
            _mocRepo.VerifyAll();
        }

        [Test]
        public void ThrowIfTokenDoesntHaveClientId_OnRefreshAccessToken()
        {
            //Arrange
            var refreshToken = new JWT
            {
                ClientId = 0,
                Scopes = new List<string> { "full" },
                UserId = 11,
                TokenId = 22,
                TokenType = JWT.REFRESH_TOKEN_TYPE,
                VerificationCode = "verificationCode"
            };

            //Act
            Assert.Throws<InvalidTokenException>(() => _sut.RefreshAccessToken(refreshToken.ToString()));

            //Assert
            _mocRepo.VerifyAll();
        }

        [Test]
        public void ThrowIfTokenDoesntHaveUserId_OnRefreshAccessToken()
        {
            //Arrange
            var refreshToken = new JWT
            {
                ClientId = 1,
                Scopes = new List<string> { "full" },
                UserId = 0,
                TokenId = 22,
                TokenType = JWT.REFRESH_TOKEN_TYPE,
                VerificationCode = "verificationCode"
            };

            //Act
            Assert.Throws<InvalidTokenException>(() => _sut.RefreshAccessToken(refreshToken.ToString()));

            //Assert
            _mocRepo.VerifyAll();
        }

        [Test]
        public void ThrowIfTokenDoesntHaveVerificationCode_OnRefreshAccessToken()
        {
            //Arrange
            var refreshToken = new JWT
            {
                ClientId = 1,
                Scopes = new List<string> { "full" },
                UserId = 11,
                TokenId = 22,
                TokenType = JWT.REFRESH_TOKEN_TYPE,
                VerificationCode = null
            };

            //Act
            Assert.Throws<InvalidTokenException>(() => _sut.RefreshAccessToken(refreshToken.ToString()));

            //Assert
            _mocRepo.VerifyAll();
        }

        [Test]
        public void ThrowIfTokenIsntARefreshToken_OnRefreshAccessToken()
        {
            //Arrange
            var refreshToken = new JWT
            {
                ClientId = 1,
                Scopes = new List<string> { "full" },
                UserId = 11,
                TokenId = 22,
                TokenType = JWT.ACCESS_TOKEN_TYPE,
                VerificationCode = "verificationCode"
            };

            //Act
            Assert.Throws<InvalidTokenException>(() => _sut.RefreshAccessToken(refreshToken.ToString()));

            //Assert
            _mocRepo.VerifyAll();
        }

        [Test]
        public void ThrowIfRecordDoestExistsInDB_OnRefreshAccessToken()
        {
            //Arrange
            var refreshToken = new JWT
            {
                ClientId = 1,
                Scopes = new List<string> { "full" },
                UserId = 11,
                TokenId = 22,
                TokenType = JWT.REFRESH_TOKEN_TYPE,
                VerificationCode = "verificationCode"
            };

            _uow.Setup(x => x.OAuthRefreshTokenRepository.GetById(refreshToken.TokenId.Value))
                .Returns(() => null);


            //Act
            Assert.Throws<InvalidTokenException>(() => _sut.RefreshAccessToken(refreshToken.ToString()));

            //Assert
            _mocRepo.VerifyAll();
        }

        [Test]
        public void ThrowIfVerificationCodesDontMatch_OnRefreshAccessToken()
        {
            //Arrange
            var refreshToken = new JWT
            {
                ClientId = 1,
                Scopes = new List<string> { "full" },
                UserId = 11,
                TokenId = 22,
                TokenType = JWT.REFRESH_TOKEN_TYPE,
                VerificationCode = "invalidCode"
            };

            var oauthRefreshToken = new OAuthRefreshToken
                                    {
                                        OAuthClientId = refreshToken.ClientId,
                                        UserId = refreshToken.UserId,
                                        VerificationCode = "verificationCode",
                                        Id = refreshToken.TokenId.Value
                                    };

            _uow.Setup(x => x.OAuthRefreshTokenRepository.GetById(refreshToken.TokenId.Value))
                .Returns(oauthRefreshToken);


            //Act
            Assert.Throws<InvalidTokenException>(() => _sut.RefreshAccessToken(refreshToken.ToString()));

            //Assert
            _mocRepo.VerifyAll();
        }

        [Test]
        public void ThrowIfClientIdIsDifferent_OnRefreshAccessToken()
        {
            //Arrange
            var refreshToken = new JWT
            {
                ClientId = 1,
                Scopes = new List<string> { "full" },
                UserId = 11,
                TokenId = 22,
                TokenType = JWT.REFRESH_TOKEN_TYPE,
                VerificationCode = "verificationCode"
            };

            var oauthRefreshToken = new OAuthRefreshToken
            {
                OAuthClientId = 44,
                UserId = refreshToken.UserId,
                VerificationCode = "verificationCode",
                Id = refreshToken.TokenId.Value
            };

            _uow.Setup(x => x.OAuthRefreshTokenRepository.GetById(refreshToken.TokenId.Value))
                .Returns(oauthRefreshToken);


            //Act
            Assert.Throws<InvalidTokenException>(() => _sut.RefreshAccessToken(refreshToken.ToString()));

            //Assert
            _mocRepo.VerifyAll();
        }

        [Test]
        public void ThrowIfVerificationUserIdIsDifferent_OnRefreshAccessToken()
        {
            //Arrange
            var refreshToken = new JWT
            {
                ClientId = 1,
                Scopes = new List<string> { "full" },
                UserId = 11,
                TokenId = 22,
                TokenType = JWT.REFRESH_TOKEN_TYPE,
                VerificationCode = "verificationCode"
            };

            var oauthRefreshToken = new OAuthRefreshToken
            {
                OAuthClientId = refreshToken.ClientId,
                UserId = 989,
                VerificationCode = "verificationCode",
                Id = refreshToken.TokenId.Value
            };

            _uow.Setup(x => x.OAuthRefreshTokenRepository.GetById(refreshToken.TokenId.Value))
                .Returns(oauthRefreshToken);


            //Act
            Assert.Throws<InvalidTokenException>(() => _sut.RefreshAccessToken(refreshToken.ToString()));

            //Assert
            _mocRepo.VerifyAll();
        }

        [Test]
        public void ThrowIfRequestedScopeDoesntExists_OnRefreshAccessToken()
        {
            //Arrange
            var scopes = new[]
                         {
                             new OAuthScope {Id = 1, Name = "full"}
                         };

            var refreshToken = new JWT
                               {
                                   ClientId = 1,
                                   Scopes = new List<string> { "full", "some random scope" },
                                   UserId = 11,
                                   TokenId = 22,
                                   TokenType = JWT.REFRESH_TOKEN_TYPE,
                                   VerificationCode = "verificationCode"
                               };

            var oauthRefreshToken = new OAuthRefreshToken
                                    {
                                        OAuthClientId = refreshToken.ClientId,
                                        UserId = refreshToken.UserId,
                                        VerificationCode = "verificationCode",
                                        Id = refreshToken.TokenId.Value,
                                        Scopes = new List<OAuthRefreshTokenScope>
                                                 {
                                                     new OAuthRefreshTokenScope
                                                     {
                                                         ScopeId = 1
                                                     }
                                                 }
                                    };

            _staticDataRepo.Setup(x => x.GetOAuthScopes())
                           .Returns(scopes);

            _uow.Setup(x => x.OAuthRefreshTokenRepository.GetById(refreshToken.TokenId.Value))
                .Returns(oauthRefreshToken);


            //Act
            Assert.Throws<InvalidTokenException>(() => _sut.RefreshAccessToken(refreshToken.ToString()));

            //Assert
            _mocRepo.VerifyAll();
        }

        [Test]
        public void ThrowIfRefreshTokenIsNotAuthorizedForARequestedScope_OnRefreshAccessToken()
        {
            //Arrange
            var scopes = new[]
                         {
                             new OAuthScope {Id = 1, Name = "full"},
                             new OAuthScope {Id = 2, Name = "scope2"}
                         };

            var refreshToken = new JWT
            {
                ClientId = 1,
                Scopes = new List<string> { "full", "scope2" },
                UserId = 11,
                TokenId = 22,
                TokenType = JWT.REFRESH_TOKEN_TYPE,
                VerificationCode = "verificationCode"
            };

            var oauthRefreshToken = new OAuthRefreshToken
            {
                OAuthClientId = refreshToken.ClientId,
                UserId = refreshToken.UserId,
                VerificationCode = "verificationCode",
                Id = refreshToken.TokenId.Value,
                Scopes = new List<OAuthRefreshTokenScope>
                                                 {
                                                     new OAuthRefreshTokenScope
                                                     {
                                                         ScopeId = 1
                                                     }
                                                 }
            };

            _staticDataRepo.Setup(x => x.GetOAuthScopes())
                           .Returns(scopes);

            _uow.Setup(x => x.OAuthRefreshTokenRepository.GetById(refreshToken.TokenId.Value))
                .Returns(oauthRefreshToken);


            //Act
            Assert.Throws<InvalidTokenException>(() => _sut.RefreshAccessToken(refreshToken.ToString()));

            //Assert
            _mocRepo.VerifyAll();
        }

        [Test]
        public void ReturnNewAccessTokenAndUpdateUserLastLogin_OnRefreshAccessToken()
        {
            //Arrange
            var scopes = new[]
                         {
                             new OAuthScope {Id = 1, Name = "full"},
                             new OAuthScope {Id = 2, Name = "scope2"}
                         };

            var refreshToken = new JWT
            {
                ClientId = 1,
                Scopes = new List<string> { "full", "scope2" },
                UserId = 11,
                TokenId = 22,
                TokenType = JWT.REFRESH_TOKEN_TYPE,
                VerificationCode = "verificationCode"
            };

            var oauthRefreshToken = new OAuthRefreshToken
                                    {
                                        OAuthClientId = refreshToken.ClientId,
                                        UserId = refreshToken.UserId,
                                        VerificationCode = "verificationCode",
                                        Id = refreshToken.TokenId.Value,
                                        Scopes = new List<OAuthRefreshTokenScope>
                                                 {
                                                     new OAuthRefreshTokenScope
                                                     {
                                                         ScopeId = 1
                                                     },
                                                     new OAuthRefreshTokenScope
                                                     {
                                                         ScopeId = 2
                                                     }
                                                 },
                                        User = new User
                                               {
                                                   LastActivity = DateTime.MinValue
                                               }
                                    };

            _staticDataRepo.Setup(x => x.GetOAuthScopes())
                           .Returns(scopes);

            _uow.Setup(x => x.OAuthRefreshTokenRepository.GetById(refreshToken.TokenId.Value))
                .Returns(oauthRefreshToken);

            _uow.Setup(x => x.SaveChanges());

            //Act
            var result = _sut.RefreshAccessToken(refreshToken.ToString());

            //Assert
            Assert.AreEqual(DateTime.UtcNow.Date, oauthRefreshToken.User.LastActivity.Date);

            Assert.AreEqual(refreshToken.ClientId, result.ClientId);
            Assert.AreEqual(refreshToken.UserId, result.UserId);
            Assert.IsTrue(result.ExpirationDate > DateTime.UtcNow &&
                          result.ExpirationDate < DateTime.UtcNow.AddHours(2));
            Assert.AreEqual(JWT.ACCESS_TOKEN_TYPE, result.TokenType);
            CollectionAssert.AreEqual(refreshToken.Scopes, result.Scopes);
            Assert.IsNull(result.TokenId);

            _mocRepo.VerifyAll();
        }
    }
}