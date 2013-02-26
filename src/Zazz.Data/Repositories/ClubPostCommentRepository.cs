using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Data.Repositories
{
    public class ClubPostCommentRepository : BaseRepository<ClubPostComment>, IClubPostCommentRepository
    {
        public ClubPostCommentRepository(DbContext dbContext) : base(dbContext)
        {
        }

        protected override int GetItemId(ClubPostComment item)
        {
            throw new InvalidOperationException("You should always provide the id for updating the comment, if it's new then use insert graph."); //a user can have multiple comments on a single post.
        }

        public Task<IQueryable<ClubPostComment>> GetPostCommentsAsync(int postId)
        {
            return Task.Run(() => DbSet.Where(p => p.PostId == postId));
        }
    }
}