namespace Zazz.Web.OAuthAuthorizationServer
{
    public class OAuthCredentials
    {
        public JWT AccessToken { get; set; }

        public JWT RefreshToken { get; set; }
    }
}