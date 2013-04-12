using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using Facebook;
using Moq;
using NUnit.Framework;
using Zazz.Core.Exceptions;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Facebook;
using Zazz.Infrastructure;
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
        private Mock<IPhotoService> _photoService;
        private Mock<IPostService> _postService;
        private Mock<IAlbumService> _albumService;

        [SetUp]
        public void Init()
        {
            _fbHelper = new Mock<IFacebookHelper>();
            _errorHander = new Mock<IErrorHandler>();
            _eventService = new Mock<IEventService>();
            _postService = new Mock<IPostService>();
            _photoService = new Mock<IPhotoService>();
            _albumService = new Mock<IAlbumService>();

            _uow = new Mock<IUoW>();
            _sut = new FacebookService(_fbHelper.Object, _errorHander.Object, _uow.Object, _eventService.Object,
                                       _postService.Object, _photoService.Object, _albumService.Object);

            _errorHander.Setup(x => x.LogException(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>()));
            _uow.Setup(x => x.SaveChanges());
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
            _eventService.Verify(x => x.CreateEvent(It.IsAny<ZazzEvent>()), Times.Never());
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
            _eventService.Verify(x => x.CreateEvent(It.IsAny<ZazzEvent>()), Times.Never());
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
            _eventService.Verify(x => x.CreateEvent(It.IsAny<ZazzEvent>()), Times.Never());
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

            _eventService.Setup(x => x.CreateEvent(It.IsAny<ZazzEvent>()));

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

            _eventService.Verify(x => x.CreateEvent(event2), Times.Once());
            _eventService.Verify(x => x.CreateEvent(event1), Times.Never());
            _eventService.Verify(x => x.CreateEvent(newEvent1), Times.Never());

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

            _uow.Setup(x => x.OAuthAccountRepository.GetUserAccount(userId, OAuthProvider.Facebook))
                .Returns(() => null);

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
                .GetUserAccount(userId, OAuthProvider.Facebook), Times.Once());
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

            _uow.Setup(x => x.OAuthAccountRepository.GetUserAccount(userId, OAuthProvider.Facebook))
                .Returns(oauthAccount);

            //Act
            var result = await _sut.GetUserPagesAsync(userId);

            //Assert
            _uow.Verify(x => x.OAuthAccountRepository
                .GetUserAccount(userId, OAuthProvider.Facebook), Times.Once());
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
        public void SaveAndAddTabToPageIfItsNotExists_OnLinkPage()
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
        public async Task ThrowIfCurrentUserIsNotTheOwner_OnUnlinkPage()
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
                await _sut.UnlinkPageAsync(page.FacebookId, 1);
                Assert.Fail("Expected exception was not thrown");
            }
            catch (SecurityException)
            {
            }

            //Assert
            _uow.Verify(x => x.FacebookPageRepository.GetByFacebookPageId(page.FacebookId), Times.Once());
            _uow.Verify(x => x.FacebookPageRepository.Remove(It.IsAny<FacebookPage>()), Times.Never());
            _uow.Verify(x => x.SaveChanges(), Times.Never());

            _uow.Verify(x => x.AlbumRepository.GetPageAlbumIds(It.IsAny<int>()), Times.Never());
            _uow.Verify(x => x.PostRepository.GetPagePostIds(It.IsAny<int>()), Times.Never());
            _uow.Verify(x => x.EventRepository.GetPageEventIds(It.IsAny<int>()), Times.Never());

            _albumService.Verify(x => x.DeleteAlbumAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never());
            _postService.Verify(x => x.RemovePostAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never());
            _eventService.Verify(x => x.DeleteEventAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never());
        }

        [Test]
        public async Task RemovePageAndAllOfItsAlbumsAndPostsAndEventsFromDb_OnUnlinkPage()
        {
            //Arrange
            var page = new FacebookPage
            {
                Id = 23,
                FacebookId = "123456",
                UserId = 123
            };

            var albumIds = new List<int> { 1, 2 };
            var postIds = new List<int> { 3, 4, 5 };
            var eventIds = new List<int> { 6, 7, 8, 9 };

            _uow.Setup(x => x.FacebookPageRepository.GetByFacebookPageId(page.FacebookId))
                .Returns(page);
            _uow.Setup(x => x.AlbumRepository.GetPageAlbumIds(page.Id))
                .Returns(albumIds);
            _uow.Setup(x => x.PostRepository.GetPagePostIds(page.Id))
                .Returns(postIds);
            _uow.Setup(x => x.EventRepository.GetPageEventIds(page.Id))
                .Returns(eventIds);


            _albumService.Setup(x => x.DeleteAlbumAsync(
                It.IsInRange(albumIds.Min(), albumIds.Max(), Range.Inclusive), page.UserId))
                         .Returns(() => Task.Run(() => { }));
            _postService.Setup(x => x.RemovePostAsync(
                It.IsInRange(postIds.Min(), postIds.Max(), Range.Inclusive), page.UserId))
                .Returns(() => Task.Run(() => { }));
            _eventService.Setup(x => x.DeleteEventAsync(
                It.IsInRange(eventIds.Min(), eventIds.Max(), Range.Inclusive), page.UserId))
                .Returns(() => Task.Run(() => { }));


            //Act
            await _sut.UnlinkPageAsync(page.FacebookId, page.UserId);

            //Assert
            _uow.Verify(x => x.FacebookPageRepository.GetByFacebookPageId(page.FacebookId), Times.Once());
            _uow.Verify(x => x.FacebookPageRepository.Remove(page), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Once());

            _uow.Verify(x => x.AlbumRepository.GetPageAlbumIds(page.Id), Times.Once());
            _uow.Verify(x => x.PostRepository.GetPagePostIds(page.Id), Times.Once());
            _uow.Verify(x => x.EventRepository.GetPageEventIds(page.Id), Times.Once());

            _albumService.Verify(x => x.DeleteAlbumAsync(
                It.IsInRange(albumIds.Min(), albumIds.Max(), Range.Inclusive), page.UserId),
                                 Times.Exactly(albumIds.Count));
            _postService.Verify(x => x.RemovePostAsync(
                It.IsInRange(postIds.Min(), postIds.Max(), Range.Inclusive), page.UserId),
                                Times.Exactly(postIds.Count));
            _eventService.Verify(x => x.DeleteEventAsync(
                It.IsInRange(eventIds.Min(), eventIds.Max(), Range.Inclusive), page.UserId),
                                 Times.Exactly(eventIds.Count));
        }

        [Test]
        public async Task NotDoAnythingIfOAuthAccountNotExists_OnUpdateUserEvents()
        {
            //Arrange
            var fbUserId = 1234L;
            _uow.Setup(x => x.OAuthAccountRepository.GetOAuthAccountByProviderId(fbUserId, OAuthProvider.Facebook))
                .Returns(() => null);

            //Act
            await _sut.UpdateUserEventsAsync(fbUserId, 5);

            //Assert
            _uow.Verify(x => x.OAuthAccountRepository.GetOAuthAccountByProviderId(fbUserId, OAuthProvider.Facebook),
                        Times.Once());
            _uow.Verify(x => x.UserRepository.WantsFbEventsSynced(It.IsAny<int>()), Times.Never());
            _fbHelper.Verify(x => x.GetEvents(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
            _eventService.Verify(x => x.CreateEvent(It.IsAny<ZazzEvent>()), Times.Never());
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
            await _sut.UpdateUserEventsAsync(oauthAccount.ProviderUserId, 5);

            //Assert
            _uow.Verify(x => x.OAuthAccountRepository
                .GetOAuthAccountByProviderId(oauthAccount.ProviderUserId, OAuthProvider.Facebook),
                Times.Once());

            _uow.Verify(x => x.UserRepository.WantsFbEventsSynced(oauthAccount.UserId), Times.Once());
            _fbHelper.Verify(x => x.GetEvents(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
            _eventService.Verify(x => x.CreateEvent(It.IsAny<ZazzEvent>()), Times.Never());
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

            var limit = 12;

            _uow.Setup(x => x.OAuthAccountRepository
                .GetOAuthAccountByProviderId(oauthAccount.ProviderUserId, OAuthProvider.Facebook))
                .Returns(() => oauthAccount);
            _uow.Setup(x => x.UserRepository.WantsFbEventsSynced(oauthAccount.UserId))
                .Returns(true);
            _fbHelper.Setup(x => x.GetEvents(oauthAccount.ProviderUserId, oauthAccount.AccessToken, limit))
                     .Returns(new List<FbEvent> { fbEvent });
            _uow.Setup(x => x.EventRepository.GetByFacebookId(fbEvent.Id))
                .Returns(() => null);
            _fbHelper.Setup(x => x.FbEventToZazzEvent(fbEvent))
                     .Returns(zazzEvent);
            _eventService.Setup(x => x.CreateEvent(zazzEvent));

            //Act
            await _sut.UpdateUserEventsAsync(oauthAccount.ProviderUserId, limit);

            //Assert
            Assert.AreEqual(oauthAccount.UserId, zazzEvent.UserId);
            _uow.Verify(x => x.OAuthAccountRepository
                .GetOAuthAccountByProviderId(oauthAccount.ProviderUserId, OAuthProvider.Facebook),
                Times.Once());

            _uow.Verify(x => x.UserRepository.WantsFbEventsSynced(oauthAccount.UserId), Times.Once());
            _fbHelper.Verify(x => x.GetEvents(oauthAccount.ProviderUserId, oauthAccount.AccessToken, limit),
                             Times.Once());
            _eventService.Verify(x => x.CreateEvent(zazzEvent), Times.Once());
        }

        [Test]
        public async Task NotDoAnythingIfPageDoesntExists_OnUpdatePageEvents()
        {
            //Arrange
            var page = new FacebookPage
                       {
                           AccessToken = "token",
                           FacebookId = "1234",
                           UserId = 123
                       };

            _uow.Setup(x => x.FacebookPageRepository.GetByFacebookPageId(page.FacebookId))
                .Returns(() => null);

            //Act
            await _sut.UpdatePageEventsAsync(page.FacebookId);

            //Assert
            _uow.Verify(x => x.FacebookPageRepository.GetByFacebookPageId(page.FacebookId), Times.Once());
            _uow.Verify(x => x.UserRepository.WantsFbEventsSynced(It.IsAny<int>()), Times.Never());
            _fbHelper.Verify(x => x.GetPageEvents(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()),
                             Times.Never());
            _eventService.Verify(x => x.CreateEvent(It.IsAny<ZazzEvent>()), Times.Never());
            _uow.Verify(x => x.SaveChanges(), Times.Never());
        }

        [Test]
        public async Task NotDoAnythingIfUserDoesntWantEventsToBeSynced_OnUpdatePageEvents()
        {
            //Arrange
            var page = new FacebookPage
            {
                AccessToken = "token",
                FacebookId = "1234",
                UserId = 123
            };

            _uow.Setup(x => x.FacebookPageRepository.GetByFacebookPageId(page.FacebookId))
                .Returns(page);
            _uow.Setup(x => x.UserRepository.WantsFbEventsSynced(page.UserId))
                .Returns(false);

            //Act
            await _sut.UpdatePageEventsAsync(page.FacebookId);

            //Assert
            _uow.Verify(x => x.FacebookPageRepository.GetByFacebookPageId(page.FacebookId), Times.Once());
            _uow.Verify(x => x.UserRepository.WantsFbEventsSynced(page.UserId), Times.Once());
            _fbHelper.Verify(x => x.GetPageEvents(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()),
                             Times.Never());
            _eventService.Verify(x => x.CreateEvent(It.IsAny<ZazzEvent>()), Times.Never());
            _uow.Verify(x => x.SaveChanges(), Times.Never());
        }

        [Test]
        public async Task AddEventsIfTheyDontExist_OnUpdatePageEvents()
        {
            //Arrange
            var page = new FacebookPage
            {
                Id = 333,
                AccessToken = "token",
                FacebookId = "1234",
                UserId = 123
            };

            var fbEvent = new FbEvent
            {
                Id = 12345
            };

            var zazzEvent = new ZazzEvent
            {
                FacebookEventId = fbEvent.Id
            };

            var limit = 15;

            _uow.Setup(x => x.FacebookPageRepository.GetByFacebookPageId(page.FacebookId))
                .Returns(page);
            _uow.Setup(x => x.UserRepository.WantsFbEventsSynced(page.UserId))
                .Returns(true);
            _fbHelper.Setup(x => x.GetPageEvents(page.FacebookId, page.AccessToken, limit))
                     .Returns(new List<FbEvent> { fbEvent });
            _uow.Setup(x => x.EventRepository.GetByFacebookId(fbEvent.Id))
                .Returns(() => null);
            _fbHelper.Setup(x => x.FbEventToZazzEvent(fbEvent))
                     .Returns(zazzEvent);
            _eventService.Setup(x => x.CreateEvent(zazzEvent));

            //Act
            await _sut.UpdatePageEventsAsync(page.FacebookId, limit);

            //Assert
            Assert.AreEqual(page.Id, zazzEvent.PageId);
            _uow.Verify(x => x.FacebookPageRepository.GetByFacebookPageId(page.FacebookId), Times.Once());
            _uow.Verify(x => x.UserRepository.WantsFbEventsSynced(page.UserId), Times.Once());
            _fbHelper.Verify(x => x.GetPageEvents(page.FacebookId, page.AccessToken, limit),
                             Times.Once());
            _eventService.Verify(x => x.CreateEvent(zazzEvent), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Once());
        }

        [Test]
        public void NotDoAnythingIfPageDoesntExists_OnUpdatePageStatuses()
        {
            //Arrange
            var page = new FacebookPage
                       {
                           AccessToken = "token",
                           FacebookId = "12345",
                           UserId = 12
                       };

            _uow.Setup(x => x.FacebookPageRepository.GetByFacebookPageId(page.FacebookId))
                .Returns(() => null);


            //Act
            _sut.UpdatePageStatuses(page.FacebookId);

            //Assert
            _uow.Verify(x => x.FacebookPageRepository.GetByFacebookPageId(page.FacebookId), Times.Once());
            _uow.Verify(x => x.UserRepository.WantsFbPostsSynced(It.IsAny<int>()), Times.Never());
            _fbHelper.Verify(x => x.GetStatuses(It.IsAny<string>(), It.IsAny<int>()), Times.Never());
            _uow.Verify(x => x.PostRepository.InsertGraph(It.IsAny<Post>()), Times.Never());
            _uow.Verify(x => x.SaveChanges(), Times.Never());
        }

        [Test]
        public void NotQueryIfUserDoestWantPostsToBeSynced_OnUpdatePageStatuses()
        {
            //Arrange
            var page = new FacebookPage
            {
                AccessToken = "token",
                FacebookId = "12345",
                UserId = 12
            };

            _uow.Setup(x => x.FacebookPageRepository.GetByFacebookPageId(page.FacebookId))
                .Returns(page);
            _uow.Setup(x => x.UserRepository.WantsFbPostsSynced(page.UserId))
                .Returns(false);

            //Act
            _sut.UpdatePageStatuses(page.FacebookId);

            //Assert
            _uow.Verify(x => x.FacebookPageRepository.GetByFacebookPageId(page.FacebookId), Times.Once());
            _uow.Verify(x => x.UserRepository.WantsFbPostsSynced(page.UserId), Times.Once());
            _fbHelper.Verify(x => x.GetStatuses(It.IsAny<string>(), It.IsAny<int>()), Times.Never());
            _uow.Verify(x => x.PostRepository.InsertGraph(It.IsAny<Post>()), Times.Never());
            _postService.Verify(x => x.NewPostAsync(It.IsAny<Post>()), Times.Never());
            _uow.Verify(x => x.SaveChanges(), Times.Never());
        }

        [Test]
        public void InsertPostIfDoesntExists_OnUpdatePageStatuses()
        {
            //Arrange
            var page = new FacebookPage
            {
                AccessToken = "token",
                FacebookId = "12345",
                UserId = 12
            };

            var fbStatus = new FbStatus
                           {
                               Id = 1,
                               Message = "message",
                               Time = DateTime.UtcNow.ToUnixTimestamp()
                           };
            var limit = 30;

            _uow.Setup(x => x.FacebookPageRepository.GetByFacebookPageId(page.FacebookId))
                .Returns(page);
            _uow.Setup(x => x.UserRepository.WantsFbPostsSynced(page.UserId))
                .Returns(true);
            _fbHelper.Setup(x => x.GetStatuses(page.AccessToken, limit))
                     .Returns(new List<FbStatus> { fbStatus });
            _uow.Setup(x => x.PostRepository.GetByFbId(fbStatus.Id))
                .Returns(() => null);
            _postService.Setup(x => x.NewPostAsync(It.IsAny<Post>()))
                        .Returns(() => Task.Run(() => { }));


            //Act
            _sut.UpdatePageStatuses(page.FacebookId, limit);

            //Assert
            _uow.Verify(x => x.FacebookPageRepository.GetByFacebookPageId(page.FacebookId), Times.Once());
            _uow.Verify(x => x.UserRepository.WantsFbPostsSynced(page.UserId), Times.Once());
            _fbHelper.Verify(x => x.GetStatuses(page.AccessToken, limit), Times.Once());
            _uow.Verify(x => x.PostRepository.InsertGraph(It.IsAny<Post>()), Times.Never());
            _postService.Verify(x => x.NewPostAsync(It.IsAny<Post>()), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Once());
        }

        [Test]
        public void UpdatePostIfExists_OnUpdatePageStatuses()
        {
            //Arrange
            var page = new FacebookPage
            {
                AccessToken = "token",
                FacebookId = "12345",
                UserId = 12
            };

            var fbStatus = new FbStatus
            {
                Id = 1,
                Message = "message",
                Time = DateTime.UtcNow.ToUnixTimestamp()
            };

            var oldPost = new Post
                          {
                              CreatedTime = DateTime.UtcNow.AddDays(-1),
                              FacebookId = fbStatus.Id,
                              Id = 1234,
                              Message = "old msg",
                              UserId = page.UserId
                          };
            var limit = 30;

            _uow.Setup(x => x.FacebookPageRepository.GetByFacebookPageId(page.FacebookId))
                .Returns(page);
            _uow.Setup(x => x.UserRepository.WantsFbPostsSynced(page.UserId))
                .Returns(true);
            _fbHelper.Setup(x => x.GetStatuses(page.AccessToken, limit))
                     .Returns(new List<FbStatus> { fbStatus });
            _uow.Setup(x => x.PostRepository.GetByFbId(fbStatus.Id))
                .Returns(oldPost);
            _uow.Setup(x => x.PostRepository.InsertGraph(It.IsAny<Post>()));

            //Act
            _sut.UpdatePageStatuses(page.FacebookId, limit);

            //Assert
            Assert.AreEqual(fbStatus.Message, oldPost.Message);

            _uow.Verify(x => x.FacebookPageRepository.GetByFacebookPageId(page.FacebookId), Times.Once());
            _uow.Verify(x => x.UserRepository.WantsFbPostsSynced(page.UserId), Times.Once());
            _fbHelper.Verify(x => x.GetStatuses(page.AccessToken, limit), Times.Once());
            _uow.Verify(x => x.PostRepository.InsertGraph(It.IsAny<Post>()), Times.Never());
            _uow.Verify(x => x.SaveChanges(), Times.Once());
        }

        [Test]
        public void NotDoAnythingIfPageNotExists_OnUpdatePagePhotosAsync()
        {
            //Arrange
            var page = new FacebookPage
            {
                AccessToken = "token",
                FacebookId = "12345",
                UserId = 12
            };

            _uow.Setup(x => x.FacebookPageRepository.GetByFacebookPageId(page.FacebookId))
                .Returns(() => null);

            //Act
            _sut.UpdatePagePhotos(page.FacebookId);

            //Assert
            _uow.Verify(x => x.UserRepository.WantsFbImagesSynced(It.IsAny<int>()), Times.Never());
            _fbHelper.Verify(x => x.GetPhotos(It.IsAny<string>(), It.IsAny<int>()), Times.Never());
            _uow.Verify(x => x.PhotoRepository.GetByFacebookId(It.IsAny<string>()), Times.Never());
            _uow.Verify(x => x.AlbumRepository.InsertGraph(It.IsAny<Album>()), Times.Never());
            _uow.Verify(x => x.SaveChanges(), Times.Never());
            _photoService.Verify(x => x.SavePhotoAsync(It.IsAny<Photo>(), It.IsAny<Stream>(), It.IsAny<bool>()),
                                 Times.Never());
        }

        [Test]
        public void NotDoAnythingIfUserDoesntWantToSyncPhotos_OnUpdatePagePhotosAsync()
        {
            //Arrange
            var page = new FacebookPage
            {
                AccessToken = "token",
                FacebookId = "12345",
                UserId = 12
            };

            _uow.Setup(x => x.FacebookPageRepository.GetByFacebookPageId(page.FacebookId))
                .Returns(page);
            _uow.Setup(x => x.UserRepository.WantsFbImagesSynced(page.UserId))
                .Returns(false);

            //Act
            _sut.UpdatePagePhotos(page.FacebookId);

            //Assert
            _uow.Verify(x => x.UserRepository.WantsFbImagesSynced(page.UserId), Times.Once());
            _fbHelper.Verify(x => x.GetPhotos(It.IsAny<string>(), It.IsAny<int>()), Times.Never());
            _uow.Verify(x => x.PhotoRepository.GetByFacebookId(It.IsAny<string>()), Times.Never());
            _uow.Verify(x => x.AlbumRepository.InsertGraph(It.IsAny<Album>()), Times.Never());
            _uow.Verify(x => x.SaveChanges(), Times.Never());
            _photoService.Verify(x => x.SavePhotoAsync(It.IsAny<Photo>(), It.IsAny<Stream>(), It.IsAny<bool>()),
                                 Times.Never());
        }

        [Test]
        public void UpdatePhotoIfExists_OnUpdatePagePhotosAsync()
        {
            //Arrange
            var page = new FacebookPage
            {
                AccessToken = "token",
                FacebookId = "12345",
                UserId = 12
            };

            var fbPhoto = new FbPhoto
                          {
                              AlbumId = "albumId",
                              CreatedTime = DateTime.UtcNow.ToUnixTimestamp(),
                              Description = "desc",
                              Source = "src",
                              Id = "id"
                          };

            var oldPhoto = new Photo
                           {
                               FacebookLink = "old link",
                               Description = "old desc"
                           };

            var limit = 30;

            _uow.Setup(x => x.FacebookPageRepository.GetByFacebookPageId(page.FacebookId))
                .Returns(page);
            _uow.Setup(x => x.UserRepository.WantsFbImagesSynced(page.UserId))
                .Returns(true);
            _fbHelper.Setup(x => x.GetPhotos(page.AccessToken, limit))
                     .Returns(new List<FbPhoto> { fbPhoto });
            _uow.Setup(x => x.PhotoRepository.GetByFacebookId(fbPhoto.Id))
                .Returns(oldPhoto);



            //Act
            _sut.UpdatePagePhotos(page.FacebookId, limit);

            //Assert
            Assert.AreEqual(fbPhoto.Description, oldPhoto.Description);
            Assert.AreEqual(fbPhoto.Source, oldPhoto.FacebookLink);

            _uow.Verify(x => x.UserRepository.WantsFbImagesSynced(page.UserId), Times.Once());
            _fbHelper.Verify(x => x.GetPhotos(page.AccessToken, limit), Times.Once());
            _uow.Verify(x => x.PhotoRepository.GetByFacebookId(fbPhoto.Id), Times.Once());
            _uow.Verify(x => x.AlbumRepository.InsertGraph(It.IsAny<Album>()), Times.Never());
            _uow.Verify(x => x.SaveChanges(), Times.Once());
            _photoService.Verify(x => x.SavePhotoAsync(It.IsAny<Photo>(), It.IsAny<Stream>(), It.IsAny<bool>()),
                                 Times.Never());
        }

        [Test]
        public void InsertPhotoAndCreateAlbumIfNotExists_OnUpdatePagePhotosAsync()
        {
            //Arrange
            var page = new FacebookPage
            {
                AccessToken = "token",
                FacebookId = "12345",
                UserId = 12
            };

            var fbPhoto = new FbPhoto
            {
                AlbumId = "albumId",
                CreatedTime = DateTime.UtcNow.ToUnixTimestamp(),
                Description = "desc",
                Source = "src",
                Id = "id"
            };

            var limit = 30;

            _uow.Setup(x => x.FacebookPageRepository.GetByFacebookPageId(page.FacebookId))
                .Returns(page);
            _uow.Setup(x => x.UserRepository.WantsFbImagesSynced(page.UserId))
                .Returns(true);
            _fbHelper.Setup(x => x.GetPhotos(page.AccessToken, limit))
                     .Returns(new List<FbPhoto> { fbPhoto });
            _uow.Setup(x => x.PhotoRepository.GetByFacebookId(fbPhoto.Id))
                .Returns(() => null);
            _uow.Setup(x => x.AlbumRepository.GetByFacebookId(fbPhoto.AlbumId))
                .Returns(() => null);
            _uow.Setup(x => x.AlbumRepository.InsertGraph(It.IsAny<Album>()));
            _photoService.Setup(x => x.SavePhotoAsync(It.IsAny<Photo>(), Stream.Null, true))
                         .Returns(() => Task.Run(() => 1));

            //Act
            _sut.UpdatePagePhotos(page.FacebookId, limit);

            //Assert

            _uow.Verify(x => x.UserRepository.WantsFbImagesSynced(page.UserId), Times.Once());
            _fbHelper.Verify(x => x.GetPhotos(page.AccessToken, limit), Times.Once());
            _uow.Verify(x => x.PhotoRepository.GetByFacebookId(fbPhoto.Id), Times.Once());
            _uow.Verify(x => x.AlbumRepository.InsertGraph(It.IsAny<Album>()), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Exactly(2));
            _photoService.Verify(x => x.SavePhotoAsync(It.IsAny<Photo>(), Stream.Null, true),
                                 Times.Once());
        }

        [Test]
        public void InsertPhotoAndUseExistingAlbumIfExists_OnUpdatePagePhotosAsync()
        {
            //Arrange
            var page = new FacebookPage
            {
                AccessToken = "token",
                FacebookId = "12345",
                UserId = 12
            };

            var fbPhoto = new FbPhoto
            {
                AlbumId = "albumId",
                CreatedTime = DateTime.UtcNow.ToUnixTimestamp(),
                Description = "desc",
                Source = "src",
                Id = "id"
            };

            var album = new Album
                        {
                            FacebookId = fbPhoto.AlbumId,
                            Id = 12,
                            IsFacebookAlbum = true,
                            Name = "s",
                            UserId = page.UserId
                        };

            var limit = 30;

            _uow.Setup(x => x.FacebookPageRepository.GetByFacebookPageId(page.FacebookId))
                .Returns(page);
            _uow.Setup(x => x.UserRepository.WantsFbImagesSynced(page.UserId))
                .Returns(true);
            _fbHelper.Setup(x => x.GetPhotos(page.AccessToken, limit))
                     .Returns(new List<FbPhoto> { fbPhoto });
            _uow.Setup(x => x.PhotoRepository.GetByFacebookId(fbPhoto.Id))
                .Returns(() => null);
            _uow.Setup(x => x.AlbumRepository.GetByFacebookId(fbPhoto.AlbumId))
                .Returns(album);
            _photoService.Setup(x => x.SavePhotoAsync(It.IsAny<Photo>(), Stream.Null, true))
                         .Returns(() => Task.Run(() => 1));

            //Act
            _sut.UpdatePagePhotos(page.FacebookId, limit);

            //Assert

            _uow.Verify(x => x.UserRepository.WantsFbImagesSynced(page.UserId), Times.Once());
            _fbHelper.Verify(x => x.GetPhotos(page.AccessToken, limit), Times.Once());
            _uow.Verify(x => x.PhotoRepository.GetByFacebookId(fbPhoto.Id), Times.Once());
            _uow.Verify(x => x.AlbumRepository.InsertGraph(It.IsAny<Album>()), Times.Never());
            _uow.Verify(x => x.SaveChanges(), Times.Once());
            _photoService.Verify(x => x.SavePhotoAsync(It.IsAny<Photo>(), Stream.Null, true),
                                 Times.Once());
        }
    }
}
