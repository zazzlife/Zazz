using System;
using System.Threading.Tasks;

namespace Zazz.Core.Interfaces
{
    public interface IErrorHandler
    {
        Task HandleAccessTokenExpiredAsync(string fbUserId);

        Task HandleFacebookApiLimitAsync(string fbUserId, string path, string fields);

        Task HandleFacebookApiErrorAsync(string fbUserId, string methodName, Exception exception);
    }
}