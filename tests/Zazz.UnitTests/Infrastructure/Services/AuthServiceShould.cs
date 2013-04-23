using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Zazz.Core.Exceptions;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;
using Zazz.Infrastructure.Services;

namespace Zazz.UnitTests.Infrastructure.Services
{
    [TestFixture]
    public class AuthServiceShould
    {
        private Mock<IUoW> _uowMock;
        private Mock<ICryptoService> _cryptoMock;
        private AuthService _sut;

        [SetUp]
        public void Init()
        {
            _uowMock = new Mock<IUoW>();
            _cryptoMock = new Mock<ICryptoService>();
            _sut = new AuthService(_uowMock.Object, _cryptoMock.Object);

            _uowMock.Setup(x => x.SaveChanges());
        }

        #region Login
        [Test]
        public void GetHashOfPassword_OnLogin()
        {
            //Arrange
            var pass = "password";
            _uowMock.Setup(x => x.UserRepository.GetByUsername(It.IsAny<string>()))
                    .Returns(() => new User { Password = pass });
            _cryptoMock.Setup(x => x.GeneratePasswordHash(It.IsAny<string>()))
                       .Returns(pass);


            //Act
            _sut.Login("user", pass);

            //Assert
            _cryptoMock.Verify(x => x.GeneratePasswordHash(pass), Times.Once());
        }

        [Test]
        public void GetUserPasswordFromDB_OnLogin()
        {
            //Arrange
            var username = "username";
            var pass = "pass";
            _uowMock.Setup(x => x.UserRepository.GetByUsername(username))
                    .Returns(() => new User { Password = pass });
            _cryptoMock.Setup(x => x.GeneratePasswordHash(It.IsAny<string>()))
                       .Returns(pass);
            //Act
            _sut.Login(username, pass);

            //Assert
            _uowMock.Verify(x => x.UserRepository.GetByUsername(username), Times.Once());
        }

        [Test]
        public void ThrowUserNotExist_WhenUserNotExists_OnLogin()
        {
            //Arrange
            _uowMock.Setup(x => x.UserRepository.GetByUsername(It.IsAny<string>()))
                    .Returns(() => null);

            //Act
            try
            {
                _sut.Login("user", "pass");
                Assert.Fail("Expected exception wasn't thrown");
            }
            catch (UserNotExistsException e)
            {
                //Assert
                Assert.IsInstanceOf<UserNotExistsException>(e);
            }
        }

        [Test]
        public void ThrowInvalidPassword_WhenPasswordsDontMatch_OnLogin()
        {
            //Arrange
            _uowMock.Setup(x => x.UserRepository.GetByUsername(It.IsAny<string>()))
                    .Returns(() => new User { Password = "password" });
            _cryptoMock.Setup(x => x.GeneratePasswordHash(It.IsAny<string>()))
                       .Returns("invalidPass");
            //Act
            try
            {
                _sut.Login("user", "invalidPassword");
                Assert.Fail("Expected exception wasn't thrown");
            }
            catch (InvalidPasswordException e)
            {
                // Assert
                Assert.IsInstanceOf<InvalidPasswordException>(e);
            }
        }

        [Test]
        public void UpdateLastActivity_WhenEverythingIsOk_OnLogin()
        {
            //Arrange
            var pass = "pass";
            var user = new User { Password = pass, LastActivity = DateTime.MaxValue };
            _uowMock.Setup(x => x.UserRepository.GetByUsername(It.IsAny<string>()))
                    .Returns(user);
            _cryptoMock.Setup(x => x.GeneratePasswordHash(It.IsAny<string>()))
                       .Returns(pass);

            //Act
            _sut.Login("user", pass);

            //Assert
            Assert.IsTrue(user.LastActivity <= DateTime.UtcNow);
        }

