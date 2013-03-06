using System.Threading.Tasks;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Infrastructure.Services
{
    public class ClubService : IClubService
    {
        private readonly IUoW _uow;

        public ClubService(IUoW uow)
        {
            _uow = uow;
        }

        public Task CreateClubAsync(Club club)
        {
            throw new System.NotImplementedException();
        }

        public Task UpdateClubAsync(Club club)
        {
            throw new System.NotImplementedException();
        }

        public void Dispose()
        {
            _uow.Dispose();
        }
    }
}