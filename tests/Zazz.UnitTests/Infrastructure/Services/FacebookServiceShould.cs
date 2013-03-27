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
        private Mock<IEventService> _eventService;

        [SetUp]
        public void Init()
        {
            _fbHelper = new Mock<IFacebookHelper>();
            _errorHander = new Mock<IErrorHandler>();
            _eventService = new Mock<IEventService>();
            _uow = new Mock<IUoW>();
            _sut = new FacebookService(_fbHelper.Object, _errorHander.Object, _uow.Object, _eventService.Object);

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
                .GetOAuthAccountByProviderId(userAId, OAuthProvider.Facebook), Times.Once());
            _uow.Verify(x => x.OAuthAccountRepository
                .GetOAuthAccountByProviderId(userBId, OAuthProvider.Facebook), Times.Once());
            _eventService.Verify(x => x.CreateEventAsync(It.IsAny<ZazzEvent>()), Times.Never());
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
            _eventService.Verify(x => x.CreateEventAsync(It.IsAny<ZazzEvent>()), Times.Never());
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
            _eventService.Verify(x => x.CreateEventAsync(It.IsAny<ZazzEvent>()), Times.Never());
            _fbHelper.Verify(x => x.GetEvents(It.IsAny<long>(), It.IsAny<string>()), Times.Never());
        }

        [Test]
        public async Task AddFbEventsIfTheyAreNewAndUpdateIfTheyExist_OnHandleRealtimeUserUpdatesAsync()
        {
            //Arrange
            var userAId = 1234L;
            var userAAccount = new OAuthAccount
            {
                AccessToken = "user a token",
                UserId = (int)userAId,
                ProviderUserId = userAId
            };

            var changes = new FbUserChanges
            {
                Entries = new List<FbUserChangesEntry>
                                        {
                                            new FbUserChangesEntry
                                            {
                                                UserId = userAId,
                                                ChangedFields = new[] {"events"}
                                            }
                                        }
            };

            // fb event 1 is exists and needs to be updated
            var event1 = new ZazzEvent
                         {
                             Id = 1,
                             Name = "old Name",
                             Description = "old desc",
                             IsDateOnly = false,
                             Location = "old loc",
                             FacebookPhotoLink = "old pic",
                             CreatedDate = DateTime.UtcNow.AddDays(-1)
                         };

            var fbEvent1 = new FbEvent
                           {
                               Id = 1
                           };

            // fb event 2 is not exists and need to be inserted
            var fbEvent2 = new FbEvent
                           {
                               Id = 2
                           };
            var event2 = new ZazzEvent { Id = 2 };

            // fb event 3 is the new version of event1
            var fbEvent3 = new FbEvent
                           {
                               Id = 1,
                               Name = "new Name",
                               Description = "new desc",
                               IsDateOnly = true,
                               Location = "new loc",
                               Pic = "new pic"
                           };

            var newEvent1 = new ZazzEvent
            {
                Id = 1,
                Name = "new Name",
                Description = "new desc",
                IsDateOnly = true,
                Location = "new loc",
                FacebookPhotoLink = "new pic",
                CreatedDate = DateTime.UtcNow
            };

            var fbEvents = new List<FbEvent> { fbEvent2, fbEvent3 };

            _uow.Setup(x => x.OAuthAccountRepository
                             .GetOAuthAccountByProviderId(userAId, OAuthProvider.Facebook))
                .Returns(() => userAAccount);

            _uow.Setup(x => x.UserRepository.WantsFbEventsSynced(It.IsAny<int>()))
                .Returns(() => true);

            _uow.Setup(x => x.EventRepository.GetByFacebookId(fbEvent1.Id))
                .Returns(event1);

            _uow.Setup(x => x.EventRepository.GetByFacebookId(fbEvent2.Id))
                .Returns(() => null);

            _fbHelper.Setup(x => x.GetEvents(userAId, userAAccount.AccessToken))
                     .Returns(fbEvents);

            _fbHelper.Setup(x => x.FbEventToZazzEvent(fbEvent3))
                     .Returns(newEvent1);

            _fbHelper.Setup(x => x.FbEventToZazzEvent(fbEvent2))
                     .Returns(event2);

            _eventService.Setup(x => x.CreateEventAsync(It.IsAny<ZazzEvent>()))
                         .Returns(() => Task.Run(() => { }));

            //Act
            await _sut.HandleRealtimeUserUpdatesAsync(changes);

            //Assert
            Assert.AreEqual(newEvent1.Name, event1.Name);
            Assert.AreEqual(newEvent1.Description, event1.Description);
            Assert.AreEqual(newEvent1.IsDateOnly, event1.IsDateOnly);
            Assert.AreEqual(newEvent1.Location, event1.Location);
            Assert.AreEqual(newEvent1.FacebookPhotoLink, event1.FacebookPhotoLink);
            Assert.AreEqual(event2.UserId, userAId);

            _uow.Verify(x => x.OAuthAccountRepository.GetOAuthAccountByProviderId(userAId, OAuthProvider.Facebook),
                        Times.Once());
            _uow.Verify(x => x.OAuthAccountRepository
                .GetOAuthAccountByProviderId(userAId, OAuthProvider.Facebook), Times.Once());

            _eventService.Verify(x => x.CreateEventAsync(event2), Times.Once());
            _eventService.Verify(x => x.CreateEventAsync(event1), Times.Never());
            _eventService.Verify(x => x.CreateEventAsync(newEvent1), Times.Never());

            _fbHelper.Verify(x => x.GetEvents(userAId, userAAccount.AccessToken), Times.Once());
            _uow.Verify(x => x.SaveAsync(), Times.Once());
        }
    }
}
