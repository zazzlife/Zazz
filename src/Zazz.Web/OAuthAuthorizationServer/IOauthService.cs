using System.Collections.Generic;
using Zazz.Core.Models.Data;

namespace Zazz.Web.OAuthAuthorizationServer
{
    public interface IOAuthService
    {
        OAuthCredentials CreateOAuthCredentials(User user, OAuthClient client, List<OAuthScope> scopes);

        JWT RefreshAccessToken(string refreshToken);
    }
}
