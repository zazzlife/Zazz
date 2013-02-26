using System.Collections.Generic;
using System.Threading.Tasks;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface IOAuthAccountRepository : IRepository<OAuthAccount>
    {
        Task<OAuthAccount> GetUserAccountAsync(int userId, OAuthProvider provider);

        Task<IEnumerable<OAuthAccount>> GetUserAccountsAsync(int userId);

        Task RemoveAsync(int userId, OAuthProvider provider);
    }
}