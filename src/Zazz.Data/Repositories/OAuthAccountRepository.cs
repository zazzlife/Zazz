using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
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
            if (item.UserId == default (int))
                throw new ArgumentException("User id cannot be 0");

            return DbSet.Where(o => o.UserId == item.UserId)
                        .Where(o => o.OAuthProvider == item.OAuthProvider)
                        .Select(o => o.Id)
                        .SingleOrDefault();
        }

        public Task<OAuthAccount> GetUserAccountAsync(int userId, OAuthProvider provider)
        {
            return Task.Run(() => DbSet.Where(o => o.UserId == userId)
                                       .Where(o => o.OAuthProvider == provider)
                                       .SingleOrDefault());
        }

        public Task<IEnumerable<OAuthAccount>> GetUserAccountsAsync(int userId)
        {
            return Task.Run(() => DbSet.Where(o => o.UserId == userId).AsEnumerable());
        }

        public Task<OAuthAccount> GetOAuthAccountByProviderId(long providerUserId, OAuthProvider provider)
        {
            return Task.Run(() => DbSet.Where(o => o.OAuthProvider == provider)
                                       .Where(o => o.ProviderUserId == providerUserId)
                                       .SingleOrDefault());
        }

        public Task<bool> OAuthAccountExistsAsync(long providerUserId, OAuthProvider provider)
        {
            return Task.Run(() => DbSet.Where(o => o.ProviderUserId == providerUserId)
                                       .Where(o => o.OAuthProvider == provider)
                                       .Any());
        }

        public async Task RemoveAsync(int userId, OAuthProvider provider)
        {
            var item = await GetUserAccountAsync(userId, provider);
            if (item != null)
                DbContext.Entry(item).State = EntityState.Deleted;
        }
    }
}