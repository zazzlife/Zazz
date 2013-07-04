using Zazz.Core.Models.Data;
using Zazz.Web.OAuthAuthorizationServer.JsonWebToken;

namespace Zazz.Web.OAuthAuthorizationServer
{
    public interface IOAuthService
    {
        OAuthCredentials CreateOAuthCredentials(User user);

        JWT RefreshAccessToken(string refreshToken);
    }
}
