using System;

namespace Zazz.Core.Interfaces
{
    public interface ILogger
    {
        void LogFatal(string nameSpace, string message, Exception exception);
        void LogError(string nameSpace, string message);
        void LogWarning(string nameSpace, string message);
        void LogInfo(string nameSpace, string message);
        void LogDebug(string nameSpace, string message);

    }
}