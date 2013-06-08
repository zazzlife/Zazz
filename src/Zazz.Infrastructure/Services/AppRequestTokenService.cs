using System;
using Zazz.Core.Exceptions;
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
            if (appId == 0)
                throw new ArgumentOutOfRangeException("appId");

            const int KEY_SIZE = 64 * 8; // 64 bytes
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

        public AppRequestToken Get(long requestId)
        {
            if (requestId == 0)
                throw new ArgumentOutOfRangeException("requestId");

            var token = _uow.AppRequestTokenRepository.GetById(requestId);
            if (token == null)
                throw new NotFoundException();

            return token;
        }

        public void Remove(long requestId)
        {
            if (requestId == 0)
                throw new ArgumentOutOfRangeException("requestId");

            _uow.AppRequestTokenRepository.Remove(requestId);
            _uow.SaveChanges();
        }

        public void Remove(AppRequestToken request)
        {
            _uow.AppRequestTokenRepository.Remove(request);
            _uow.SaveChanges();
        }
    }
}