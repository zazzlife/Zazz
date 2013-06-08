using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface IAppRequestTokenService
    {
        AppRequestToken Create(int appId);

        AppRequestToken Get(long requestId);

        void Remove(long requestId);
    }
}