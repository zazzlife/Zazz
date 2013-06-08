using System;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Infrastructure.Services
{
    public class AppRequestTokenService : IAppRequestTokenService
    {
        private readonly IUoW _uow;
        private readonly ICryptoService _cryptoService;

        public AppRequestTokenService(IUoW uow, ICryptoService cryptoService)
        {
            _uow = uow;
            _cryptoService = cryptoService;
        }

        public AppRequestToken Create(int appId)
        {
            const int KEY_SIZE = 128; //bits
            var token = new AppRequestToken
                        {
                            AppId = appId,
                            ExpirationTime = DateTime.UtcNow.AddHours(1),
                            Token = _cryptoService.GenerateKey(KEY_SIZE)
                        };

            _uow.AppRequestTokenRepository.InsertGraph(token);
            _uow.SaveChanges();

            return token;
        }

        public AppRequestToken Get(int requestId)
        {
            throw new System.NotImplementedException();
        }

        public void Remove(int requestId)
        {
            throw new System.NotImplementedException();
        }
    }
}