namespace Zazz.Web.OAuthAuthorizationServer
{
    public class OAuthCredentials
    {
        public JWT AccessToken { get; set; }

        public string RefreshToken { get; set; }
    }
}