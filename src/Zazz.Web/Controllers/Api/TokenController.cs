using System;
using System.Net;
using System.Web.Http;
using Newtonsoft.Json;
using Zazz.Web.OAuthAuthorizationServer;

namespace Zazz.Web.Controllers.Api
{
    public class TokenController : ApiController
    {
        public OAuthAccessTokenResponse Post(OAuthAccessTokenRequest request)
        {
            if (request == null)
                throw new OAuthException(OAuthError.InvalidRequest);

            if (request.grant_type == GrantType.password && (String.IsNullOrWhiteSpace(request.password) ||
                String.IsNullOrWhiteSpace(request.username) ||
                String.IsNullOrWhiteSpace(request.scope)))
            {
                throw new OAuthException(OAuthError.InvalidRequest);
            }
                

            throw new NotImplementedException();
        }
    }

    public class OAuthAccessTokenRequest
    {
        public GrantType grant_type { get; set; }

        public string username { get; set; }

        public string password { get; set; }

        public string scope { get; set; }
    }

    public enum GrantType
    {
        undefined,
        password,
        refresh_token
    }

    public class OAuthAccessTokenResponse
    {
        
    }
}