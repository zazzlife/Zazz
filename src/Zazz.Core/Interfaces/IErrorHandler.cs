using System;
using System.Threading.Tasks;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface IErrorHandler : IDisposable
    {
        void HandleAccessTokenExpired(string fbUserId, OAuthProvider provider);

        void HandleFacebookApiLimitReached(string fbUserId, string path, string fields);

        void LogException(string message, string nameSpace, Exception exception);
    }
}