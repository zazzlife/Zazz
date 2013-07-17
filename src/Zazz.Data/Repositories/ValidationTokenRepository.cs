using System;
using System.Data.Entity;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Data.Repositories
{
    public class ValidationTokenRepository : BaseRepository<UserValidationToken>, IValidationTokenRepository
    {
        public ValidationTokenRepository(DbContext dbContext) : base(dbContext)
        {
        }
    }
}