        [Test]
        public void CallSaveChanges_WhenEverythingIsOk_OnLogin()
        {
            //Arrange
            var pass = "pass";
            var user = new User { Password = pass, LastActivity = DateTime.MaxValue };
            _uowMock.Setup(x => x.UserRepository.GetByUsername(It.IsAny<string>()))
                    .Returns(user);

            _cryptoMock.Setup(x => x.GeneratePasswordHash(It.IsAny<string>()))
                       .Returns(pass);

            //Act
            _sut.Login("user", pass);

            //Assert
            _uowMock.Verify(x => x.SaveChanges(), Times.Once());
        }
        #endregion

        #region Register

        [Test]
        public void ThrowWhenUserDetailIsNull_OnRegister()
        {
            //Arrange
            var user = new User();

            //Act
            try
            {
                _sut.Register(user, false);
                Assert.Fail("Expected exception wasn't thrown");
            }
            catch (ArgumentNullException)
            {
            }

            //Assert
        }

        [Test]
        public void CheckForExistingUser_OnRegister()
        {
            //Arrange
            var user = new User { Email = "email", Username = "username", Password = "pass", UserDetail = new UserDetail() };
            _uowMock.Setup(x => x.UserRepository.ExistsByUsername(user.Username))
                    .Returns(false);
            _uowMock.Setup(x => x.UserRepository.ExistsByEmail(user.Email))
                    .Returns(false);

            //Act
            _sut.Register(user, false);

            //Assert
            _uowMock.Verify(x => x.UserRepository.ExistsByUsername(user.Username), Times.Once());
        }

        [Test]
        public void ThrowIfUsernameExists_OnRegister()
        {
            //Arrange
            var user = new User { Email = "email", Username = "username", Password = "pass", UserDetail = new UserDetail() };
            _uowMock.Setup(x => x.UserRepository.ExistsByUsername(user.Username))
                    .Returns(true);
            _uowMock.Setup(x => x.UserRepository.ExistsByEmail(user.Email))
                    .Returns(false);

            //Act
            try
            {
                _sut.Register(user, false);
                Assert.Fail("Expected exception wasn't thrown");
            }
            catch (UsernameExistsException e)
            {
                //Assert
                Assert.IsInstanceOf<UsernameExistsException>(e);
            }
        }

        [Test]
        public void CheckForExistingEmail_OnRegister()
        {
            //Arrange
            var user = new User { Email = "email", Username = "username", Password = "pass", UserDetail = new UserDetail() };
            _uowMock.Setup(x => x.UserRepository.ExistsByUsername(user.Username))
                    .Returns(false);
            _uowMock.Setup(x => x.UserRepository.ExistsByEmail(user.Email))
                    .Returns(false);

            //Act
            _sut.Register(user, false);

            //Assert
            _uowMock.Verify(x => x.UserRepository.ExistsByEmail(user.Email), Times.Once());

        }

        [Test]
        public void ThrowIfEmailExists_OnRegister()
        {
            //Arrange
            var user = new User { Email = "email", Username = "username", Password = "pass", UserDetail = new UserDetail() };
            _uowMock.Setup(x => x.UserRepository.ExistsByUsername(user.Username))
                    .Returns(false);
            _uowMock.Setup(x => x.UserRepository.ExistsByEmail(user.Email))
                    .Returns(true);

            //Act
            try
            {
                _sut.Register(user, false);
                Assert.Fail("Expected exception wasn't thrown");
            }
            catch (EmailExistsException e)
            {
                //Assert
                Assert.IsInstanceOf<EmailExistsException>(e);
            }
        }

        [Test]
        public void HashThePassword_OnRegister()
        {
            //Arrange
            var clearPass = "pass";
            var hashedPass = "hashedPassword";

            var user = new User { Email = "email", Username = "username", Password = clearPass, UserDetail = new UserDetail() };

            _uowMock.Setup(x => x.UserRepository.ExistsByUsername(user.Username))
                    .Returns(false);
            _uowMock.Setup(x => x.UserRepository.ExistsByEmail(user.Email))
                    .Returns(false);
            _cryptoMock.Setup(x => x.GeneratePasswordHash(user.Password))
                       .Returns(hashedPass);

            //Act
            _sut.Register(user, false);

            //Assert
            _cryptoMock.Verify(x => x.GeneratePasswordHash(clearPass), Times.Once());
            Assert.AreEqual(user.Password, hashedPass);
        }

