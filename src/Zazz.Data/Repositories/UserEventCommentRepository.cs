using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Data.Repositories
{
    public class UserEventCommentRepository : BaseRepository<UserEventComment>, IUserEventCommentRepository
    {
        public UserEventCommentRepository(DbContext dbContext) : base(dbContext)
        {
        }

        protected override int GetItemId(UserEventComment item)
        {
            throw new InvalidOperationException("You should always provide the id for updating the comment, if it's new then use insert graph."); //a user can have multiple comments on a single event.
        }

        public Task<IEnumerable<UserEventComment>> GetEventCommentsAsync(int eventId, int take = 0, int skip = 0)
        {
            return Task.Run(() =>
                                {
                                    var query = DbSet.Where(c => c.UserEventId == eventId);

                                    if (skip != 0)
                                        query = query.Skip(skip);

                                    if (take != 0)
                                        query = query.OrderBy(c => c.Date).Take(take);

                                    return query.AsEnumerable();
                                });
        }
    }
}