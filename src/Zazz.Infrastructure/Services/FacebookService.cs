using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Security;
using System.Threading.Tasks;
using Facebook;
using Zazz.Core.Exceptions;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Facebook;

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

        public FacebookService(IFacebookHelper facebookHelper, IErrorHandler errorHandler, IUoW uow,
                               IEventService eventService, IPostService postService, IPhotoService photoService)
        {
            _facebookHelper = facebookHelper;
            _errorHandler = errorHandler;
            _uow = uow;
            _eventService = eventService;
            _postService = postService;
            _photoService = photoService;
        }

        public async Task HandleRealtimeUserUpdatesAsync(FbUserChanges changes)
        {
            var tasks = new List<Task>();
            foreach (var entry in changes.Entries.Where(c => c.ChangedFields.Contains("events")))
                tasks.Add(UpdateUserEventsAsync(entry.UserId));

            await Task.WhenAll(tasks);
            _uow.SaveChanges();
        }

        public async Task UpdateUserEventsAsync(long fbUserId)
        {
            //getting user account
            var oauthAccount = _uow.OAuthAccountRepository
                                     .GetOAuthAccountByProviderId(fbUserId, OAuthProvider.Facebook);

            if (oauthAccount == null)
                return;

            //checking if the user wants to sync events
            if (!_uow.UserRepository.WantsFbEventsSynced(oauthAccount.UserId))
                return;

            // getting last 10 events from fb
            var events = _facebookHelper.GetEvents(fbUserId, oauthAccount.AccessToken);
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
                    await _eventService.CreateEventAsync(convertedEvent);
                }
            }
        }

        //public async Task HandleRealtimePageUpdatesAsync(FbPageChanges changes)
        //{
        //}

        public async Task UpdatePageEventsAsync(string pageId)
        {
            var page = _uow.FacebookPageRepository.GetByFacebookPageId(pageId);
            if (page == null)
                return;

            if (!_uow.UserRepository.WantsFbEventsSynced(page.UserId))
                return;

            var fbEvents = _facebookHelper.GetPageEvents(pageId, page.AccessToken);
            foreach (var fbEvent in fbEvents)
            {
                var dbEvent = _uow.EventRepository.GetByFacebookId(fbEvent.Id);
                var convertedEvent = _facebookHelper.FbEventToZazzEvent(fbEvent);

                if (dbEvent != null)
                {
                    if (!dbEvent.CreatedDate.Equals(convertedEvent.CreatedDate))
                    {
                        UpdateDbEvent(ref dbEvent, ref convertedEvent);
                    }
                }
                else
                {
                    convertedEvent.UserId = page.UserId;
                    await _eventService.CreateEventAsync(convertedEvent);
                }
            }

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

        public void UpdatePageStatuses(string pageId)
        {
            var page = _uow.FacebookPageRepository.GetByFacebookPageId(pageId);
            if (page == null)
                return;

            if (!_uow.UserRepository.WantsFbPostsSynced(page.UserId))
                return;

            var statuses = _facebookHelper.GetStatuses(page.AccessToken, 25);
            foreach (var s in statuses)
            {
                var dbPost = _uow.PostRepository.GetByFbId(s.Id);
                if (dbPost != null)
                {
                    dbPost.Message = s.Message;
                }
                else
                {
                    var post = new Post
                               {
                                   FacebookId = s.Id,
                                   Message = s.Message,
                                   UserId = page.UserId,
                                   CreatedTime = s.Time.UnixTimestampToDateTime()
                               };

                    _postService.NewPostAsync(post);
                }
            }

            _uow.SaveChanges();
        }

        public void UpdatePagePhotos(string pageId)
        {
            var page = _uow.FacebookPageRepository.GetByFacebookPageId(pageId);
            if (page == null)
                return;

            if (!_uow.UserRepository.WantsFbImagesSynced(page.UserId))
                return;

            var photos = _facebookHelper.GetPhotos(page.AccessToken, 25);
            foreach (var fbPhoto in photos)
            {
                var dbPhoto = _uow.PhotoRepository.GetByFacebookId(fbPhoto.Id);
                if (dbPhoto != null)
                {
                    dbPhoto.FacebookLink = fbPhoto.Source;
                    dbPhoto.Description = fbPhoto.Description;
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
                                    UserId = page.UserId
                                };

                        _uow.AlbumRepository.InsertGraph(album);
                        _uow.SaveChanges();
                    }

                    var photo = new Photo
                                {
                                    AlbumId = album.Id,
                                    Description = fbPhoto.Description,
                                    FacebookId = fbPhoto.Id,
                                    FacebookLink = fbPhoto.Source,
                                    IsFacebookPhoto = true,
                                    UploadDate = fbPhoto.CreatedTime.UnixTimestampToDateTime(),
                                    UploaderId = page.UserId
                                };

                    _photoService.SavePhotoAsync(photo, Stream.Null, true);
                }
            }

            _uow.SaveChanges();
        }

        public async Task<IEnumerable<FbPage>> GetUserPagesAsync(int userId)
        {
            var oAuthAccount = await _uow.OAuthAccountRepository.GetUserAccountAsync(userId, OAuthProvider.Facebook);

            if (oAuthAccount == null)
                throw new OAuthAccountNotFoundException();

            return _facebookHelper.GetPages(oAuthAccount.AccessToken);
        }

        public void LinkPage(FacebookPage fbPage)
        {
            var page = _uow.FacebookPageRepository.GetByFacebookPageId(fbPage.FacebookId);
            if (page != null)
                throw new FacebookPageExistsException();

            //_facebookHelper.LinkPage(fbPage.FacebookId, fbPage.AccessToken);
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

            _uow.FacebookPageRepository.Remove(page);
            _uow.SaveChanges();
        }

        public void Dispose()
        {
            _uow.Dispose();
            _eventService.Dispose();
        }
    }
}