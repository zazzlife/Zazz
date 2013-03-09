using System;
using System.Net.Mail;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Infrastructure;

namespace Zazz.UnitTests.Infrastructure
{
    [TestFixture]
    public class ErrorHandlerShould
    {
        private Mock<IUoW> _uow;
        private Mock<ILogger> _logger;
        private Mock<IEmailService> _emailService;
        private ErrorHandler _sut;
        private OAuthAccount _oauthAccount;

        [SetUp]
        public void Init()
        {
            _uow = new Mock<IUoW>();
            _logger = new Mock<ILogger>();
            _emailService = new Mock<IEmailService>();
            _sut = new ErrorHandler(_uow.Object, _logger.Object, _emailService.Object);

            _uow.Setup(x => x.SaveAsync())
                .Returns(() => Task.Run(() => { }));

            _emailService.Setup(x => x.SendAsync(It.IsAny<MailMessage>()))
                         .Returns(() => Task.Run(() => { }));

            _oauthAccount = new OAuthAccount
                                {
                                    User = new User
                                               {
                                                   Email = "test@zazzlife.com",
                                                   UserDetail = new UserDetail()
                                               }
                                };
        }

        [Test]
        public async Task UpdateRecordOnDB_OnAccessTokenExpired()
        {
            //Arrange
            var nameSpace = "Test";
            var fbUserId = 1234L;
            _logger.Setup(x => x.LogError(nameSpace, It.IsAny<string>()));
            _uow.Setup(x => x.OAuthAccountRepository.GetOAuthAccountByProviderIdAsync(fbUserId, OAuthProvider.Facebook))
                .Returns(() => Task.Run(() => _oauthAccount));


            //Act
            await _sut.HandleAccessTokenExpiredAsync(fbUserId.ToString(), OAuthProvider.Facebook);

            //Assert
            Assert.AreEqual(DateTime.UtcNow.Date, _oauthAccount.User.UserDetail.LastSyncError.Value.Date);
            _uow.Verify(x => x.SaveAsync(), Times.Once());
        }

        [Test]
        public async Task SendEmailIfSendNotificationIsTureAndLastEmailWasSentOverAWeekAgo_OnAccessTokenExpired()
        {
            //Arrange
            _oauthAccount.User.UserDetail.SendSyncErrorNotifications = true;
            _oauthAccount.User.UserDetail.LasySyncErrorEmailSent = DateTime.UtcNow.AddDays(-8);
            var nameSpace = "Test";
            var fbUserId = 1234L;
            _logger.Setup(x => x.LogError(nameSpace, It.IsAny<string>()));
            _uow.Setup(x => x.OAuthAccountRepository.GetOAuthAccountByProviderIdAsync(fbUserId, OAuthProvider.Facebook))
                .Returns(() => Task.Run(() => _oauthAccount));
            

            //Act
            await _sut.HandleAccessTokenExpiredAsync(fbUserId.ToString(), OAuthProvider.Facebook);

            //Assert
            Assert.AreEqual(DateTime.UtcNow.Date, _oauthAccount.User.UserDetail.LasySyncErrorEmailSent.Value.Date);
            _emailService.Verify(x => x.SendAsync(It.IsAny<MailMessage>()), Times.Once());
            _uow.Verify(x => x.SaveAsync(), Times.Once());
        }

        [Test]
        public async Task NotSendEmailIfSendNotificationIsFalseAndLastEmailWasSentOverAWeekAgo_OnAccessTokenExpired()
        {
            //Arrange
            _oauthAccount.User.UserDetail.SendSyncErrorNotifications = false;
            _oauthAccount.User.UserDetail.LasySyncErrorEmailSent = DateTime.UtcNow.AddDays(-8);
            var nameSpace = "Test";
            var fbUserId = 1234L;
            _logger.Setup(x => x.LogError(nameSpace, It.IsAny<string>()));
            _uow.Setup(x => x.OAuthAccountRepository.GetOAuthAccountByProviderIdAsync(fbUserId, OAuthProvider.Facebook))
                .Returns(() => Task.Run(() => _oauthAccount));


            //Act
            await _sut.HandleAccessTokenExpiredAsync(fbUserId.ToString(), OAuthProvider.Facebook);

            //Assert
            Assert.AreNotEqual(DateTime.UtcNow.Date, _oauthAccount.User.UserDetail.LasySyncErrorEmailSent.Value.Date);
            _emailService.Verify(x => x.SendAsync(It.IsAny<MailMessage>()), Times.Never());
            _uow.Verify(x => x.SaveAsync(), Times.Once());
        }

        [Test]
        public async Task NotSendEmailIfSendNotificationIsTrueAndLastEmailWasNotSentOverAWeekAgo_OnAccessTokenExpired([Values(0, -1, -2, -3, -4, -5, -6)] int daysAgo)
        {
            //Arrange
            _oauthAccount.User.UserDetail.SendSyncErrorNotifications = false;
            _oauthAccount.User.UserDetail.LasySyncErrorEmailSent = DateTime.UtcNow.AddDays(daysAgo);
            var nameSpace = "Test";
            var fbUserId = 1234L;
            _logger.Setup(x => x.LogError(nameSpace, It.IsAny<string>()));
            _uow.Setup(x => x.OAuthAccountRepository.GetOAuthAccountByProviderIdAsync(fbUserId, OAuthProvider.Facebook))
                .Returns(() => Task.Run(() => _oauthAccount));


            //Act
            await _sut.HandleAccessTokenExpiredAsync(fbUserId.ToString(), OAuthProvider.Facebook);

            //Assert
            _emailService.Verify(x => x.SendAsync(It.IsAny<MailMessage>()), Times.Never());
            _uow.Verify(x => x.SaveAsync(), Times.Once());
        }

        [Test]
        public async Task LogTheError_OnAccessTokenExpired()
        {
            //Arrange
            var fbUserId = 1234L;
            _logger.Setup(x => x.LogError(It.IsAny<string>(), It.IsAny<string>()));
            _uow.Setup(x => x.OAuthAccountRepository.GetOAuthAccountByProviderIdAsync(fbUserId, OAuthProvider.Facebook))
                .Returns(() => Task.Run(() => _oauthAccount));

            //Act
            await _sut.HandleAccessTokenExpiredAsync(fbUserId.ToString(), OAuthProvider.Facebook);

            //Assert
            _logger.Verify(x => x.LogError(It.IsAny<string>(), It.IsAny<string>()), Times.Once());
        }


    }
}