using Zazz.Core.Models.Data;

namespace Zazz.Web.OAuthAuthorizationServer
{
    public class OAuthCredentials
    {
        public JWT AccessToken { get; set; }

        public OAuthRefreshToken RefreshToken { get; set; }
    }
}