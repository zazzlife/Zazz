using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface IOAuthClientRepository
    {
        OAuthClient GetById(string clientId);
    }
}