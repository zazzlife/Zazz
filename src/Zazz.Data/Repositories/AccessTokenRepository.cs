using System;
using System.Data.Entity;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Data.Repositories
{
    public class AccessTokenRepository : BaseRepository<AccessToken>, IAccessTokenRepository
    {
        public AccessTokenRepository(DbContext dbContext) : base(dbContext)
        {}

        protected override int GetItemId(AccessToken item)
        {
            throw new InvalidOperationException("You must provide the Id for updating. If you need to insert use InsertGraph");
        }
    }
}