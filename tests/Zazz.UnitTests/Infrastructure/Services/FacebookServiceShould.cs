using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Facebook;
using Moq;
using NUnit.Framework;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Facebook;
using Zazz.Infrastructure.Services;

namespace Zazz.UnitTests.Infrastructure.Services
{
    [TestFixture]
    public class FacebookServiceShould
    {
        private Mock<IFacebookHelper> _fbHelper;
        private FacebookService _sut;
        private Mock<IErrorHandler> _errorHander;
        private Mock<IUoW> _uow;

        [SetUp]
        public void Init()
        {
            _fbHelper = new Mock<IFacebookHelper>();
            _errorHander = new Mock<IErrorHandler>();
            _uow = new Mock<IUoW>();
            _sut = new FacebookService(_fbHelper.Object, _errorHander.Object, _uow.Object);

            _fbHelper.Setup(x => x.SetAccessToken(It.IsAny<string>()));
            _errorHander.Setup(x => x.LogException(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>()));
            _uow.Setup(x => x.SaveAsync())
                .Returns(() => Task.Run(() => { }));
        }

        [Test]
        public async Task CallRightPathAndSetAccessToken_OnGetUser()
        {
            //Arrange
            var id = "Soroush.Mirzaei";
            var token = "token";
            var user = new FbUser();
            _fbHelper.Setup(x => x.GetAsync<FbUser>(id, "email"))
                     .Returns(() => Task.Run(() => user));

            //Act
            var result = await _sut.GetUserAsync(id, token);

            //Assert
            _fbHelper.Verify(x => x.SetAccessToken(token), Times.Once());
            _fbHelper.Verify(x => x.GetAsync<FbUser>(id, "email"), Times.Once());
            Assert.AreSame(user, result);
        }

        [Test]
        public async Task CallErrorHandlerTokenExpiredWhenOAuthExceptionOccures_OnGetUser()
        {
            //Arrange
            var id = "Soroush.Mirzaei";
            var token = "token";
            var user = new FbUser();
            _fbHelper.Setup(x => x.GetAsync<FbUser>(id, "email"))
                     .Throws<FacebookOAuthException>();
            _errorHander.Setup(x => x.HandleAccessTokenExpiredAsync(id, OAuthProvider.Facebook))
                        .Returns(() => Task.Run(() => { }));
            //Act
            try
            {
                var t = await _sut.GetUserAsync(id, token);
                Assert.Fail("Expected Exception Wasn't Thrown");
            }
            catch (FacebookOAuthException)
            {
            }

            //Assert
            _errorHander.Verify(x => x.HandleAccessTokenExpiredAsync(id, OAuthProvider.Facebook), Times.Once());
        }

        [Test]
        public async Task CallErrorHandlerApiLimitReachedWhenApiLimitExceptionOccures_OnGetUser()
        {
            //Arrange
            var id = "Soroush.Mirzaei";
            var token = "token";
            var user = new FbUser();
            _fbHelper.Setup(x => x.GetAsync<FbUser>(id, "email"))
                     .Throws<FacebookApiLimitException>();
            _errorHander.Setup(x => x.HandleFacebookApiLimitReachedAsync(id, It.IsAny<string>(), It.IsAny<string>()))
                        .Returns(() => Task.Run(() => { }));
            //Act
            try
            {
                var t = await _sut.GetUserAsync(id, token);
                Assert.Fail("Expected Exception Wasn't Thrown");
            }
            catch (FacebookApiLimitException)
            {
            }

            //Assert
            _errorHander.Verify(x => x.HandleFacebookApiLimitReachedAsync(id, It.IsAny<string>(), It.IsAny<string>()),
                                Times.Once());
        }

        [Test]
        public async Task CallErrorHandlerLogErrorWhenUknownExceptionOccures_OnGetUser()
        {
            //Arrange
            var id = "Soroush.Mirzaei";
            var token = "token";
            var user = new FbUser();
            _fbHelper.Setup(x => x.GetAsync<FbUser>(id, "email"))
                     .Throws<Exception>();
            _errorHander.Setup(x => x.LogException(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>()));

            //Act
            try
            {
                var t = await _sut.GetUserAsync(id, token);
                Assert.Fail("Expected Exception Wasn't Thrown");
            }
            catch (Exception)
            {
            }

            //Assert
            _errorHander.Verify(x => x.LogException(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>()),
                                Times.Once());
        }

