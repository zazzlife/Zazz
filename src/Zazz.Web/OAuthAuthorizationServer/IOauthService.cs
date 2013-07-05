using Zazz.Core.Models.Data;

namespace Zazz.Web.OAuthAuthorizationServer
{
    public interface IOAuthService
    {
        OAuthCredentials CreateOAuthCredentials(User user, OAuthClient client);

        JWT RefreshAccessToken(string refreshToken);
    }
}
