using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Web.OAuthAuthorizationServer
{
    public class OAuthService : IOAuthService
    {
        private readonly IUoW _uow;

        public OAuthService(IUoW uow)
        {
            _uow = uow;
        }

        public OAuthCredentials CreateOAuthCredentials(User user, OAuthClient client)
        {
            throw new System.NotImplementedException();
        }

        public JWT RefreshAccessToken(string refreshToken)
        {
            throw new System.NotImplementedException();
        }
    }
}