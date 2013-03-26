using System;
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

        public FacebookService(IFacebookHelper facebookHelper, IErrorHandler errorHandler)
        {
            _facebookHelper = facebookHelper;
            _errorHandler = errorHandler;
        }

        public Task HandleRealtimeUpdatesAsync(FbUserChanges userChanges)
        {
            throw new NotImplementedException();
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