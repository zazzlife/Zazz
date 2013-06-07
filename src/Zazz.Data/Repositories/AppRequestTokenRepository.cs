using System.Data.Entity;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Data.Repositories
{
    public class AppRequestTokenRepository : BaseLongRepository<AppRequestToken>, IAppRequestTokenRepository
    {
        public AppRequestTokenRepository(DbContext dbContext) : base(dbContext)
        {}
    }
}