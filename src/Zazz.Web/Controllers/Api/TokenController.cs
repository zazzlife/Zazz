using System.Web.Http;

namespace Zazz.Web.Controllers.Api
{
    public class TokenController : ApiController
    {
        public OAuthAccessTokenResponse Post(OAuthAccessTokenRequest request)
        {
            
        }
    }

    public class OAuthAccessTokenRequest
    {}

    public class OAuthAccessTokenResponse
    {}
}