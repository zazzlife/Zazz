using System;
using System.Net;
using System.Web.Http;

namespace Zazz.Web.Controllers.Api
{
    public class TokenController : ApiController
    {
        public OAuthAccessTokenResponse Post(OAuthAccessTokenRequest request)
        {
            if (request == null)
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            throw new NotImplementedException();
        }
    }

    public class OAuthAccessTokenRequest
    {}

    public class OAuthAccessTokenResponse
    {}
}