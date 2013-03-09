using System;
using Zazz.Core.Interfaces;
using log4net;

namespace Zazz.Infrastructure
{
    public class Logger : ILogger
    {
        public void LogFatal(string nameSpace, string message, Exception exception)
        {
            var logger = LogManager.GetLogger(nameSpace);
            if (logger.IsFatalEnabled)
                logger.Fatal(message, exception);
        }

        public void LogError(string nameSpace, string message)
        {
            var logger = LogManager.GetLogger(nameSpace);
            if (logger.IsErrorEnabled)
                logger.Error(message);
        }

        public void LogWarning(string nameSpace, string message)
        {
            var logger = LogManager.GetLogger(nameSpace);
            if (logger.IsWarnEnabled)
                logger.Warn(message);
        }

        public void LogInfo(string nameSpace, string message)
        {
            var logger = LogManager.GetLogger(nameSpace);
            if (logger.IsInfoEnabled)
                logger.Info(message);
        }

        public void LogDebug(string nameSpace, string message)
        {
            var logger = LogManager.GetLogger(nameSpace);
            if (logger.IsDebugEnabled)
                logger.Debug(message);
        }
    }
}