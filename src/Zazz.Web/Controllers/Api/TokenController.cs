using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using Newtonsoft.Json;
using Zazz.Core.Exceptions;
using Zazz.Core.Interfaces;
using Zazz.Core.Models;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;
using Zazz.Web.Models.Api;
using Zazz.Web.OAuthAuthorizationServer;

namespace Zazz.Web.Controllers.Api
{
    public class TokenController : ApiController
    {
        private readonly IUserService _userService;
        private readonly IPhotoService _photoService;
        private readonly IOAuthService _oauthService;
        private readonly IOAuthClientRepository _oauthClientRepository;
        private readonly ICryptoService _cryptoService;
        private readonly IStaticDataRepository _staticDataRepository;

        public TokenController(IUserService userService, IPhotoService photoService,
            IOAuthService oauthService, IOAuthClientRepository oauthClientRepository, ICryptoService cryptoService,
            IStaticDataRepository staticDataRepository)
        {
            _userService = userService;
            _photoService = photoService;
            _oauthService = oauthService;
            _oauthClientRepository = oauthClientRepository;
            _cryptoService = cryptoService;
            _staticDataRepository = staticDataRepository;
        }

        public OAuthAccessTokenResponse Post(OAuthAccessTokenRequest request)
        {
            if (request == null)
                throw new OAuthException(OAuthError.InvalidRequest);

            if (request.grant_type != GrantType.password && request.grant_type != GrantType.refresh_token)
                throw new OAuthException(OAuthError.UnsupportedGrantType);

            // checking password arguments
            if (request.grant_type == GrantType.password &&
                (
                    String.IsNullOrWhiteSpace(request.password) ||
                    String.IsNullOrWhiteSpace(request.username) ||
                    String.IsNullOrWhiteSpace(request.scope))
                )
            {
                throw new OAuthException(OAuthError.InvalidRequest);
            }

            // checking refresh token arguments
            if (request.grant_type == GrantType.refresh_token && String.IsNullOrWhiteSpace(request.refresh_token))
                throw new OAuthException(OAuthError.InvalidRequest);

            //authorizing client
            if (Request.Headers.Authorization == null ||
                String.IsNullOrWhiteSpace(Request.Headers.Authorization.Parameter))
                throw new OAuthException(OAuthError.InvalidClient);

            var clientId = Request.Headers.Authorization.Parameter;

            var client = _oauthClientRepository.GetById(clientId);
            if (client == null)
                throw new OAuthException(OAuthError.InvalidClient);


            if (request.grant_type == GrantType.password)
            {
                if (!client.IsAllowedToRequestPasswordGrantType)
                    throw new OAuthException(OAuthError.UnauthorizedClient);

                if (request.scope.Contains("full") && !client.IsAllowedToRequestFullScope)
                    throw new OAuthException(OAuthError.InvalidScope);

                // extracting scopes
                var requestsedScopes = request.scope.Split(',');
                var scopes = new List<OAuthScope>();

                foreach (var rs in requestsedScopes)
                {
                    var scope = _staticDataRepository
                        .GetOAuthScopes()
                        .SingleOrDefault(s => s.Name.Equals(rs, StringComparison.InvariantCultureIgnoreCase));

                    if (scope == null)
                        throw new OAuthException(OAuthError.InvalidScope);
                }
            
                // validating user credentials
                var user = _userService.GetUser(request.username);
                if (user == null)
                    throw new OAuthException(OAuthError.InvalidGrant);

                var password = _cryptoService.DecryptPassword(user.Password, user.PasswordIV);
                if (!password.Equals(request.password))
                    throw new OAuthException(OAuthError.InvalidGrant);

                var creds = _oauthService.CreateOAuthCredentials(user, client, scopes);

                return new OAuthAccessTokenResponse
                       {
                           AccessToken = creds.AccessToken.ToJWTString(),
                           TokenType = "Bearer",
                           RefreshToken = creds.RefreshToken.ToJWTString(),
                           ExpiresIn = 60*60,
                           User = new ApiBasicUserInfo
                                  {
                                      AccountType = user.AccountType,
                                      DisplayName = _userService.GetUserDisplayName(user.Id),
                                      DisplayPhoto = _photoService.GetUserImageUrl(user.Id),
                                      IsConfirmed = user.IsConfirmed,
                                      UserId = user.Id
                                  }

                       };
            }
            // refresh token
            else if (request.grant_type == GrantType.refresh_token)
            {
                try
                {
                    var accessToken = _oauthService.RefreshAccessToken(request.refresh_token);
                    return new OAuthAccessTokenResponse
                           {
                               AccessToken = accessToken.ToJWTString(),
                               TokenType = "Bearer",
                               ExpiresIn = 60*60
                           };
                }
                catch (InvalidTokenException)
                {
                    throw new OAuthException(OAuthError.InvalidGrant);
                }
            }
            else
            {
                throw new OAuthException(OAuthError.UnsupportedGrantType);
            }
        }
    }

    public class OAuthAccessTokenRequest
    {
        public GrantType grant_type { get; set; }

        public string refresh_token { get; set; }

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
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        public ApiBasicUserInfo User { get; set; }
    }
}