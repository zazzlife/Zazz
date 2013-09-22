using System.Data;
using System.Data.Entity;
using System.Linq;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Repositories;
using Zazz.Core.Models.Data;

namespace Zazz.Data.Repositories
{
    public class PhotoLikeRepository : IPhotoLikeRepository
    {
        private readonly ZazzDbContext _context;
        private readonly IDbSet<PhotoLike> _dbSet;

        public PhotoLikeRepository(ZazzDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<PhotoLike>();
        }

        public void InsertGraph(PhotoLike photoLike)
        {
            _dbSet.Add(photoLike);
        }

        public bool Exists(int photoId, int userId)
        {
            return _dbSet
                .Where(v => v.PhotoId == photoId)
                .Where(v => v.UserId == userId)
                .Any();
        }

        public int GetLikesCount(int photoId)
        {
            return _dbSet.Count(v => v.PhotoId == photoId);
        }

        public void Remove(PhotoLike photoLike)
        {
            _context.Entry(photoLike).State = EntityState.Deleted;
        }

        public void Remove(int photoId, int userId)
        {
            var like = _dbSet.Where(v => v.PhotoId == photoId)
                             .Where(v => v.UserId == userId)
                             .SingleOrDefault();

            if (like == null)
                return;

            Remove(like);
        }
    }
}