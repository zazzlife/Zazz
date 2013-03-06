using System;
using System.Security;
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

        public async Task UpdateClubAsync(Club club, int currentUserId)
        {
            if (club.Id == 0)
                throw new ArgumentException("Club Id cannot be 0");

            var isAuthorized = await IsUserAuthorized(currentUserId, club.Id);
            if (!isAuthorized)
                throw new SecurityException();

            _uow.ClubRepository.InsertOrUpdate(club);
            await _uow.SaveAsync();
        }

        public void Dispose()
        {
            _uow.Dispose();
        }
    }
}