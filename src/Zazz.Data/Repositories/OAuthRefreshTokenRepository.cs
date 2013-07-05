using System;
using System.Data.Entity;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Data.Repositories
{
    public class OAuthRefreshTokenRepository : BaseRepository<OAuthRefreshToken>, IOAuthRefreshTokenRepository
    {
        public OAuthRefreshTokenRepository(DbContext dbContext) : base(dbContext)
        {}

        protected override int GetItemId(OAuthRefreshToken item)
        {
            throw new InvalidOperationException("You need to provide the id for updating this record, Use InsertGraph for inserting new record");
        }

        public OAuthRefreshToken Get(int userId, int clientId)
        {
            throw new NotImplementedException();
        }
    }
}