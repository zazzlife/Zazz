using System;
using System.Net.Mail;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Services;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;
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
        private LinkedAccount _oauthAccount;

        [SetUp]
        public void Init()
        {
            _uow = new Mock<IUoW>();
            _logger = new Mock<ILogger>();
            _emailService = new Mock<IEmailService>();
            _sut = new ErrorHandler(_uow.Object, _logger.Object, _emailService.Object);

            _uow.Setup(x => x.SaveChanges());

            _emailService.Setup(x => x.Send(It.IsAny<MailMessage>()));

            _oauthAccount = new LinkedAccount
                                {
                                    User = new User
                                               {
                                                   Email = "test@zazzlife.com",
                                                   UserDetail = new UserDetail()
                                               }
                                };
        }

        //[Test]
        //public void UpdateRecordOnDB_OnAccessTokenExpired()
        //{
        //    //Arrange
        //    var nameSpace = "Test";
        //    var fbUserId = 1234L;
        //    _logger.Setup(x => x.LogError(nameSpace, It.IsAny<string>()));
        //    _uow.Setup(x => x.OAuthAccountRepository.GetOAuthAccountByProviderId(fbUserId, OAuthProvider.Facebook))
        //        .Returns(_oauthAccount);


        //    //Act
        //    _sut.HandleAccessTokenExpired(fbUserId.ToString(), OAuthProvider.Facebook);

        //    //Assert
        //    Assert.AreEqual(DateTime.UtcNow.Date, _oauthAccount.User.UserDetail.LastSyncError.Value.Date);
        //    _uow.Verify(x => x.SaveChanges(), Times.Once());
        //}

        //[Test]
        //public void SendEmailIfSendNotificationIsTureAndLastEmailWasSentOverAWeekAgo_OnAccessTokenExpired()
        //{
        //    //Arrange
        //    _oauthAccount.User.UserDetail.SendSyncErrorNotifications = true;
        //    _oauthAccount.User.UserDetail.LasySyncErrorEmailSent = DateTime.UtcNow.AddDays(-8);
        //    var nameSpace = "Test";
        //    var fbUserId = 1234L;
        //    _logger.Setup(x => x.LogError(nameSpace, It.IsAny<string>()));
        //    _uow.Setup(x => x.OAuthAccountRepository.GetOAuthAccountByProviderId(fbUserId, OAuthProvider.Facebook))
        //        .Returns(_oauthAccount);
            

        //    //Act
        //    _sut.HandleAccessTokenExpired(fbUserId.ToString(), OAuthProvider.Facebook);

        //    //Assert
        //    Assert.AreEqual(DateTime.UtcNow.Date, _oauthAccount.User.UserDetail.LasySyncErrorEmailSent.Value.Date);
        //    _emailService.Verify(x => x.Send(It.IsAny<MailMessage>()), Times.Once());
        //    _uow.Verify(x => x.SaveChanges(), Times.Once());
        //}

        //[Test]
        //public void NotSendEmailIfSendNotificationIsFalseAndLastEmailWasSentOverAWeekAgo_OnAccessTokenExpired()
        //{
        //    //Arrange
        //    _oauthAccount.User.UserDetail.SendSyncErrorNotifications = false;
        //    _oauthAccount.User.UserDetail.LasySyncErrorEmailSent = DateTime.UtcNow.AddDays(-8);
        //    var nameSpace = "Test";
        //    var fbUserId = 1234L;
        //    _logger.Setup(x => x.LogError(nameSpace, It.IsAny<string>()));
        //    _uow.Setup(x => x.OAuthAccountRepository.GetOAuthAccountByProviderId(fbUserId, OAuthProvider.Facebook))
        //        .Returns(_oauthAccount);


        //    //Act
        //    _sut.HandleAccessTokenExpired(fbUserId.ToString(), OAuthProvider.Facebook);

        //    //Assert
        //    Assert.AreEqual(DateTime.UtcNow.Date, _oauthAccount.User.UserDetail.LastSyncError.Value.Date);
        //    Assert.AreNotEqual(DateTime.UtcNow.Date, _oauthAccount.User.UserDetail.LasySyncErrorEmailSent.Value.Date);
        //    _emailService.Verify(x => x.Send(It.IsAny<MailMessage>()), Times.Never());
        //    _uow.Verify(x => x.SaveChanges(), Times.Once());
        //}

        //[Test]
        //public void NotSendEmailIfSendNotificationIsTrueAndLastEmailWasNotSentOverAWeekAgo_OnAccessTokenExpired([Values(0, -1, -2, -3, -4, -5, -6)] int daysAgo)
        //{
        //    //Arrange
        //    _oauthAccount.User.UserDetail.SendSyncErrorNotifications = false;
        //    _oauthAccount.User.UserDetail.LasySyncErrorEmailSent = DateTime.UtcNow.AddDays(daysAgo);
        //    var nameSpace = "Test";
        //    var fbUserId = 1234L;
        //    _logger.Setup(x => x.LogError(nameSpace, It.IsAny<string>()));
        //    _uow.Setup(x => x.OAuthAccountRepository.GetOAuthAccountByProviderId(fbUserId, OAuthProvider.Facebook))
        //        .Returns(_oauthAccount);


        //    //Act
        //    _sut.HandleAccessTokenExpired(fbUserId.ToString(), OAuthProvider.Facebook);

        //    //Assert
        //    Assert.AreEqual(DateTime.UtcNow.Date, _oauthAccount.User.UserDetail.LastSyncError.Value.Date);
        //    _emailService.Verify(x => x.Send(It.IsAny<MailMessage>()), Times.Never());
        //    _uow.Verify(x => x.SaveChanges(), Times.Once());
        //}

        //[Test]
        //public void LogTheError_OnAccessTokenExpired()
        //{
        //    //Arrange
        //    var fbUserId = 1234L;
        //    _logger.Setup(x => x.LogError(It.IsAny<string>(), It.IsAny<string>()));
        //    _uow.Setup(x => x.OAuthAccountRepository.GetOAuthAccountByProviderId(fbUserId, OAuthProvider.Facebook))
        //        .Returns(_oauthAccount);

        //    //Act
        //    _sut.HandleAccessTokenExpired(fbUserId.ToString(), OAuthProvider.Facebook);

        //    //Assert
        //    _logger.Verify(x => x.LogError(It.IsAny<string>(), It.IsAny<string>()), Times.Once());
        //}

        //[Test]
        //public void LogAndAddRecordForRetry_OnHandleApiLimit()
        //{
        //    //Arrange
        //    _uow.Setup(x => x.FacebookSyncRetryRepository.InsertGraph(It.IsAny<FacebookSyncRetry>()));
        //    _logger.Setup(x => x.LogError(It.IsAny<string>(), It.IsAny<string>()));


        //    //Act
        //    _sut.HandleFacebookApiLimitReached("1234", "", "");

        //    //Assert
        //    _logger.Verify(x => x.LogError(It.IsAny<string>(), It.IsAny<string>()), Times.Once());
        //    _uow.Verify(x => x.FacebookSyncRetryRepository.InsertGraph(It.IsAny<FacebookSyncRetry>()), Times.Once());
        //    _uow.Verify(x => x.SaveChanges(), Times.Once());
        //}

        //[Test]
        //public void LogTheFbError_OnLogError()
        //{
        //    //Arrange
        //    _logger.Setup(x => x.LogFatal(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>()));

        //    //Act
        //    _sut.LogException("1234", "", new Exception());

        //    //Assert
        //    _logger.Verify(x => x.LogFatal(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Once());
        //}


    }
}