using System;
using System.Data.Entity;
using System.Linq;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Data.Repositories
{
    public class OAuthRefreshTokenRepository : BaseRepository<OAuthRefreshToken>, IOAuthRefreshTokenRepository
    {
        public OAuthRefreshTokenRepository(DbContext dbContext) : base(dbContext)
        {}

        public override OAuthRefreshToken GetById(int id)
        {
            return DbSet
                .Include(t => t.User)
                .Include(t => t.Scopes)
                .SingleOrDefault(t => t.Id == id);
        }
    }
}