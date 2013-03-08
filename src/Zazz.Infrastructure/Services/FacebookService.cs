using System;
using System.Threading.Tasks;
using Facebook;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Facebook;

namespace Zazz.Infrastructure.Services
{
    public class FacebookService : IFacebookService
    {
        private readonly IFacebookHelper _facebookHelper;

        public FacebookService(IFacebookHelper facebookHelper)
        {
            _facebookHelper = facebookHelper;
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
                throw new NotImplementedException();
            }
            catch (FacebookApiLimitException)
            {
                throw new NotImplementedException();
            }
            catch (FacebookApiException)
            {
                throw new NotImplementedException();
            }
            catch (Exception)
            {
                throw new NotImplementedException();
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