namespace Zazz.Core.Interfaces
{
    public interface ILogger
    {
        void LogDebug(string nameSpace, string message);
        void LogFatal(string nameSpace, string message);
        void LogError(string nameSpace, string message);
        void LogWarning(string nameSpace, string message);
        void LogInfo(string nameSpace, string message);
    }
}