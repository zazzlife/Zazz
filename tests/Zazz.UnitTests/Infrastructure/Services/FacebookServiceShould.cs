using System;
using System.Threading.Tasks;
using Facebook;
using Moq;
using NUnit.Framework;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Facebook;
using Zazz.Infrastructure.Services;

namespace Zazz.UnitTests.Infrastructure.Services
{
    [TestFixture]
    public class FacebookServiceShould
    {
        private Mock<IFacebookHelper> _fbHelper;
        private FacebookService _sut;
        private Mock<IErrorHandler> _errorHander;

        [SetUp]
        public void Init()
        {
            _fbHelper = new Mock<IFacebookHelper>();
            _errorHander = new Mock<IErrorHandler>();
            _sut = new FacebookService(_fbHelper.Object, _errorHander.Object);

            _fbHelper.Setup(x => x.SetAccessToken(It.IsAny<string>()));
            _errorHander.Setup(x => x.LogException(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>()));
        }

        [Test]
        public async Task CallRightPathAndSetAccessToken_OnGetUser()
        {
            //Arrange
            var id = "Soroush.Mirzaei";
            var token = "token";
            var user = new FbUser();
            _fbHelper.Setup(x => x.GetAsync<FbUser>(id, "email"))
                     .Returns(() => Task.Run(() => user));

            //Act
            var result = await _sut.GetUserAsync(id, token);

            //Assert
            _fbHelper.Verify(x => x.SetAccessToken(token), Times.Once());
            _fbHelper.Verify(x => x.GetAsync<FbUser>(id, "email"), Times.Once());
            Assert.AreSame(user, result);
        }

        [Test]
        public async Task CallErrorHandlerTokenExpiredWhenOAuthExceptionOccures_OnGetUser()
        {
            //Arrange
            var id = "Soroush.Mirzaei";
            var token = "token";
            var user = new FbUser();
            _fbHelper.Setup(x => x.GetAsync<FbUser>(id, "email"))
                     .Throws<FacebookOAuthException>();
            _errorHander.Setup(x => x.HandleAccessTokenExpiredAsync(id, OAuthProvider.Facebook))
                        .Returns(() => Task.Run(() => { }));
            //Act
            try
            {
                var t = await _sut.GetUserAsync(id, token);
                Assert.Fail("Expected Exception Wasn't Thrown");
            }
            catch (FacebookOAuthException)
            {
            }

            //Assert
            _errorHander.Verify(x => x.HandleAccessTokenExpiredAsync(id, OAuthProvider.Facebook), Times.Once());
        }

        [Test]
        public async Task CallErrorHandlerApiLimitReachedWhenApiLimitExceptionOccures_OnGetUser()
        {
            //Arrange
            var id = "Soroush.Mirzaei";
            var token = "token";
            var user = new FbUser();
            _fbHelper.Setup(x => x.GetAsync<FbUser>(id, "email"))
                     .Throws<FacebookApiLimitException>();
            _errorHander.Setup(x => x.HandleFacebookApiLimitReachedAsync(id, It.IsAny<string>(), It.IsAny<string>()))
                        .Returns(() => Task.Run(() => { }));
            //Act
            try
            {
                var t = await _sut.GetUserAsync(id, token);
                Assert.Fail("Expected Exception Wasn't Thrown");
            }
            catch (FacebookApiLimitException)
            {
            }

            //Assert
            _errorHander.Verify(x => x.HandleFacebookApiLimitReachedAsync(id, It.IsAny<string>(), It.IsAny<string>()),
                                Times.Once());
        }

        [Test]
        public async Task CallErrorHandlerLogErrorWhenUknownExceptionOccures_OnGetUser()
        {
            //Arrange
            var id = "Soroush.Mirzaei";
            var token = "token";
            var user = new FbUser();
            _fbHelper.Setup(x => x.GetAsync<FbUser>(id, "email"))
                     .Throws<Exception>();
            _errorHander.Setup(x => x.LogException(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>()));
            
                //Act
            try
            {
                var t = await _sut.GetUserAsync(id, token);
                Assert.Fail("Expected Exception Wasn't Thrown");
            }
            catch (Exception)
            {
            }

            //Assert
            _errorHander.Verify(x => x.LogException(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>()),
                                Times.Once());
        }
    }
}