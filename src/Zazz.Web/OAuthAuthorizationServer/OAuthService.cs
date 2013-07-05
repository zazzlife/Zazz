using System;
using System.Collections.Generic;
using System.Linq;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Web.OAuthAuthorizationServer
{
    public class OAuthService : IOAuthService
    {
        private readonly IUoW _uow;
        private readonly ICryptoService _cryptoService;

        public OAuthService(IUoW uow, ICryptoService cryptoService)
        {
            _uow = uow;
            _cryptoService = cryptoService;
        }

        public OAuthCredentials CreateOAuthCredentials(User user, OAuthClient client, List<OAuthScope> scopes)
        {
            var scopesName = scopes.Select(s => s.Name).ToList();
            var verifyToken = Convert.ToBase64String(_cryptoService.GenerateKey(1024));

            var oAuthRefreshToken = new OAuthRefreshToken
                                    {
                                        CreatedDate = DateTime.UtcNow,
                                        OAuthClientId = client.Id,
                                        Scopes = scopes.Select(s => new OAuthRefreshTokenScope
                                                                    {
                                                                        ScopeId = s.Id
                                                                    }).ToList(),
                                        UserId = user.Id,
                                        VerificationCode = verifyToken
                                    };

            _uow.OAuthRefreshTokenRepository.InsertGraph(oAuthRefreshToken);
            _uow.SaveChanges();

            var accessToken = new JWT
                              {
                                  ClientId = client.Id,
                                  ExpirationDate = DateTime.UtcNow.AddHours(1),
                                  IssuedDate = DateTime.UtcNow,
                                  TokenType = JWT.ACCESS_TOKEN_TYPE,
                                  Scopes = scopesName,
                                  UserId = user.Id
                              };

            var refreshToken = new JWT
                               {
                                   ClientId = client.Id,
                                   IssuedDate = oAuthRefreshToken.CreatedDate,
                                   TokenType = JWT.REFRESH_TOKEN_TYPE,
                                   Scopes = scopesName,
                                   UserId = user.Id,
                                   VerificationCode = verifyToken
                               };
            
            

            return new OAuthCredentials
                   {
                       AccessToken = accessToken,
                       RefreshToken = refreshToken
                   };
        }

        public JWT RefreshAccessToken(string refreshToken)
        {
            throw new System.NotImplementedException();
        }
    }
}