using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Security;
using System.Threading.Tasks;
using Facebook;
using Zazz.Core.Exceptions;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Services;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;
using Zazz.Core.Models.Facebook;
using Zazz.Infrastructure.Helpers;

namespace Zazz.Infrastructure.Services
{
    public class FacebookService : IFacebookService
    {
        private readonly IFacebookHelper _facebookHelper;
        private readonly IErrorHandler _errorHandler;
        private readonly IUoW _uow;
        private readonly IEventService _eventService;
        private readonly IPostService _postService;
        private readonly IPhotoService _photoService;
        private readonly IAlbumService _albumService;
        private readonly ILogger _logger;

        public FacebookService(IFacebookHelper facebookHelper, IErrorHandler errorHandler, IUoW uow,
                               IEventService eventService, IPostService postService, IPhotoService photoService,
                               IAlbumService albumService, ILogger logger)
        {
            _facebookHelper = facebookHelper;
            _errorHandler = errorHandler;
            _uow = uow;
            _eventService = eventService;
            _postService = postService;
            _photoService = photoService;
            _albumService = albumService;
            _logger = logger;
        }

        public void HandleRealtimeUserUpdatesAsync(FbUserChanges changes)
        {
            foreach (var entry in changes.Entries.Where(c => c.ChangedFields.Contains("events")))
                UpdateUserEvents(entry.UserId);

            _uow.SaveChanges();
        }

        public void UpdateUserEvents(long fbUserId, int limit = 5)
        {
            //getting user account
            var oauthAccount = _uow.LinkedAccountRepository
                                     .GetOAuthAccountByProviderId(fbUserId, OAuthProvider.Facebook);

            if (oauthAccount == null)
                return;

            //checking if the user wants to sync events
            if (!_uow.UserRepository.WantsFbEventsSynced(oauthAccount.UserId))
                return;

            // getting last 10 events from fb
            var events = _facebookHelper.GetEvents(fbUserId, oauthAccount.AccessToken, limit);
            foreach (var fbEvent in events)
            {
                //getting the current event that we have in db
                var dbEvent = _uow.EventRepository.GetByFacebookId(fbEvent.Id);
                var convertedEvent = _facebookHelper.FbEventToZazzEvent(fbEvent); //converting the fb event to our model
                //checking if we actually have the event in db or it's new
                if (dbEvent != null)
                {
                    // checking if the event has changed or not
                    if (!dbEvent.CreatedDate.Equals(convertedEvent.CreatedDate))
                    {
                        UpdateDbEvent(ref dbEvent, ref convertedEvent);
                    }
                }
                else
                {
                    //we don't have the event
                    convertedEvent.UserId = oauthAccount.UserId;
                    _eventService.CreateEvent(convertedEvent);
                }
            }
        }

        //public async Task HandleRealtimePageUpdatesAsync(FbPageChanges changes)
        //{
        //}

        public void SyncPageEvents(string pageId)
        {
            var page = _uow.FacebookPageRepository.GetByFacebookPageId(pageId);
            if (page == null)
                return;

            if (!_uow.UserRepository.WantsFbEventsSynced(page.UserId))
                return;

            var fbEvents = _facebookHelper.GetPageEvents(pageId, page.AccessToken);
            var dbEvents = _uow.EventRepository.GetPageEvents(page.Id).ToList();

            foreach (var fbEvent in fbEvents)
            {
                var dbEvent = dbEvents.SingleOrDefault(e => e.FacebookEventId == fbEvent.Id);
                
                var convertedEvent = _facebookHelper.FbEventToZazzEvent(fbEvent);
                convertedEvent.PageId = page.Id;

                if (dbEvent != null)
                {
                    UpdateDbEvent(ref dbEvent, ref convertedEvent);
                    dbEvents.Remove(dbEvent); // we remove the synced items from collection, if there is anything left, means it was deleted from fb
                }
                else
                {
                    convertedEvent.UserId = page.UserId;
                    _eventService.CreateEvent(convertedEvent);
                }
            }

            foreach (var e in dbEvents)
                _eventService.DeleteEvent(e.Id, page.UserId);

            _uow.SaveChanges(); // remove this if you save changes somewhere else
        }