        [Test]
        public void AssignUTCDateTimeAsRegiterDate_OnRegister()
        {
            //Arrange
            var user = new User
            {
                Email = "email",
                Username = "username",
                Password = "pass",
                UserDetail = new UserDetail
                                 {
                                     JoinedDate = DateTime.MaxValue
                                 }
            };
            _uowMock.Setup(x => x.UserRepository.ExistsByUsername(user.Username))
                    .Returns(false);
            _uowMock.Setup(x => x.UserRepository.ExistsByEmail(user.Email))
                    .Returns(false);
            _cryptoMock.Setup(x => x.GeneratePasswordHash(user.Password))
                       .Returns(user.Password);

            //Act
            _sut.Register(user, false);

            //Assert
            Assert.IsTrue(user.UserDetail.JoinedDate <= DateTime.UtcNow);
        }

        [Test]
        public void GenerateValidationTokenIfRequested_OnRegister()
        {
            //Arrange
            var user = new User
            {
                Email = "email",
                Username = "username",
                Password = "pass",
                UserDetail = new UserDetail
                                 {
                                     JoinedDate = DateTime.MaxValue
                                 }
            };
            _uowMock.Setup(x => x.UserRepository.ExistsByUsername(user.Username))
                    .Returns(false);
            _uowMock.Setup(x => x.UserRepository.ExistsByEmail(user.Email))
                    .Returns(false);
            _cryptoMock.Setup(x => x.GeneratePasswordHash(user.Password))
                       .Returns(user.Password);


            //Act
            _sut.Register(user, true);

            //Assert
            Assert.IsNotNull(user.ValidationToken);
            Assert.AreEqual(DateTime.UtcNow.AddYears(1).Date, user.ValidationToken.ExpirationDate.Date);
            Assert.IsNotNull(user.ValidationToken.Token);
        }



        [Test]
        public void SaveUserWhenEverythingIsOk_OnRegister()
        {
            //Arrange
            var user = new User
            {
                Email = "email",
                Username = "username",
                Password = "pass",
                UserDetail = new UserDetail
                {
                    JoinedDate = DateTime.MaxValue
                }
            };
            _uowMock.Setup(x => x.UserRepository.ExistsByUsername(user.Username))
                    .Returns(false);
            _uowMock.Setup(x => x.UserRepository.ExistsByEmail(user.Email))
                    .Returns(false);
            _cryptoMock.Setup(x => x.GeneratePasswordHash(user.Password))
                       .Returns(user.Password);

            //Act
            _sut.Register(user, false);

            //Assert
            _uowMock.Verify(x => x.UserRepository.InsertGraph(user), Times.Once());
            _uowMock.Verify(x => x.SaveChanges(), Times.Once());
        }
        #endregion

        #region Generate Reset Password Token

        [Test]
        public void GetUserIdByEmail_OnGenerateResetToken()
        {
            //Arrange
            var email = "email";
            _uowMock.Setup(x => x.UserRepository.GetIdByEmail(email))
                    .Returns(12);
            _uowMock.Setup(x => x.ValidationTokenRepository.GetById(It.IsAny<int>()))
                    .Returns(() => null);

            //Act
            _sut.GenerateResetPasswordToken(email);

            //Assert
            _uowMock.Verify(x => x.UserRepository.GetIdByEmail(email), Times.Once());
        }

        [Test]
        public void ThrowEmailNotExists_WhenEmailNotExists_OnGenerateResetToken()
        {
            //Arrange
            var email = "email";
            _uowMock.Setup(x => x.UserRepository.GetIdByEmail(email))
                    .Returns(0);

            try
            {
                //Act
                _sut.GenerateResetPasswordToken(email);
                Assert.Fail("Expected exception wasn't thrown");
            }
            catch (EmailNotExistsException e)
            {
                //Assert
                Assert.IsInstanceOf<EmailNotExistsException>(e);
            }
        }

