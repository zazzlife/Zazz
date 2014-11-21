using System.Data;
using System.Data.Entity;
using System.Linq;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Repositories;
using Zazz.Core.Models.Data;

namespace Zazz.Data.Repositories
{
    public class PostLikeRepository : IPostLikeRepository
    {
        private readonly ZazzDbContext _context;
        private readonly IDbSet<PostLike> _dbSet;

        public PostLikeRepository(ZazzDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<PostLike>();
        }

        public void InsertGraph(PostLike postLike)
        {
            _dbSet.Add(postLike);
        }

        public bool Exists(int postId, int userId)
        {
            return _dbSet
                .Where(v => v.PostId == postId)
                .Where(v => v.UserId == userId)
                .Any();
        }

        public int GetLikesCount(int postId)
        {
            return _dbSet.Count(v => v.PostId == postId);
        }

        public void Remove(PostLike postLike)
        {
            _context.Entry(postLike).State = EntityState.Deleted;
        }

        public void Remove(int postId, int userId)
        {
            var like = _dbSet.Where(v => v.PostId == postId)
                             .Where(v => v.UserId == userId)
                             .SingleOrDefault();

            if (like == null)
                return;

            Remove(like);
        }
    }
}