        public void UpdateDbEvent(ref ZazzEvent dbEvent, ref ZazzEvent convertedEvent)
        {
            dbEvent.Name = convertedEvent.Name;
            dbEvent.Description = convertedEvent.Description;
            dbEvent.IsDateOnly = convertedEvent.IsDateOnly;
            dbEvent.CreatedDate = convertedEvent.CreatedDate;
            dbEvent.FacebookPhotoLink = convertedEvent.FacebookPhotoLink;
            dbEvent.Time = convertedEvent.Time;
            dbEvent.TimeUtc = convertedEvent.TimeUtc;
            dbEvent.Location = convertedEvent.Location;
            dbEvent.Street = convertedEvent.Street;
            dbEvent.City = convertedEvent.City;
            dbEvent.Latitude = convertedEvent.Latitude;
            dbEvent.Longitude = convertedEvent.Longitude;
        }

        public void SyncPageStatuses(string pageFbId)
        {
            var page = _uow.FacebookPageRepository.GetByFacebookPageId(pageFbId);
            if (page == null)
                return;

            if (!_uow.UserRepository.WantsFbPostsSynced(page.UserId))
                return;

            var fbStatuses = _facebookHelper.GetStatuses(page.AccessToken);
            var dbPosts = _uow.PostRepository.GetPagePosts(page.Id).ToList();

            foreach (var s in fbStatuses)
            {
                var dbPost = dbPosts.SingleOrDefault(p => p.FacebookId == s.Id);

                if (dbPost != null)
                {
                    dbPost.PageId = page.Id;
                    dbPost.Message = s.Message;
                    dbPosts.Remove(dbPost); // we remove the synced items from collection, if there is anything left, means it was deleted from fb
                }
                else
                {
                    var post = new Post
                               {
                                   FacebookId = s.Id,
                                   Message = s.Message,
                                   FromUserId = page.UserId,
                                   CreatedTime = s.Time.UnixTimestampToDateTime(),
                                   PageId = page.Id
                               };

                    _postService.NewPost(post, Enumerable.Empty<int>());
                }
            }

            // if there is anything left in the collection means, it's been deleted from fb.
            foreach (var dbPost in dbPosts)
                _postService.DeletePost(dbPost.Id, page.UserId);

            _uow.SaveChanges();
        }

        public void SyncPagePhotos(string pageId)
        {
            var page = _uow.FacebookPageRepository.GetByFacebookPageId(pageId);
            if (page == null)
                return;

            if (!_uow.UserRepository.WantsFbImagesSynced(page.UserId))
                return;

            var fbPhotos = _facebookHelper.GetPhotos(page.AccessToken);
            var dbPhotos = _uow.PhotoRepository.GetPagePhotos(page.Id).ToList();

            foreach (var fbPhoto in fbPhotos)
            {
                var dbPhoto = dbPhotos.SingleOrDefault(p => p.FacebookId == fbPhoto.Id);
                if (dbPhoto != null)
                {
                    dbPhoto.Description = fbPhoto.Description;
                    dbPhotos.Remove(dbPhoto); // we remove the synced items from collection, if there is anything left, means it was deleted from fb
                }
                else
                {
                    var album = _uow.AlbumRepository.GetByFacebookId(fbPhoto.AlbumId);
                    if (album == null)
                    {
                        album = new Album
                                {
                                    FacebookId = fbPhoto.AlbumId,
                                    IsFacebookAlbum = true,
                                    Name = _facebookHelper.GetAlbumName(fbPhoto.AlbumId, page.AccessToken),
                                    UserId = page.UserId,
                                    PageId = page.Id,
                                    CreatedDate = DateTime.UtcNow // TODO: use the facebook time instead.
                                };

                        _uow.AlbumRepository.InsertGraph(album);
                        _uow.SaveChanges();
                    }

                    var photo = new Photo
                                {
                                    AlbumId = album.Id,
                                    Description = fbPhoto.Description,
                                    FacebookId = fbPhoto.Id,
                                    IsFacebookPhoto = true,
                                    UploadDate = fbPhoto.CreatedTime.UnixTimestampToDateTime(),
                                    UserId = page.UserId,
                                    PageId = page.Id
                                };

                    //TODO: use async/await
                    using (var photoStream = new HttpClient().GetStreamAsync(fbPhoto.Source).Result)
                        _photoService.SavePhoto(photo, photoStream, true, Enumerable.Empty<int>());
                }
            }

            // if there is anything left in the collection means, it's been deleted from fb.
            foreach (var dbPhoto in dbPhotos)
                _photoService.RemovePhoto(dbPhoto.Id, page.UserId);

            _uow.SaveChanges();
        }