        [Test]
        public void GenerateValidToken_OnGenerateResetToken()
        {
            //Arrange
            var userId = 12;
            var email = "email";
            _uowMock.Setup(x => x.UserRepository.GetIdByEmail(email))
                    .Returns(userId);
            _uowMock.Setup(x => x.ValidationTokenRepository.GetById(userId))
                    .Returns(() =>  null);

            //Act
            var result = _sut.GenerateResetPasswordToken(email);

            //Assert
            Assert.AreEqual(DateTime.UtcNow.AddDays(1).Date, result.ExpirationDate.Date);
            Assert.IsNotNull(result.Token);
            Assert.AreEqual(userId, result.Id);
        }

        [Test]
        public void DeleteOldTokenRecordIfExists_OnGenerateResetToken()
        {
            //Arrange
            var email = "email";
            var userId = 1;
            var token = new ValidationToken();
            _uowMock.Setup(x => x.UserRepository.GetIdByEmail(email))
                    .Returns(userId);
            _uowMock.Setup(x => x.ValidationTokenRepository.GetById(userId))
                    .Returns(token);
            _uowMock.Setup(x => x.ValidationTokenRepository.Remove(token));

            //Act
            var result = _sut.GenerateResetPasswordToken(email);

            //Assert
            _uowMock.Verify(x => x.ValidationTokenRepository.GetById(userId), Times.Once());
            _uowMock.Verify(x => x.ValidationTokenRepository.Remove(token), Times.Once());

        }

        [Test]
        public void NotCallRemoveWhenOldTokenNotExists_OnGenerateResetToken()
        {
            //Arrange
            var email = "email";
            var userId = 1;
            var token = new ValidationToken();
            _uowMock.Setup(x => x.UserRepository.GetIdByEmail(email))
                    .Returns(userId);
            _uowMock.Setup(x => x.ValidationTokenRepository.GetById(userId))
                    .Returns(() => null);
            _uowMock.Setup(x => x.ValidationTokenRepository.Remove(token));

            //Act
            var result = _sut.GenerateResetPasswordToken(email);

            //Assert
            _uowMock.Verify(x => x.ValidationTokenRepository.GetById(userId), Times.Once());
            _uowMock.Verify(x => x.ValidationTokenRepository.Remove(token), Times.Never());
        }

        [Test]
        public void SaveWhenEverythingIsOk_OnGenerateResetToken()
        {
            //Arrange
            var email = "email";
            var userId = 1;
            var token = new ValidationToken();
            _uowMock.Setup(x => x.UserRepository.GetIdByEmail(email))
                    .Returns(userId);
            _uowMock.Setup(x => x.ValidationTokenRepository.GetById(userId))
                    .Returns(() => null);
            _uowMock.Setup(x => x.ValidationTokenRepository.Remove(token));

            //Act
            var result = _sut.GenerateResetPasswordToken(email);

            //Assert
            _uowMock.Verify(x => x.ValidationTokenRepository.InsertGraph(result));
            _uowMock.Verify(x => x.SaveChanges(), Times.Once());
        }

        #endregion

        #region Is Token Valid

        [Test]
        public void RetrunTrueWhenTokenIsValid_OnIsTokenValid()
        {
            //Arrange
            var token = new ValidationToken { Id = 12, Token = Guid.NewGuid(), ExpirationDate = DateTime.UtcNow.AddDays(1) };
            _uowMock.Setup(x => x.ValidationTokenRepository.GetById(token.Id))
                    .Returns(() => token);

            //Act
            var result = _sut.IsTokenValid(token.Id, token.Token);

            //Assert
            Assert.IsTrue(result);
            _uowMock.Verify(x => x.ValidationTokenRepository.GetById(token.Id), Times.Once());
        }

