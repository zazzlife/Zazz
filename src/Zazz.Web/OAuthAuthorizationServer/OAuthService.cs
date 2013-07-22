using System;
using System.Collections.Generic;
using System.Linq;
using Zazz.Core.Exceptions;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Repositories;
using Zazz.Core.Interfaces.Services;
using Zazz.Core.Models.Data;

namespace Zazz.Web.OAuthAuthorizationServer
{
    public class OAuthService : IOAuthService
    {
        private readonly IUoW _uow;
        private readonly ICryptoService _cryptoService;
        private readonly IStaticDataRepository _staticDataRepository;

        public OAuthService(IUoW uow, ICryptoService cryptoService, IStaticDataRepository staticDataRepository)
        {
            _uow = uow;
            _cryptoService = cryptoService;
            _staticDataRepository = staticDataRepository;
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
                                   TokenId = oAuthRefreshToken.Id,
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
            var jwt = new JWT(refreshToken);
            if (!jwt.TokenId.HasValue || //no token id
                jwt.ClientId == 0 || //no client id
                jwt.UserId == 0 || //no userId
                String.IsNullOrWhiteSpace(jwt.VerificationCode) || //no verification code
                jwt.TokenType != JWT.REFRESH_TOKEN_TYPE) // invalid token
            {
                throw new InvalidTokenException();
            }

            var dbRecord = _uow.OAuthRefreshTokenRepository.GetById(jwt.TokenId.Value);
            if (dbRecord == null)
                throw new InvalidTokenException();

            //checking verification code, client id and user id
            if (!dbRecord.VerificationCode.Equals(jwt.VerificationCode) ||
                dbRecord.OAuthClientId != jwt.ClientId ||
                dbRecord.UserId != jwt.UserId)
            {
                throw new InvalidTokenException();
            }

            //checking scopes
            var authorizedScopes = dbRecord.Scopes;
            var availableScopes = _staticDataRepository.GetOAuthScopes().ToList();
            foreach (var requestedScope in jwt.Scopes)
            {
                var scopeId = availableScopes
                    .Where(s => s.Name.Equals(requestedScope, StringComparison.InvariantCultureIgnoreCase))
                    .Select(s => s.Id)
                    .FirstOrDefault();

                if (!authorizedScopes.Any(s => s.ScopeId == scopeId))
                {
                    throw new InvalidTokenException();
                }
            }

            dbRecord.User.LastActivity = DateTime.UtcNow;
            _uow.SaveChanges();

            return new JWT
                   {
                       ClientId = dbRecord.OAuthClientId,
                       ExpirationDate = DateTime.UtcNow.AddHours(1),
                       IssuedDate = DateTime.UtcNow,
                       Scopes = jwt.Scopes,
                       TokenType = JWT.ACCESS_TOKEN_TYPE,
                       UserId = dbRecord.UserId,
                   };
        }
    }
}