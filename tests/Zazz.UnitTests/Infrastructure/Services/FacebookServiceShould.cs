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
using Zazz.Core.Models.Data.Enums;
using Zazz.Core.Models.Facebook;
using Zazz.Infrastructure;
using Zazz.Infrastructure.Helpers;
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
        private MockRepository _mockRepo;

        [SetUp]
        public void Init()
        {
            _mockRepo = new MockRepository(MockBehavior.Strict);

            _fbHelper = _mockRepo.Create<IFacebookHelper>();
            _errorHander = _mockRepo.Create<IErrorHandler>();
            _eventService = _mockRepo.Create<IEventService>();
            _postService = _mockRepo.Create<IPostService>();
            _photoService = _mockRepo.Create<IPhotoService>();
            _albumService = _mockRepo.Create<IAlbumService>();

            _uow = _mockRepo.Create<IUoW>();
            _sut = new FacebookService(_fbHelper.Object, _errorHander.Object, _uow.Object, _eventService.Object,
                                       _postService.Object, _photoService.Object, _albumService.Object);
        }

        [Test]
        public void NotThrowOrDoAnythingIfItDidntFintAnyUser_OnHandleRealtimeUserUpdatesAsync()
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

            _uow.Setup(x => x.LinkedAccountRepository
                .GetOAuthAccountByProviderId(It.IsAny<long>(), OAuthProvider.Facebook))
                .Returns(() => null);
            _uow.Setup(x => x.SaveChanges());

            //Act
            _sut.HandleRealtimeUserUpdatesAsync(changes);

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void NotDoAnythingIfChangedFieldsAreNotEvents_OnHandleRealtimeUserUpdatesAsync()
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

            _uow.Setup(x => x.SaveChanges());

            //Act
            _sut.HandleRealtimeUserUpdatesAsync(changes);

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void NotRequestEventsIfUserDoesntWantEventsToBeSynced_OnHandleRealtimeUserUpdatesAsync()
        {
            //Arrange
            var userAId = 1234L;
            var userBId = 5678L;
            var userAAccount = new LinkedAccount
                               {
                                   AccessToken = "user a token",
                                   UserId = (int)userAId,
                                   ProviderUserId = userAId
                               };
            var userBAccount = new LinkedAccount
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

            _uow.Setup(x => x.LinkedAccountRepository
                .GetOAuthAccountByProviderId(userAId, OAuthProvider.Facebook))
                .Returns(() => userAAccount);
            _uow.Setup(x => x.LinkedAccountRepository
                .GetOAuthAccountByProviderId(userBId, OAuthProvider.Facebook))
                .Returns(() => userBAccount);
            _uow.Setup(x => x.UserRepository.WantsFbEventsSynced(It.IsAny<int>()))
                .Returns(() => false);
            _uow.Setup(x => x.SaveChanges());

            //Act
            _sut.HandleRealtimeUserUpdatesAsync(changes);

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void AddFbEventsIfTheyAreNewAndUpdateIfTheyExist_OnHandleRealtimeUserUpdatesAsync()
        {
            //Arrange
            var userAId = 1234L;
            var userAAccount = new LinkedAccount
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

            _uow.Setup(x => x.LinkedAccountRepository
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

            _uow.Setup(x => x.SaveChanges());

            //Act
            _sut.HandleRealtimeUserUpdatesAsync(changes);

            //Assert
            Assert.AreEqual(newEvent1.Name, event1.Name);
            Assert.AreEqual(newEvent1.Description, event1.Description);
            Assert.AreEqual(newEvent1.IsDateOnly, event1.IsDateOnly);
            Assert.AreEqual(newEvent1.Location, event1.Location);
            Assert.AreEqual(newEvent1.FacebookPhotoLink, event1.FacebookPhotoLink);
            Assert.AreEqual(event2.UserId, userAId);

            _mockRepo.VerifyAll();
        }

        [Test]
        public void ThrowIfAccessTokenWasEmpty_OnGetPages()
        {
            //Arrange
            var userId = 1234;
            var oauthAccount = new LinkedAccount
                               {
                                   AccessToken = "token"
                               };

            _uow.Setup(x => x.LinkedAccountRepository.GetUserAccount(userId, OAuthProvider.Facebook))
                .Returns(() => null);

            //Act
            Assert.Throws<OAuthAccountNotFoundException>(() => _sut.GetUserPages(userId));

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void CallAndReturnResultFromFBHelper_OnGetPages()
        {
            //Arrange
            var userId = 1234;
            var oauthAccount = new LinkedAccount
            {
                AccessToken = "token"
            };

            _uow.Setup(x => x.LinkedAccountRepository.GetUserAccount(userId, OAuthProvider.Facebook))
                .Returns(oauthAccount);

            _fbHelper.Setup(x => x.GetPages(oauthAccount.AccessToken))
                     .Returns(Enumerable.Empty<FbPage>);

            //Act
            var result = _sut.GetUserPages(userId);

            //Assert
            _mockRepo.VerifyAll();
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


            //Act
            Assert.Throws<FacebookPageExistsException>(() => _sut.LinkPage(page));

            //Assert
            _mockRepo.VerifyAll();
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
            _uow.Setup(x => x.SaveChanges());

            //Act
            _sut.LinkPage(page);

            //Assert
            _mockRepo.VerifyAll();
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
            Assert.Throws<SecurityException>(() => _sut.UnlinkPage(page.FacebookId, 1));

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void RemovePageAndAllOfItsAlbumsAndPostsAndEventsFromDb_OnUnlinkPage()
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

            _uow.Setup(x => x.FacebookPageRepository.Remove(page));

            _uow.Setup(x => x.SaveChanges());

            _albumService.Setup(x => x.DeleteAlbum(
                It.IsInRange(albumIds.Min(), albumIds.Max(), Range.Inclusive), page.UserId));
            _postService.Setup(x => x.RemovePost(
                It.IsInRange(postIds.Min(), postIds.Max(), Range.Inclusive), page.UserId));
            _eventService.Setup(x => x.DeleteEvent(
                It.IsInRange(eventIds.Min(), eventIds.Max(), Range.Inclusive), page.UserId));


            //Act
            _sut.UnlinkPage(page.FacebookId, page.UserId);

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void NotDoAnythingIfOAuthAccountNotExists_OnUpdateUserEvents()
        {
            //Arrange
            var fbUserId = 1234L;
            _uow.Setup(x => x.LinkedAccountRepository.GetOAuthAccountByProviderId(fbUserId, OAuthProvider.Facebook))
                .Returns(() => null);

            //Act
            _sut.UpdateUserEvents(fbUserId, 5);

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void NotDoAnythingIfUserDoesntWantEventsToBeSynced_OnUpdateUserEvents()
        {
            //Arrange
            var oauthAccount = new LinkedAccount
                               {
                                   AccessToken = "token",
                                   ProviderUserId = 1234,
                                   UserId = 111
                               };

            _uow.Setup(x => x.LinkedAccountRepository
                .GetOAuthAccountByProviderId(oauthAccount.ProviderUserId, OAuthProvider.Facebook))
                .Returns(() => oauthAccount);
            _uow.Setup(x => x.UserRepository.WantsFbEventsSynced(oauthAccount.UserId))
                .Returns(false);

            //Act
            _sut.UpdateUserEvents(oauthAccount.ProviderUserId, 5);

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void AddEventsIfTheyDontExists_OnUpdateUserEvents()
        {
            //Arrange
            var oauthAccount = new LinkedAccount
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

            _uow.Setup(x => x.LinkedAccountRepository
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
            _sut.UpdateUserEvents(oauthAccount.ProviderUserId, limit);

            //Assert
            Assert.AreEqual(oauthAccount.UserId, zazzEvent.UserId);
           _mockRepo.VerifyAll();
        }

        [Test]
        public void NotDoAnythingIfPageDoesntExists_OnUpdatePageEvents()
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
            _sut.UpdatePageEvents(page.FacebookId);

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void NotDoAnythingIfUserDoesntWantEventsToBeSynced_OnUpdatePageEvents()
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
            _sut.UpdatePageEvents(page.FacebookId);

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void AddEventsIfTheyDontExist_OnUpdatePageEvents()
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

            _uow.Setup(x => x.SaveChanges());

            //Act
            _sut.UpdatePageEvents(page.FacebookId, limit);

            //Assert
            Assert.AreEqual(page.Id, zazzEvent.PageId);
            _mockRepo.VerifyAll();
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
            _mockRepo.VerifyAll();
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
            _mockRepo.VerifyAll();
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
            _postService.Setup(x => x.NewPost(It.IsAny<Post>(), Enumerable.Empty<int>()));

            _uow.Setup(x => x.SaveChanges());

            //Act
            _sut.UpdatePageStatuses(page.FacebookId, limit);

            //Assert
            _mockRepo.VerifyAll();
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
                              FromUserId = page.UserId
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
            _uow.Setup(x => x.SaveChanges());

            //Act
            _sut.UpdatePageStatuses(page.FacebookId, limit);

            //Assert
            Assert.AreEqual(fbStatus.Message, oldPost.Message);

            _mockRepo.VerifyAll();
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
            _mockRepo.VerifyAll();
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
            _mockRepo.VerifyAll();
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

            _uow.Setup(x => x.SaveChanges());

            //Act
            _sut.UpdatePagePhotos(page.FacebookId, limit);

            //Assert
            Assert.AreEqual(fbPhoto.Description, oldPhoto.Description);

            _mockRepo.VerifyAll();
        }

        [Test, Ignore("Right now it has direct reference to HttpClient, and tries to download an actual image")]
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
            _photoService.Setup(x => x.SavePhoto(It.IsAny<Photo>(), Stream.Null, true, Enumerable.Empty<int>()))
                         .Returns(1);

            //Act
            _sut.UpdatePagePhotos(page.FacebookId, limit);

            //Assert

            _mockRepo.VerifyAll();
        }

        [Test, Ignore("Right now it has direct reference to HttpClient, and tries to download an actual image")]
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
            _photoService.Setup(x => x.SavePhoto(It.IsAny<Photo>(), Stream.Null, true, Enumerable.Empty<int>()))
                         .Returns(1);

            //Act
            _sut.UpdatePagePhotos(page.FacebookId, limit);

            //Assert

            _mockRepo.VerifyAll();
        }

        [Test]
        public void NotCallRepoAndReturnEmptyIfFbResultIsEmpty_OnFindZazzFbFriends()
        {
            //Arrange
            var accessToken = "token";
            _fbHelper.Setup(x => x.GetFriends(accessToken))
                     .Returns(Enumerable.Empty<FbFriend>());

            //Act
            var result = _sut.FindZazzFbFriends(accessToken);

            //Assert
            CollectionAssert.IsEmpty(result);
            _mockRepo.VerifyAll();
        }

        [Test]
        public void RetrunResultFromRepo_OnFindZazzFbFriends()
        {
            //Arrange
            var accessToken = "token";
            var fbFriends = new[]
                            {
                                new FbFriend {Id = 11},
                                new FbFriend {Id = 22},
                            };

            _fbHelper.Setup(x => x.GetFriends(accessToken))
                     .Returns(fbFriends);

            var users = new[]
                        {
                            new User {Id = 1},
                            new User {Id = 2},
                        };

            _uow.Setup(x => x.LinkedAccountRepository
                             .GetUsersByProviderId(It.IsAny<IEnumerable<long>>(), OAuthProvider.Facebook))
                .Returns(users.AsQueryable());

            //Act
            var result = _sut.FindZazzFbFriends(accessToken).ToList();

            //Assert
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.Any(u => u.Id == users[0].Id));
            Assert.IsTrue(result.Any(u => u.Id == users[1].Id));

            _mockRepo.VerifyAll();
        }
    }
}