        [Test]
        public void RetrunFalseWhenTokenIsNotValid_OnIsTokenValid()
        {
            //Arrange
            var token = new ValidationToken { Id = 12, Token = Guid.NewGuid(), ExpirationDate = DateTime.UtcNow.AddDays(1) };
            _uowMock.Setup(x => x.ValidationTokenRepository.GetById(token.Id))
                    .Returns(() => token);

            //Act
            var result = _sut.IsTokenValid(token.Id, Guid.NewGuid());

            //Assert
            Assert.IsFalse(result);
            _uowMock.Verify(x => x.ValidationTokenRepository.GetById(token.Id), Times.Once());

        }

        [Test]
        public void ThrowExpiredExceptionWhenTokenIsExpired_OnIsTokenValid()
        {
            //Arrange
            var token = new ValidationToken { Id = 12, Token = Guid.NewGuid(), ExpirationDate = DateTime.UtcNow.AddDays(-1) };
            _uowMock.Setup(x => x.ValidationTokenRepository.GetById(token.Id))
                    .Returns(() => token);

            //Act
            try
            {
                var result = _sut.IsTokenValid(token.Id, token.Token);
                Assert.Fail("Expected Exception wasn't thrown");
            }
            catch (TokenExpiredException e)
            {
                //Assert
                _uowMock.Verify(x => x.ValidationTokenRepository.GetById(token.Id), Times.Once());
                Assert.IsInstanceOf<TokenExpiredException>(e);
            }
        }

        #endregion

        #region Reset Password

        [Test]
        public void ThrowInvalidTokenExceptionWhenTokenIsNotValid_OnResetPassword()
        {
            //Arrange
            var token = new ValidationToken { Id = 12, Token = Guid.NewGuid(), ExpirationDate = DateTime.UtcNow.AddDays(1) };
            _uowMock.Setup(x => x.ValidationTokenRepository.GetById(token.Id));

            //Act
            try
            {
                _sut.ResetPassword(token.Id, Guid.NewGuid(), "");
                Assert.Fail("Expected Exception wasn't thrown");
            }
            catch (InvalidTokenException e)
            {
                //Assert
                _uowMock.Verify(x => x.ValidationTokenRepository.GetById(token.Id), Times.Once());
                Assert.IsInstanceOf<InvalidTokenException>(e);
            }
        }

        [Test]
        public void HashTheNewPassword_OnResetPassword()
        {
            //Arrange
            var newPass = "newpass";
            var newPassHash = "hash";
            var user = new User { Password = "pass" };
            var token = new ValidationToken { Id = 12, Token = Guid.NewGuid(), ExpirationDate = DateTime.UtcNow.AddDays(1) };
            _cryptoMock.Setup(x => x.GeneratePasswordHash(newPass))
                       .Returns(() => newPassHash);
            _uowMock.Setup(x => x.ValidationTokenRepository.GetById(token.Id))
                .Returns(token);
            _uowMock.Setup(x => x.UserRepository.GetById(token.Id))
                    .Returns(user);
            _uowMock.Setup(x => x.ValidationTokenRepository.Remove(user.Id));

            //Act
            _sut.ResetPassword(token.Id, token.Token, newPass);

            //Assert
            _cryptoMock.Verify(x => x.GeneratePasswordHash(newPass));
        }

        [Test]
        public void UpdateTheUserCorrectly_OnResetPassword()
        {
            //Arrange
            var newPass = "newpass";
            var newPassHash = "hash";
            var user = new User { Password = "pass" };
            var token = new ValidationToken { Id = 12, Token = Guid.NewGuid(), ExpirationDate = DateTime.UtcNow.AddDays(1) };

            _cryptoMock.Setup(x => x.GeneratePasswordHash(newPass))
                       .Returns(() => newPassHash);
            _uowMock.Setup(x => x.ValidationTokenRepository.GetById(token.Id))
                    .Returns(token);

            _uowMock.Setup(x => x.UserRepository.GetById(token.Id))
                    .Returns(user);

            _uowMock.Setup(x => x.ValidationTokenRepository.Remove(user.Id));
            //Act
            _sut.ResetPassword(token.Id, token.Token, newPass);

            //Assert
            Assert.AreEqual(newPassHash, user.Password);
            _uowMock.Verify(x => x.SaveChanges(), Times.Once());
        }

