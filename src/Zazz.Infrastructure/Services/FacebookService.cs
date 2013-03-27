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
                var oauthAccount = await _uow.OAuthAccountRepository
                                         .GetOAuthAccountByProviderIdAsync(entry.UserId, OAuthProvider.Facebook);
                if (!oauthAccount.User.UserDetail.SyncFbEvents)
                    return;

                var events = await _facebookHelper.GetEventsAsync(entry.UserId, oauthAccount.AccessToken);
                foreach (var fbEvent in events)
                {
                    throw new NotImplementedException();
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