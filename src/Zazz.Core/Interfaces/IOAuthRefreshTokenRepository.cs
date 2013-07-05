using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface IOAuthRefreshTokenRepository : IRepository<OAuthRefreshToken>
    {
        OAuthRefreshToken Get(int userId, int clientId);
    }
}