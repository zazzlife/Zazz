using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Repositories;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;

namespace Zazz.Data.Repositories
{
    public class LinkedAccountRepository : BaseRepository<LinkedAccount>, ILinkedAccountRepository
    {
        public LinkedAccountRepository(DbContext dbContext) : base(dbContext)
        {
        }

        public LinkedAccount GetUserAccount(int userId, OAuthProvider provider)
        {
            return DbSet.Where(o => o.UserId == userId)
                        .Where(o => o.Provider == provider)
                        .SingleOrDefault();
        }

        public IEnumerable<LinkedAccount> GetUserAccounts(int userId)
        {
            return DbSet.Where(o => o.UserId == userId).AsEnumerable();
        }

        public LinkedAccount GetOAuthAccountByProviderId(long providerUserId, OAuthProvider provider)
        {
            return DbSet.Where(o => o.Provider == provider)
                        .Where(o => o.ProviderUserId == providerUserId)
                        .SingleOrDefault();
        }

        public bool Exists(long providerUserId, OAuthProvider provider)
        {
            return DbSet.Where(o => o.ProviderUserId == providerUserId)
                        .Where(o => o.Provider == provider)
                        .Any();
        }

        public bool Exists(int userId, OAuthProvider provider)
        {
            return DbSet.Where(o => o.UserId == userId)
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

        public IQueryable<User> GetUsersByProviderId(IEnumerable<long> providerIds, OAuthProvider provider)
        {
            return DbSet.Where(u => providerIds.Contains(u.ProviderUserId))
                        .Where(u => u.Provider == provider)
                        .Select(u => u.User);
        }
    }
}