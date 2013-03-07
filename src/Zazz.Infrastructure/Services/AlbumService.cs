using System.Threading.Tasks;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Infrastructure.Services
{
    public class AlbumService : IAlbumService
    {
        private readonly IUoW _uoW;

        public AlbumService(IUoW uoW)
        {
            _uoW = uoW;
        }

        public async Task CreateAlbumAsync(Album album)
        {
            _uoW.AlbumRepository.InsertGraph(album);

            await _uoW.SaveAsync();
        }

        public Task UpdateAlbumAsync(Album album, int currentUserId)
        {
            throw new System.NotImplementedException();
        }

        public Task DeleteAlbumAsync(int albumId, int currentUserId)
        {
            throw new System.NotImplementedException();
        }

        public void Dispose()
        {
            _uoW.Dispose();
        }
    }
}