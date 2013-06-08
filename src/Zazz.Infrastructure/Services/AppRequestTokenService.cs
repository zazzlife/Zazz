using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Infrastructure.Services
{
    public class AppRequestTokenService : IAppRequestTokenService
    {
        private readonly IUoW _uow;

        public AppRequestTokenService(IUoW uow)
        {
            _uow = uow;
        }

        public AppRequestToken Create(int appId)
        {
            throw new System.NotImplementedException();
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