using System;
using System.Threading.Tasks;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface IErrorHandler : IDisposable
    {
        Task HandleAccessTokenExpiredAsync(string fbUserId, OAuthProvider provider);

        Task HandleFacebookApiLimitReachedAsync(string fbUserId, string path, string fields);

        Task HandleFacebookApiErrorAsync(string fbUserId, string methodName, Exception exception);
    }
}