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
                        .Where(o => o.Provider == item.Provider)
                        .Where(o => o.ProviderUserId == item.ProviderUserId)
                        .Select(o => o.Id)
                        .SingleOrDefault();
        }

        public OAuthAccount GetUserAccount(int userId, OAuthProvider provider)
        {
            return DbSet.Where(o => o.UserId == userId)
                        .Where(o => o.Provider == provider)
                        .SingleOrDefault();
        }

        public IEnumerable<OAuthAccount> GetUserAccounts(int userId)
        {
            return DbSet.Where(o => o.UserId == userId).AsEnumerable();
        }

        public OAuthAccount GetOAuthAccountByProviderId(long providerUserId, OAuthProvider provider)
        {
            return DbSet.Where(o => o.Provider == provider)
                        .Where(o => o.ProviderUserId == providerUserId)
                        .SingleOrDefault();
        }

        public bool OAuthAccountExists(long providerUserId, OAuthProvider provider)
        {
            return DbSet.Where(o => o.ProviderUserId == providerUserId)
                        .Where(o => o.Provider == provider)
                        .Any();
        }

        public string GetAccessToken(int userId, OAuthProvider provider)
        {
            return DbSet
                .Where(a => a.UserId == userId)
                .Where(a => a.Provider == OAuthProvider.Facebook)
                .Select(a => a.AccessToken)
                .SingleOrDefault();
        }

        public void Remove(int userId, OAuthProvider provider)
        {
            var item = GetUserAccount(userId, provider);
            if (item != null)
                DbContext.Entry(item).State = EntityState.Deleted;
        }
    }
}