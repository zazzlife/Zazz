using System.Collections.Generic;
using System.Data.Entity;
using System.Threading.Tasks;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Data.Repositories
{
    public class OAuthAccountRepository : BaseRepository<OAuthAccount>, IOAuthAccountRepository
    {
        public OAuthAccountRepository(DbContext dbContext) : base(dbContext)
        {
        }

        protected override int GetItemId(OAuthAccount item)
        {
            throw new System.NotImplementedException();
        }

        public Task<OAuthAccount> GetUserAccount(int userId, OAuthProvider provider)
        {
            throw new System.NotImplementedException();
        }

        public Task<IEnumerable<OAuthAccount>> GetUserAccounts(int userId)
        {
            throw new System.NotImplementedException();
        }

        public void Remove(int userId, OAuthProvider provider)
        {
            throw new System.NotImplementedException();
        }
    }
}