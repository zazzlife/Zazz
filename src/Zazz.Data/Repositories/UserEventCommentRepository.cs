using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Data.Repositories
{
    public class UserEventCommentRepository : BaseRepository<Comment>, IUserEventCommentRepository
    {
        public UserEventCommentRepository(DbContext dbContext) : base(dbContext)
        {
        }

        protected override int GetItemId(Comment item)
        {
            throw new InvalidOperationException("You should always provide the id for updating the comment, if it's new then use insert graph."); //a user can have multiple comments on a single event.
        }

        public Task<IQueryable<Comment>> GetEventCommentsAsync(int eventId)
        {
            return Task.Run(() => DbSet.Where(c => c.PostId == eventId));
        }
    }
}