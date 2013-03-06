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

        public async Task CreateClubAsync(Club club)
        {
            _uow.ClubRepository.InsertGraph(club);
            await _uow.SaveAsync();
        }

        public Task<bool> IsUserAuthorized(int userId, int clubId)
        {
            return _uow.ClubAdminRepository.ExistsAsync(userId, clubId);
        }

        public async Task UpdateClubAsync(Club club)
        {
        }

        public void Dispose()
        {
            _uow.Dispose();
        }
    }
}