        public IEnumerable<FbPage> GetUserPages(int userId)
        {
            var oAuthAccount = _uow.LinkedAccountRepository.GetUserAccount(userId, OAuthProvider.Facebook);
            if (oAuthAccount == null)
                throw new OAuthAccountNotFoundException();

            return _facebookHelper.GetPages(oAuthAccount.AccessToken);
        }

        public void LinkPage(FacebookPage fbPage)
        {
            var page = _uow.FacebookPageRepository.GetByFacebookPageId(fbPage.FacebookId);
            if (page != null)
                throw new FacebookPageExistsException();

            _facebookHelper.LinkPage(fbPage.FacebookId, fbPage.AccessToken);
            _uow.FacebookPageRepository.InsertGraph(fbPage);
            _uow.SaveChanges();
        }

        public void UnlinkPage(string fbPageId, int currentUserId)
        {
            var page = _uow.FacebookPageRepository.GetByFacebookPageId(fbPageId);
            if (page == null)
                return;

            if (page.UserId != currentUserId)
                throw new SecurityException();

            //TODO: don't delete the records!

            var pageAlbums = _uow.AlbumRepository.GetPageAlbums(page.Id)
                .Select(a => a.Id)
                .ToList();

            var pagePhotos = _uow.PhotoRepository.GetPagePhotos(page.Id)
                                        .Select(p => p.Id);

            var pagePosts = _uow.PostRepository.GetPagePosts(page.Id)
                .Select(p => p.Id)
                .ToList();

            var pageEvents = _uow.EventRepository.GetPageEvents(page.Id)
                .Select(e => e.Id)
                .ToList();

            foreach (var a in pageAlbums)
                _albumService.DeleteAlbum(a, currentUserId);

            foreach (var p in pagePhotos)
                _photoService.RemovePhoto(p, currentUserId);

            foreach (var p in pagePosts)
                _postService.DeletePost(p, currentUserId);

            foreach (var e in pageEvents)
                _eventService.DeleteEvent(e, currentUserId);

            _uow.FacebookPageRepository.Remove(page);
            _uow.SaveChanges();
        }

        public void UpdatePagesAccessToken(int userId, string accessToken)
        {
            var pages = _uow.FacebookPageRepository.GetUserPages(userId)
                .ToList();

            if (pages.Count < 1)
                return;

            var updatedPages = _facebookHelper.GetPages(accessToken).ToList();

            foreach (var fbPage in pages)
            {
                var updatedPage = updatedPages.SingleOrDefault(p => p.Id == fbPage.FacebookId);
                
                if (updatedPage != null)
                {
                    fbPage.AccessToken = updatedPage.AcessToken;
                    fbPage.Name = updatedPage.Name;
                }
                else
                {
                    //page was deleted from fb.
                    var pageAlbums = _uow.AlbumRepository.GetPageAlbums(fbPage.Id)
                                         .Select(a => a.Id);

                    var pagePhotos = _uow.PhotoRepository.GetPagePhotos(fbPage.Id)
                                         .Select(p => p.Id);

                    var pagePosts = _uow.PostRepository.GetPagePosts(fbPage.Id)
                                        .Select(p => p.Id);

                    var pageEvents = _uow.EventRepository.GetPageEvents(fbPage.Id)
                                         .Select(e => e.Id);

                    foreach (var a in pageAlbums)
                        _albumService.DeleteAlbum(a, userId);

                    foreach (var p in pagePhotos)
                        _photoService.RemovePhoto(p, userId);

                    foreach (var p in pagePosts)
                        _postService.DeletePost(p, userId);

                    foreach (var e in pageEvents)
                        _eventService.DeleteEvent(e, userId);

                    _uow.FacebookPageRepository.Remove(fbPage);
                }
            }

            _uow.SaveChanges();
        }

        public IQueryable<User> FindZazzFbFriends(string accessToken)
        {
            var fbFriends = _facebookHelper.GetFriends(accessToken).ToList();
            if (fbFriends.Count == 0)
                return Enumerable.Empty<User>().AsQueryable();

            var u = _uow.LinkedAccountRepository.GetUsersByProviderId(fbFriends.Select(f => f.Id), OAuthProvider.Facebook);
            return u;
        }
    }
}