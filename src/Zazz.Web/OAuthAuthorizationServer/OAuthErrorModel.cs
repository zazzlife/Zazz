using Newtonsoft.Json;

namespace Zazz.Web.OAuthAuthorizationServer
{
    public class OAuthErrorModel
    {
        [JsonProperty("error")]
        public OAuthError Error { get; set; }

        [JsonProperty("error_description")]
        public string ErrorDescription { get; set; }
    }
}