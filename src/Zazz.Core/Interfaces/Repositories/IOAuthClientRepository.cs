using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces.Repositories
{
    public interface IOAuthClientRepository
    {
        OAuthClient GetById(string clientId);
    }
}