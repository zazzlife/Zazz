using System.Data.Entity;
using System.Linq;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Data.Repositories
{
    public class PhotoVoteRepository : IPhotoVoteRepository
    {
        private readonly ZazzDbContext _context;
        private readonly IDbSet<PhotoVote> _dbSet;

        public PhotoVoteRepository(ZazzDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<PhotoVote>();
        }

        public void InsertGraph(PhotoVote vote)
        {
            _dbSet.Add(vote);
        }

        public bool Exists(int photoId, int userId)
        {
            return _dbSet
                .Where(v => v.PhotoId == photoId)
                .Where(v => v.UserId == userId)
                .Any();
        }

        public int GetVotesCount(int photoId)
        {
            return _dbSet.Count(v => v.PhotoId == photoId);
        }

        public void Remove(PhotoVote vote)
        {
            throw new System.NotImplementedException();
        }

        public void Remove(int photoId, int userId)
        {
            throw new System.NotImplementedException();
        }
    }
}