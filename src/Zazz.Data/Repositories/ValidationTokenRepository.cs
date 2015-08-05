using System;
using System.Data.Entity;
using System.Linq;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Repositories;
using Zazz.Core.Models.Data;

namespace Zazz.Data.Repositories
{
    public class ValidationTokenRepository : BaseRepository<UserValidationToken>, IValidationTokenRepository
    {
        public ValidationTokenRepository(DbContext dbContext) : base(dbContext)
        {
        }

        public IQueryable<UserValidationToken> GetValidationTokens(int userid)
        {
            return (from token in DbSet
                    where token.User.Id == userid
                    select token);
        }
    }
}