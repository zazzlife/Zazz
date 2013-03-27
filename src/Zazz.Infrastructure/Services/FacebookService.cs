using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Facebook;
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

        public FacebookService(IFacebookHelper facebookHelper, IErrorHandler errorHandler, IUoW uow)
        {
            _facebookHelper = facebookHelper;
            _errorHandler = errorHandler;
            _uow = uow;
        }

        public async Task HandleRealtimeUserUpdatesAsync(FbUserChanges changes)
        {
            var tasks = new List<Task>();
            foreach (var entry in changes.Entries)
                tasks.Add(UpdateUserEvents(entry));

            await Task.WhenAll(tasks);
            await _uow.SaveAsync();
        }

        private async Task UpdateUserEvents(FbUserChangesEntry entry)
        {
            if (entry.ChangedFields.Contains("events"))
            {
                //getting user account
                var oauthAccount = _uow.OAuthAccountRepository
                                         .GetOAuthAccountByProviderId(entry.UserId, OAuthProvider.Facebook);
                if (!oauthAccount.User.UserDetail.SyncFbEvents) //checking if the user wants to sync events
                    return;

                // getting last 10 events from fb
                var events = await _facebookHelper.GetEventsAsync(entry.UserId, oauthAccount.AccessToken);
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
                    }
                    else
                    {
                        //we don't have the event
                        convertedEvent.UserId = oauthAccount.UserId;
                        _uow.EventRepository.InsertGraph(convertedEvent);
                    }
                }
            }
        }

        public async Task HandleRealtimePageUpdatesAsync(FbPageChanges changes)
        {
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
    }
}