        [Test]
        public void RemoveTokenOnSuccessfulReset_OnResetPassword()
        {
            //Arrange
            var newPass = "newpass";
            var newPassHash = "hash";
            var user = new User { Password = "pass" };
            var token = new ValidationToken { Id = 12, Token = Guid.NewGuid(), ExpirationDate = DateTime.UtcNow.AddDays(1) };

            _cryptoMock.Setup(x => x.GeneratePasswordHash(newPass))
                       .Returns(() => newPassHash);
            _uowMock.Setup(x => x.ValidationTokenRepository.GetById(token.Id))
                    .Returns(token);

            _uowMock.Setup(x => x.UserRepository.GetById(token.Id))
                    .Returns(user);

            _uowMock.Setup(x => x.ValidationTokenRepository.Remove(user.Id));

            //Act
            _sut.ResetPassword(token.Id, token.Token, newPass);

            //Assert
            Assert.AreEqual(newPassHash, user.Password);
            _uowMock.Verify(x => x.SaveChanges(), Times.Once());
            _uowMock.Verify(x => x.ValidationTokenRepository.Remove(user.Id));
        }

        #endregion

        #region Change Password

        [Test]
        public void HashTheCurrentPasswordBeforeComparing_OnChangePassword()
        {
            //Arrange
            var userId = 12;
            var newPassword = "newPass";
            var newPassHash = "hashpass";
            var oldPassHash = "oldHash";
            var oldPass = "oldPass";
            var user = new User { Password = oldPassHash };

            _uowMock.Setup(x => x.UserRepository.GetById(userId))
                    .Returns(user);
            _cryptoMock.Setup(x => x.GeneratePasswordHash(newPassword))
                       .Returns(newPassHash);
            _cryptoMock.Setup(x => x.GeneratePasswordHash(oldPass))
                       .Returns(oldPassHash);

            //Act
            _sut.ChangePassword(userId, oldPass, newPassword);

            //Assert
            _cryptoMock.Verify(x => x.GeneratePasswordHash(oldPass), Times.Once());
        }

        [Test]
        public void ThrowInvalidPasswordIfCurrentPasswordIsNotCorrect_OnChangePassword()
        {
            //Arrange
            var userId = 12;
            var newPassword = "newPass";
            var newPassHash = "hashpass";
            var oldPassHash = "oldHash";
            var oldPass = "oldPass";
            var invalidPass = "invalid pass";
            var invalidPassHash = "invalid pass hash";

            var user = new User { Password = oldPassHash };

            _uowMock.Setup(x => x.UserRepository.GetById(userId))
                    .Returns(user);
            _cryptoMock.Setup(x => x.GeneratePasswordHash(newPassword))
                       .Returns(newPassHash);
            _cryptoMock.Setup(x => x.GeneratePasswordHash(invalidPass))
                       .Returns(invalidPassHash);

            //Act
            try
            {
                _sut.ChangePassword(userId, invalidPass, newPassword);
                Assert.Fail("Expected Exception Wasn't thrown");
            }
            catch (InvalidPasswordException e)
            {
                //Assert
                Assert.IsInstanceOf<InvalidPasswordException>(e);
                _uowMock.Verify(x => x.UserRepository.GetById(userId), Times.Once());
                _cryptoMock.Verify(x => x.GeneratePasswordHash(invalidPass), Times.Once());
            }
        }