        [Test]
        public async Task NotThrowOrDoAnythingIfItDidntFintAnyUser_OnHandleRealtimeUserUpdatesAsync()
        {
            //Arrange
            var userAId = 1234L;
            var userBId = 5678L;
            var changes = new FbUserChanges
                          {
                              Entries = new List<FbUserChangesEntry>
                                        {
                                            new FbUserChangesEntry
                                            {
                                                UserId = userAId,
                                                ChangedFields = new[] {"events"}
                                            },
                                            new FbUserChangesEntry
                                            {
                                                UserId = userBId,
                                                ChangedFields = new[] {"events"}
                                            }
                                        }
                          };

            _uow.Setup(x => x.OAuthAccountRepository
                .GetOAuthAccountByProviderId(It.IsAny<long>(), OAuthProvider.Facebook))
                .Returns(() => null);

            //Act
            await _sut.HandleRealtimeUserUpdatesAsync(changes);

            //Assert
            _uow.Verify(x => x.OAuthAccountRepository.GetOAuthAccountByProviderId(userAId, OAuthProvider.Facebook),
                        Times.Once());
            _uow.Verify(x => x.OAuthAccountRepository.GetOAuthAccountByProviderId(userBId, OAuthProvider.Facebook),
                        Times.Once());
            _uow.Verify(x => x.OAuthAccountRepository
                .GetOAuthAccountByProviderId(userAId, OAuthProvider.Facebook), Times.Never());
            _uow.Verify(x => x.OAuthAccountRepository
                .GetOAuthAccountByProviderId(userBId, OAuthProvider.Facebook), Times.Never());
            _uow.Verify(x => x.EventRepository.InsertGraph(It.IsAny<ZazzEvent>()), Times.Never());
            _fbHelper.Verify(x => x.GetEvents(It.IsAny<long>(), It.IsAny<string>()), Times.Never());
        }

        [Test]
        public async Task NotDoAnythingIfChangedFieldsAreNotEvents_OnHandleRealtimeUserUpdatesAsync()
        {
            //Arrange
            var userAId = 1234L;
            var userBId = 5678L;
            var changes = new FbUserChanges
            {
                Entries = new List<FbUserChangesEntry>
                                        {
                                            new FbUserChangesEntry
                                            {
                                                UserId = userAId,
                                                ChangedFields = new[] {"random"}
                                            },
                                            new FbUserChangesEntry
                                            {
                                                UserId = userBId,
                                                ChangedFields = new[] {"random"}
                                            }
                                        }
            };

            //Act
            await _sut.HandleRealtimeUserUpdatesAsync(changes);

            //Assert
            _uow.Verify(x => x.OAuthAccountRepository.GetOAuthAccountByProviderId(userAId, OAuthProvider.Facebook),
                        Times.Never());
            _uow.Verify(x => x.OAuthAccountRepository.GetOAuthAccountByProviderId(userBId, OAuthProvider.Facebook),
                        Times.Never());
            _uow.Verify(x => x.OAuthAccountRepository
                .GetOAuthAccountByProviderId(userAId, OAuthProvider.Facebook), Times.Never());
            _uow.Verify(x => x.OAuthAccountRepository
                .GetOAuthAccountByProviderId(userBId, OAuthProvider.Facebook), Times.Never());
            _uow.Verify(x => x.EventRepository.InsertGraph(It.IsAny<ZazzEvent>()), Times.Never());
            _fbHelper.Verify(x => x.GetEvents(It.IsAny<long>(), It.IsAny<string>()), Times.Never());
        }

        [Test]
        public async Task NotRequestEventsIfUserDoesntWantEventsToBeSynced_OnHandleRealtimeUserUpdatesAsync()
        {
            //Arrange
            var userAId = 1234L;
            var userBId = 5678L;
            var userAAccount = new OAuthAccount
                               {
                                   AccessToken = "user a token",
                                   UserId = (int)userAId,
                                   ProviderUserId = userAId
                               };
            var userBAccount = new OAuthAccount
                               {
                                   AccessToken = "user a token",
                                   UserId = (int)userBId,
                                   ProviderUserId = userBId
                               };

            var changes = new FbUserChanges
            {
                Entries = new List<FbUserChangesEntry>
                                        {
                                            new FbUserChangesEntry
                                            {
                                                UserId = userAId,
                                                ChangedFields = new[] {"events"}
                                            },
                                            new FbUserChangesEntry
                                            {
                                                UserId = userBId,
                                                ChangedFields = new[] {"events"}
                                            }
                                        }
            };

            _uow.Setup(x => x.OAuthAccountRepository
                .GetOAuthAccountByProviderId(userAId, OAuthProvider.Facebook))
                .Returns(() => userAAccount);
            _uow.Setup(x => x.OAuthAccountRepository
                .GetOAuthAccountByProviderId(userBId, OAuthProvider.Facebook))
                .Returns(() => userBAccount);
            _uow.Setup(x => x.UserRepository.WantsFbEventsSynced(It.IsAny<int>()))
                .Returns(() => false);


            //Act
            await _sut.HandleRealtimeUserUpdatesAsync(changes);

            //Assert
            _uow.Verify(x => x.OAuthAccountRepository.GetOAuthAccountByProviderId(userAId, OAuthProvider.Facebook),
                        Times.Once());
            _uow.Verify(x => x.OAuthAccountRepository.GetOAuthAccountByProviderId(userBId, OAuthProvider.Facebook),
                        Times.Once());
            _uow.Verify(x => x.OAuthAccountRepository
                .GetOAuthAccountByProviderId(userAId, OAuthProvider.Facebook), Times.Once());
            _uow.Verify(x => x.OAuthAccountRepository
                .GetOAuthAccountByProviderId(userBId, OAuthProvider.Facebook), Times.Once());
            _uow.Verify(x => x.EventRepository.InsertGraph(It.IsAny<ZazzEvent>()), Times.Never());
            _fbHelper.Verify(x => x.GetEvents(It.IsAny<long>(), It.IsAny<string>()), Times.Never());
        }
    }
}
