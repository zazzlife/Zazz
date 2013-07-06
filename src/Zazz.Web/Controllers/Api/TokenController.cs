using System;
using System.Net;
using System.Web.Http;
using Newtonsoft.Json;
using Zazz.Core.Interfaces;
using Zazz.Web.OAuthAuthorizationServer;

namespace Zazz.Web.Controllers.Api
{
    public class TokenController : ApiController
    {
        private readonly IUserService _userService;
        private readonly IPhotoService _photoService;
        private readonly IOAuthService _oauthService;
        private readonly IOAuthClientRepository _oauthClientRepository;

        public TokenController(IUserService userService, IPhotoService photoService,
            IOAuthService oauthService, IOAuthClientRepository oauthClientRepository)
        {
            _userService = userService;
            _photoService = photoService;
            _oauthService = oauthService;
            _oauthClientRepository = oauthClientRepository;
        }

        public OAuthAccessTokenResponse Post(OAuthAccessTokenRequest request)
        {
            if (request == null)
                throw new OAuthException(OAuthError.InvalidRequest);

            if (request.grant_type == GrantType.password &&
                (
                    String.IsNullOrWhiteSpace(request.password) ||
                    String.IsNullOrWhiteSpace(request.username) ||
                    String.IsNullOrWhiteSpace(request.scope))
                )
            {
                throw new OAuthException(OAuthError.InvalidRequest);
            }

            //authorizing client
            if (Request.Headers.Authorization == null ||
                String.IsNullOrWhiteSpace(Request.Headers.Authorization.Parameter))
            {
                throw new OAuthException(OAuthError.InvalidClient);
            }
            else
            {
                var clientId = Request.Headers.Authorization.Parameter;
                
                var client = _oauthClientRepository.GetById(clientId);
                if (client == null)
                    throw new OAuthException(OAuthError.InvalidClient);
            }

            // password grant type
            if (request.grant_type == GrantType.password)
            {
                
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