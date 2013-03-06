using System;
using System.Data.Entity;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Data.Repositories
{
    public class UserFeedRepository : BaseRepository<UserFeed>, IUserFeedRepository
    {
        public UserFeedRepository(DbContext dbContext) : base(dbContext)
        {
        }

        protected override int GetItemId(UserFeed item)
        {
            throw new InvalidOperationException("You need to provide the id for updating this record, Use InsertGraph for inserting new record");
        }
    }
}