        [Test]
        public void HashTheNewPasswordIfEverythingIsFine_OnChangePassword()
        {
            //Arrange
            var userId = 12;
            var newPassword = "newPass";
            var newPassHash = "hashpass";
            var oldPassHash = "oldHash";
            var oldPass = "oldPass";
            var user = new User { Password = oldPassHash };

            _uowMock.Setup(x => x.UserRepository.GetById(userId))
                    .Returns(user);
            _cryptoMock.Setup(x => x.GeneratePasswordHash(newPassword))
                       .Returns(newPassHash);
            _cryptoMock.Setup(x => x.GeneratePasswordHash(oldPass))
                       .Returns(oldPassHash);

            //Act
            _sut.ChangePassword(userId, oldPass, newPassword);

            //Assert
            _uowMock.Verify(x => x.UserRepository.GetById(userId), Times.Once());
            _cryptoMock.Verify(x => x.GeneratePasswordHash(newPassword), Times.Once());
            _cryptoMock.Verify(x => x.GeneratePasswordHash(oldPass), Times.Once());
        }

        [Test]
        public void SaveUserCorrectlyWhenEverythingIsFine_OnChangePassword()
        {
            //Arrange
            var userId = 12;
            var newPassword = "newPass";
            var newPassHash = "hashpass";
            var oldPassHash = "oldHash";
            var oldPass = "oldPass";
            var user = new User { Password = oldPassHash };

            _uowMock.Setup(x => x.UserRepository.GetById(userId))
                    .Returns(user);
            _cryptoMock.Setup(x => x.GeneratePasswordHash(newPassword))
                       .Returns(newPassHash);
            _cryptoMock.Setup(x => x.GeneratePasswordHash(oldPass))
                       .Returns(oldPassHash);

            //Act
            _sut.ChangePassword(userId, oldPass, newPassword);

            //Assert

            Assert.AreEqual(newPassHash, user.Password);
            _uowMock.Verify(x => x.SaveChanges());
            _uowMock.Verify(x => x.UserRepository.GetById(userId), Times.Once());
            _cryptoMock.Verify(x => x.GeneratePasswordHash(newPassword), Times.Once());
            _cryptoMock.Verify(x => x.GeneratePasswordHash(oldPass), Times.Once());
        }

        #endregion

        #region Get/Link OAuth User

