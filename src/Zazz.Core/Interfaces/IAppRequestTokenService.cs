using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface IAppRequestTokenService
    {
        AppRequestToken Create(int appId);

        AppRequestToken Get(int requestId);

        void Remove(int requestId);
    }
}