using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;

namespace Zazz.Core.Interfaces
{
    public interface IAppRequestTokenService
    {
        AppRequestToken Create(int appId, AppTokenType tokenType);

        AppRequestToken Get(long requestId);

        void Remove(long requestId);
        void Remove(AppRequestToken request);
    }
}