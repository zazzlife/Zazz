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
        private readonly IUoW _uoW;
        private readonly ILogger _logger;
        private readonly IEmailService _emailService;

        public ErrorHandler(IUoW uoW, ILogger logger, IEmailService emailService)
        {
            _uoW = uoW;
            _logger = logger;
            _emailService = emailService;
        }

        public async Task HandleAccessTokenExpiredAsync(string fbUserId, OAuthProvider provider)
        {
            var errorLog = "Expired access token: " + fbUserId;
            _logger.LogError("ErrorHandler", errorLog);

            var oauthAccount = await _uoW.OAuthAccountRepository.GetOAuthAccountByProviderIdAsync(long.Parse(fbUserId),
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
            await _uoW.SaveAsync();
        }

        public Task HandleFacebookApiLimitAsync(string fbUserId, string path, string fields)
        {
            throw new NotImplementedException();
        }

        public Task HandleFacebookApiErrorAsync(string fbUserId, string methodName, Exception exception)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _uoW.Dispose();
        }
    }
}