using System.Collections.Generic;
using System.Threading.Tasks;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface IOAuthAccountRepository
    {
        Task<OAuthAccount> GetUserAccount(int userId, OAuthProvider provider);

        Task<IEnumerable<OAuthAccount>> GetUserAccounts(int userId);

        void Remove(int userId, OAuthProvider provider);
    }
}