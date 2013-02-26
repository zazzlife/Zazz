using System;
using System.Data.Entity;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Data.Repositories
{
    public class ValidationTokenRepository : BaseRepository<ValidationToken>, IValidationTokenRepository
    {
        public ValidationTokenRepository(DbContext dbContext) : base(dbContext)
        {
        }

        protected override int GetItemId(ValidationToken item)
        {
            throw new InvalidOperationException("You should always provide the id for updating the token, if it's new then use insert graph.");
        }
    }
}