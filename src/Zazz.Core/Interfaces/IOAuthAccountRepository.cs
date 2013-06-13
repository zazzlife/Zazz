using System.Collections.Generic;
using System.Threading.Tasks;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;

namespace Zazz.Core.Interfaces
{
    public interface IOAuthAccountRepository : IRepository<OAuthAccount>
    {
        /// <summary>
        /// Gets the user's OAuth Account
        /// </summary>
        /// <param name="userId">The User id on our site</param>
        /// <param name="provider"></param>
        /// <returns></returns>
        OAuthAccount GetUserAccount(int userId, OAuthProvider provider);

        /// <summary>
        /// Gets all of user OAuth Accounts
        /// </summary>
        /// <param name="userId">The User id on our site</param>
        /// <returns></returns>
        IEnumerable<OAuthAccount> GetUserAccounts(int userId);

        /// <summary>
        /// Gets the OAuth Account by user's OAuth ID
        /// </summary>
        /// <param name="providerUserId">OAuth Provider's User ID</param>
        /// <param name="provider"></param>
        /// <returns></returns>
        OAuthAccount GetOAuthAccountByProviderId(long providerUserId, OAuthProvider provider);

        /// <summary>
        /// Checks if the third party account exists
        /// </summary>
        /// <param name="providerUserId">OAuth Provider's User ID</param>
        /// <param name="provider"></param>
        /// <returns></returns>
        bool Exists(long providerUserId, OAuthProvider provider);

        bool Exists(int userId, OAuthProvider provider);

        string GetAccessToken(int userId, OAuthProvider provider);

        void Remove(int userId, OAuthProvider provider);
    }
}