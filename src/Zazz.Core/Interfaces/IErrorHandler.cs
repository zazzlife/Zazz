using System;
using System.Threading.Tasks;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface IErrorHandler : IDisposable
    {
        Task HandleAccessTokenExpiredAsync(string fbUserId, OAuthProvider provider);

        Task HandleFacebookApiLimitReachedAsync(string fbUserId, string path, string fields);

        void LogException(string message, string nameSpace, Exception exception);
    }
}