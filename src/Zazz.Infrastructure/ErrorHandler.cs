using System;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Infrastructure
{
    public class ErrorHandler : IErrorHandler
    {
        private readonly IUoW _uow;
        private readonly ILogger _logger;
        private readonly IEmailService _emailService;

        public ErrorHandler(IUoW uow, ILogger logger, IEmailService emailService)
        {
            _uow = uow;
            _logger = logger;
            _emailService = emailService;
        }

        public async Task HandleAccessTokenExpiredAsync(string fbUserId, OAuthProvider provider)
        {
            var errorLog = "Expired access token: " + fbUserId;
            _logger.LogError("ErrorHandler", errorLog);

            var oauthAccount = _uow.OAuthAccountRepository.GetOAuthAccountByProviderId(long.Parse(fbUserId),
                                                                                            provider);

            if (oauthAccount.User.UserDetail.SendSyncErrorNotifications &&
                oauthAccount.User.UserDetail.LasySyncErrorEmailSent < DateTime.UtcNow.AddDays(-7))
            {
                bool isHtml;
                string subject;
                var message = EmailMessages.AccessTokenExpiredMessage(out isHtml, out subject);
                var emailMessage = new MailMessage(EmailMessages.NOREPLY_ADDRESS, oauthAccount.User.Email)
                                       {
                                           Body = message,
                                           IsBodyHtml = isHtml,
                                           Subject = subject,
                                           BodyEncoding = Encoding.UTF8
                                       };

                await _emailService.SendAsync(emailMessage);
                oauthAccount.User.UserDetail.LasySyncErrorEmailSent = DateTime.UtcNow;
            }

            oauthAccount.User.UserDetail.LastSyncError = DateTime.UtcNow;
            _uow.SaveChanges();
        }

        public async Task HandleFacebookApiLimitReachedAsync(string fbUserId, string path, string fields)
        {
            var logMessage = String.Format("Facebook API limit reached: [FB user id: {0}] [Path: {1}] [Fields: {2}]",
                                           fbUserId, path, fields);
            _logger.LogError("ErrorHandler", logMessage);

            var fbSyncRetry = new FacebookSyncRetry
                                  {
                                      FacebookUserId = long.Parse(fbUserId),
                                      Fields = fields,
                                      Path = path,
                                      LastTry = DateTime.UtcNow
                                  };
            _uow.FacebookSyncRetryRepository.InsertGraph(fbSyncRetry);
            _uow.SaveChanges();
        }

        public void LogException(string message, string nameSpace, Exception exception)
        {
            _logger.LogFatal(nameSpace, message, exception);
        }

        public void Dispose()
        {
            _uow.Dispose();
        }
    }
}