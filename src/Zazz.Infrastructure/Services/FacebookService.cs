using System;
using System.Globalization;
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

        public FacebookService(IFacebookHelper facebookHelper, IErrorHandler errorHandler, IUoW uow,
                               IEventService eventService)
        {
            _facebookHelper = facebookHelper;
            _errorHandler = errorHandler;
            _uow = uow;
            _eventService = eventService;
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

        public async Task HandleRealtimePageUpdatesAsync(FbPageChanges changes)
        {
        }

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

        public async Task<IEnumerable<FbPage>> GetUserPagesAsync(int userId)
        {
            var oAuthAccount = await _uow.OAuthAccountRepository.GetUserAccountAsync(userId, OAuthProvider.Facebook);

            if (oAuthAccount == null)
                throw new OAuthAccountNotFoundException();

            return _facebookHelper.GetPages(oAuthAccount.AccessToken);
        }

        public Task<FbUser> GetUserAsync(string id, string accessToken = null)
        {
            try
            {
                _facebookHelper.SetAccessToken(accessToken);
                return _facebookHelper.GetAsync<FbUser>(id, "email");
            }
            catch (FacebookOAuthException)
            {
                _errorHandler.HandleAccessTokenExpiredAsync(id, OAuthProvider.Facebook).Wait();
                throw;
            }
            catch (FacebookApiLimitException)
            {
                _errorHandler.HandleFacebookApiLimitReachedAsync(id, "", "").Wait();
                throw;
            }
            catch (Exception e)
            {
                _errorHandler.LogException("Fb user id: " + id, "FacebookService.GetUserAsync", e);
                throw;
            }
        }

        public Task<FbEvent> GetEventAsync(string id, string accessToken = null)
        {
            throw new System.NotImplementedException();
        }

        public Task<FbPost> GetPostAsync(string id, string accessToken = null)
        {
            throw new System.NotImplementedException();
        }

        public Task<string> GetPictureAsync(string objectId, int width, int height, string accessToken = null)
        {
            throw new System.NotImplementedException();
        }

        public Task<string> GetPictureAsync(string objectId, PictureSize pictureSize, string accessToken = null)
        {
            throw new System.NotImplementedException();
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