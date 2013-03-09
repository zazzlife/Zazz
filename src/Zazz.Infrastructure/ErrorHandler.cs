using System;
using System.Threading.Tasks;
using Zazz.Core.Interfaces;

namespace Zazz.Infrastructure
{
    public class ErrorHandler : IErrorHandler
    {
        private readonly IUoW _uoW;
        private readonly ILogger _logger;
        private readonly IEmailService _emailService;

        public ErrorHandler(IUoW uoW, ILogger logger, IEmailService emailService)
        {
            _uoW = uoW;
            _logger = logger;
            _emailService = emailService;
        }

        public Task HandleAccessTokenExpiredAsync(string fbUserId)
        {
            throw new NotImplementedException();
        }

        public Task HandleFacebookApiLimitAsync(string fbUserId, string path, string fields)
        {
            throw new NotImplementedException();
        }

        public Task HandleFacebookApiErrorAsync(string fbUserId, string methodName, Exception exception)
        {
            throw new NotImplementedException();
        }
    }
}