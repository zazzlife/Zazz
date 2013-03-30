using System;
using System.Collections.Generic;
using System.Security;
using System.Threading.Tasks;
using Facebook;
using Moq;
using NUnit.Framework;
using Zazz.Core.Exceptions;
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
            _uow.Setup(x => x.SaveChanges());
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
            _fbHelper.Verify(x => x.GetEvents(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
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
            _fbHelper.Verify(x => x.GetEvents(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
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
            _fbHelper.Verify(x => x.GetEvents(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
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

            _fbHelper.Setup(x => x.GetEvents(userAId, userAAccount.AccessToken, It.IsAny<int>()))
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

            _fbHelper.Verify(x => x.GetEvents(userAId, userAAccount.AccessToken, It.IsAny<int>()), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Once());
        }

        [Test]
        public async Task ThrowIfAccessTokenWasEmpty_OnGetPages()
        {
            //Arrange
            var userId = 1234;
            var oauthAccount = new OAuthAccount
                               {
                                   AccessToken = "token"
                               };

            _uow.Setup(x => x.OAuthAccountRepository.GetUserAccountAsync(userId, OAuthProvider.Facebook))
                .Returns(() => Task.Factory.StartNew<OAuthAccount>(() => null));

            //Act
            try
            {
                var result = await _sut.GetUserPagesAsync(userId);
                Assert.Fail("Expected exception wasn't thrown");
            }
            catch (OAuthAccountNotFoundException)
            {
            }

            //Assert
            _uow.Verify(x => x.OAuthAccountRepository
                .GetUserAccountAsync(userId, OAuthProvider.Facebook), Times.Once());
            _fbHelper.Verify(x => x.GetPages(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public async Task CallAndReturnResultFromFBHelper_OnGetPages()
        {
            //Arrange
            var userId = 1234;
            var oauthAccount = new OAuthAccount
            {
                AccessToken = "token"
            };

            _uow.Setup(x => x.OAuthAccountRepository.GetUserAccountAsync(userId, OAuthProvider.Facebook))
                .Returns(() => Task.Factory.StartNew<OAuthAccount>(() => oauthAccount));

            //Act
            var result = await _sut.GetUserPagesAsync(userId);

            //Assert
            _uow.Verify(x => x.OAuthAccountRepository
                .GetUserAccountAsync(userId, OAuthProvider.Facebook), Times.Once());
            _fbHelper.Verify(x => x.GetPages(oauthAccount.AccessToken), Times.Once());
        }

        [Test]
        public void ThrowIfPageExists_OnLinkPage()
        {
            //Arrange
            var page = new FacebookPage
                       {
                           FacebookId = "123456"
                       };
            _uow.Setup(x => x.FacebookPageRepository.GetByFacebookPageId(page.FacebookId))
                .Returns(page);
            _uow.Setup(x => x.FacebookPageRepository.InsertGraph(It.IsAny<FacebookPage>()));


            //Act
            try
            {
                _sut.LinkPage(page);
                Assert.Fail("Expected exception wasn't thrown");
            }
            catch (FacebookPageExistsException)
            {
            }

            //Assert
            _uow.Verify(x => x.FacebookPageRepository.GetByFacebookPageId(page.FacebookId), Times.Once());
            _uow.Verify(x => x.FacebookPageRepository.InsertGraph(page), Times.Never());
            _uow.Verify(x => x.SaveChanges(), Times.Never());
            _fbHelper.Verify(x => x.LinkPage(page.FacebookId, page.AccessToken), Times.Never());
        }

        [Test]
        public void SaveAndLinkNewPageIfItsNotExists_OnLinkPage()
        {
            //Arrange
            var page = new FacebookPage
                       {
                           FacebookId = "123456",
                           AccessToken = "tok",
                       };

            _uow.Setup(x => x.FacebookPageRepository.GetByFacebookPageId(page.FacebookId))
                .Returns(() => null);
            _uow.Setup(x => x.FacebookPageRepository.InsertGraph(It.IsAny<FacebookPage>()));
            _fbHelper.Setup(x => x.LinkPage(page.FacebookId, page.AccessToken));


            //Act
            _sut.LinkPage(page);

            //Assert
            _fbHelper.Verify(x => x.LinkPage(page.FacebookId, page.AccessToken), Times.Once());
            _uow.Verify(x => x.FacebookPageRepository.GetByFacebookPageId(page.FacebookId), Times.Once());
            _uow.Verify(x => x.FacebookPageRepository.InsertGraph(page), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Once());
        }

        [Test]
        public void ThrowIfCurrentUserIsNotTheOwner_OnUnlinkPage()
        {
            //Arrange
            var page = new FacebookPage
            {
                FacebookId = "123456",
                UserId = 123
            };

            _uow.Setup(x => x.FacebookPageRepository.GetByFacebookPageId(page.FacebookId))
                .Returns(page);

            //Act
            try
            {
                _sut.UnlinkPage(page.FacebookId, 1);
                Assert.Fail("Expected exception was not thrown");
            }
            catch (SecurityException)
            {
            }

            //Assert
            _uow.Verify(x => x.FacebookPageRepository.GetByFacebookPageId(page.FacebookId), Times.Once());
            _uow.Verify(x => x.FacebookPageRepository.Remove(It.IsAny<FacebookPage>()), Times.Never());
            _uow.Verify(x => x.SaveChanges(), Times.Never());
        }

        [Test]
        public void RemovePageFromDb_OnUnlinkPage()
        {
            //Arrange
            var page = new FacebookPage
            {
                FacebookId = "123456",
                UserId = 123
            };

            _uow.Setup(x => x.FacebookPageRepository.GetByFacebookPageId(page.FacebookId))
                .Returns(page);

            //Act
            _sut.UnlinkPage(page.FacebookId, page.UserId);

            //Assert
            _uow.Verify(x => x.FacebookPageRepository.GetByFacebookPageId(page.FacebookId), Times.Once());
            _uow.Verify(x => x.FacebookPageRepository.Remove(page), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Once());
        }

        [Test]
        public async Task NotDoAnythingIfOAuthAccountNotExists_OnUpdateUserEvents()
        {
            //Arrange
            var fbUserId = 1234L;
            _uow.Setup(x => x.OAuthAccountRepository.GetOAuthAccountByProviderId(fbUserId, OAuthProvider.Facebook))
                .Returns(() => null);

            //Act
            await _sut.UpdateUserEventsAsync(fbUserId);

            //Assert
            _uow.Verify(x => x.OAuthAccountRepository.GetOAuthAccountByProviderId(fbUserId, OAuthProvider.Facebook),
                        Times.Once());
            _uow.Verify(x => x.UserRepository.WantsFbEventsSynced(It.IsAny<int>()), Times.Never());
            _fbHelper.Verify(x => x.GetEvents(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
            _eventService.Verify(x => x.CreateEventAsync(It.IsAny<ZazzEvent>()), Times.Never());
        }

        [Test]
        public async Task NotDoAnythingIfUserDoesntWantEventsToBeSynced_OnUpdateUserEvents()
        {
            //Arrange
            var oauthAccount = new OAuthAccount
                               {
                                   AccessToken = "token",
                                   ProviderUserId = 1234,
                                   UserId = 111
                               };

            _uow.Setup(x => x.OAuthAccountRepository
                .GetOAuthAccountByProviderId(oauthAccount.ProviderUserId, OAuthProvider.Facebook))
                .Returns(() => oauthAccount);
            _uow.Setup(x => x.UserRepository.WantsFbEventsSynced(oauthAccount.UserId))
                .Returns(false);

            //Act
            await _sut.UpdateUserEventsAsync(oauthAccount.ProviderUserId);

            //Assert
            _uow.Verify(x => x.OAuthAccountRepository
                .GetOAuthAccountByProviderId(oauthAccount.ProviderUserId, OAuthProvider.Facebook),
                Times.Once());

            _uow.Verify(x => x.UserRepository.WantsFbEventsSynced(oauthAccount.UserId), Times.Once());
            _fbHelper.Verify(x => x.GetEvents(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
            _eventService.Verify(x => x.CreateEventAsync(It.IsAny<ZazzEvent>()), Times.Never());
        }

        [Test]
        public async Task AddEventsIfTheyDontExists_OnUpdateUserEvents()
        {
            //Arrange
            var oauthAccount = new OAuthAccount
            {
                AccessToken = "token",
                ProviderUserId = 1234,
                UserId = 111
            };

            var fbEvent = new FbEvent
                          {
                              Id = 12345
                          };

            var zazzEvent = new ZazzEvent
                            {
                                FacebookEventId = fbEvent.Id
                            };

            _uow.Setup(x => x.OAuthAccountRepository
                .GetOAuthAccountByProviderId(oauthAccount.ProviderUserId, OAuthProvider.Facebook))
                .Returns(() => oauthAccount);
            _uow.Setup(x => x.UserRepository.WantsFbEventsSynced(oauthAccount.UserId))
                .Returns(true);
            _fbHelper.Setup(x => x.GetEvents(oauthAccount.ProviderUserId, oauthAccount.AccessToken, It.IsAny<int>()))
                     .Returns(new List<FbEvent> { fbEvent });
            _uow.Setup(x => x.EventRepository.GetByFacebookId(fbEvent.Id))
                .Returns(() => null);
            _fbHelper.Setup(x => x.FbEventToZazzEvent(fbEvent))
                     .Returns(zazzEvent);
            _eventService.Setup(x => x.CreateEventAsync(zazzEvent))
                         .Returns(() => Task.Run(() => { }));

            //Act
            await _sut.UpdateUserEventsAsync(oauthAccount.ProviderUserId);

            //Assert
            Assert.AreEqual(oauthAccount.UserId, zazzEvent.UserId);
            _uow.Verify(x => x.OAuthAccountRepository
                .GetOAuthAccountByProviderId(oauthAccount.ProviderUserId, OAuthProvider.Facebook),
                Times.Once());

            _uow.Verify(x => x.UserRepository.WantsFbEventsSynced(oauthAccount.UserId), Times.Once());
            _fbHelper.Verify(x => x.GetEvents(oauthAccount.ProviderUserId, oauthAccount.AccessToken, It.IsAny<int>()),
                             Times.Once());
            _eventService.Verify(x => x.CreateEventAsync(zazzEvent), Times.Once());
        }
    }
}
