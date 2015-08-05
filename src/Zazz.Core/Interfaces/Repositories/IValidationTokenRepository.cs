using System.Linq;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces.Repositories
{
    public interface IValidationTokenRepository : IRepository<UserValidationToken>
    {
        IQueryable<UserValidationToken> GetValidationTokens(int userId);
    }
}