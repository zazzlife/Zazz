using System;
using System.Collections.Generic;
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
            throw new System.NotImplementedException();
        }

        public JWT RefreshAccessToken(string refreshToken)
        {
            throw new System.NotImplementedException();
        }
    }
}