        [Test]
        public void CheckForOAuthAccountFirstAndNotLookForEmailIfExists_OnGetOAuthUserAsync()
        {
            //Arrange
            var providerId = 1234L;
            var provider = OAuthProvider.Facebook;
            var email = "email";
            var user = new User { Email = email };
            var oauthAccount = new OAuthAccount { ProviderUserId = providerId, Provider = provider };

            _uowMock.Setup(x => x.OAuthAccountRepository.GetOAuthAccountByProviderId(providerId, provider))
                    .Returns(oauthAccount);
            _uowMock.Setup(x => x.UserRepository.GetByEmail(email))
                    .Returns(user);

            //Act
            var result = _sut.GetOAuthUser(oauthAccount, email);

            //Assert
            _uowMock.Verify(x => x.OAuthAccountRepository.GetOAuthAccountByProviderId(providerId, provider), Times.Once());
            _uowMock.Verify(x => x.UserRepository.GetByEmail(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void CheckForUserWithEmailIFOAuthAccountNotExists_OnGetOAuthUserAsync()
        {
            //Arrange
            var providerId = 1234L;
            var provider = OAuthProvider.Facebook;
            var email = "email";
            var user = new User { Email = email };
            var oauthAccount = new OAuthAccount { ProviderUserId = providerId, Provider = provider };

            _uowMock.Setup(x => x.OAuthAccountRepository.GetOAuthAccountByProviderId(providerId, provider))
                    .Returns(() => null);
            _uowMock.Setup(x => x.UserRepository.GetByEmail(email))
                    .Returns(user);

            //Act
            var result = _sut.GetOAuthUser(oauthAccount, email);

            //Assert
            _uowMock.Verify(x => x.OAuthAccountRepository.GetOAuthAccountByProviderId(providerId, provider), Times.Once());
            _uowMock.Verify(x => x.UserRepository.GetByEmail(email), Times.Once());
        }

        [Test]
        public void AddOAuthAccountIfUserExistsAndOAuthAccountIsNot_OnGetOAuthUserAsync()
        {
            //Arrange
            var providerId = 1234L;
            var provider = OAuthProvider.Facebook;
            var email = "email";
            var user = new User { Email = email, Id = 23 };
            var oauthAccount = new OAuthAccount { ProviderUserId = providerId, Provider = provider };

            _uowMock.Setup(x => x.OAuthAccountRepository.GetOAuthAccountByProviderId(providerId, provider))
                    .Returns(() => null);
            _uowMock.Setup(x => x.UserRepository.GetByEmail(email))
                    .Returns(user);
            _uowMock.Setup(x => x.OAuthAccountRepository.InsertGraph(oauthAccount));

            //Act
            var result = _sut.GetOAuthUser(oauthAccount, email);

            //Assert
            _uowMock.Verify(x => x.OAuthAccountRepository.GetOAuthAccountByProviderId(providerId, provider), Times.Once());
            _uowMock.Verify(x => x.UserRepository.GetByEmail(email), Times.Once());
            _uowMock.Verify(x => x.OAuthAccountRepository.InsertGraph(oauthAccount), Times.Once());
            Assert.AreEqual(user.Id, oauthAccount.UserId);
            _uowMock.Verify(x => x.SaveChanges(), Times.Once());
        }

        [Test]
        public void ReturnNullIfNotUserNorOAuthAccountExists_OnGetOAuthUserAsync()
        {
            //Arrange
            var providerId = 1234L;
            var provider = OAuthProvider.Facebook;
            var email = "email";
            var user = new User { Email = email };
            var oauthAccount = new OAuthAccount { ProviderUserId = providerId, Provider = provider };

            _uowMock.Setup(x => x.OAuthAccountRepository.GetOAuthAccountByProviderId(providerId, provider))
                    .Returns(() => null);
            _uowMock.Setup(x => x.UserRepository.GetByEmail(email))
                    .Returns(() => null);

            //Act
            var result = _sut.GetOAuthUser(oauthAccount, email);

            //Assert
            _uowMock.Verify(x => x.OAuthAccountRepository.GetOAuthAccountByProviderId(providerId, provider), Times.Once());
            _uowMock.Verify(x => x.UserRepository.GetByEmail(email), Times.Once());
            Assert.IsNull(result);
        }

        [Test]
        public void NotAddNewOAuthAccountIfItExists_OnAddOAuthAccount()
        {
            //Arrange
            var providerId = 1234L;
            var provider = OAuthProvider.Facebook;

            var oauthAccount = new OAuthAccount { ProviderUserId = providerId, Provider = provider };

            _uowMock.Setup(x => x.OAuthAccountRepository.GetOAuthAccountByProviderId(providerId, provider))
                    .Returns(new OAuthAccount());

            //Act
            _sut.AddOAuthAccount(oauthAccount);

            //Assert

            _uowMock.Verify(x => x.OAuthAccountRepository.GetOAuthAccountByProviderId(providerId, provider),
                            Times.Once());
            _uowMock.Verify(x => x.OAuthAccountRepository.InsertGraph(It.IsAny<OAuthAccount>()), Times.Never());
            _uowMock.Verify(x => x.SaveChanges(), Times.Never());
        }

        [Test]
        public void AddNewOAuthAccountIfItNotExists_OnAddOAuthAccount()
        {
            //Arrange
            var providerId = 1234L;
            var provider = OAuthProvider.Facebook;

            var oauthAccount = new OAuthAccount { ProviderUserId = providerId, Provider = provider };

            _uowMock.Setup(x => x.OAuthAccountRepository.GetOAuthAccountByProviderId(providerId, provider))
                    .Returns(() => null);

            //Act
            _sut.AddOAuthAccount(oauthAccount);

            //Assert

            _uowMock.Verify(x => x.OAuthAccountRepository.GetOAuthAccountByProviderId(providerId, provider),
                            Times.Once());
            _uowMock.Verify(x => x.OAuthAccountRepository.InsertGraph(oauthAccount), Times.Once());
            _uowMock.Verify(x => x.SaveChanges(), Times.Once());
        }

        #endregion
    }
}