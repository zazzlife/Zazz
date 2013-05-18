using System;
using System.Text;
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
        private byte[] _password;
        private byte[] _iv;
        private User _user;
        private string _pass;

        [SetUp]
        public void Init()
        {
            _pass = "pass";
            _password = new byte[] {1, 2, 3, 4, 5, 6};
            _iv = new byte[] { 1, 2, 3, 3, 4, 5 };
            _user = new User
                    {
                        Id = 22,
                        Username = "username",
                        Email = "email",
                        Password = _password,
                        PasswordIV = _iv,
                    };

            _uowMock = new Mock<IUoW>();
            _cryptoMock = new Mock<ICryptoService>();
            _sut = new AuthService(_uowMock.Object, _cryptoMock.Object);

            _uowMock.Setup(x => x.SaveChanges());
        }

        #region Login
        [Test]
        public void DecryptUserPassword_OnLogin()
        {
            //Arrange
            var pass = "password";
            
            _uowMock.Setup(x => x.UserRepository.GetByUsername(It.IsAny<string>(),
                It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()))
                    .Returns(_user);
            _cryptoMock.Setup(x => x.DecryptPassword(_user.Password, _user.PasswordIV))
                       .Returns(pass);


            //Act
            _sut.Login("user", pass);

            //Assert
            _cryptoMock.Verify(x => x.DecryptPassword(_user.Password, _user.PasswordIV), Times.Once());
        }

        [Test]
        public void ThrowUserNotExist_WhenUserNotExists_OnLogin()
        {
            //Arrange
            _uowMock.Setup(x => x.UserRepository.GetByUsername(It.IsAny<string>(),
                It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()))
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
            _uowMock.Setup(x => x.UserRepository.GetByUsername(It.IsAny<string>(),
                It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()))
                    .Returns(_user);

            _cryptoMock.Setup(x => x.DecryptPassword(_user.Password, _user.PasswordIV))
                       .Returns("valid pass");

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
            _user.LastActivity = DateTime.MaxValue;
            _uowMock.Setup(x => x.UserRepository.GetByUsername(It.IsAny<string>(),
                It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()))
                    .Returns(_user);
            _cryptoMock.Setup(x => x.DecryptPassword(_user.Password, _user.PasswordIV))
                       .Returns(pass);

            //Act
            _sut.Login("user", pass);

            //Assert
            Assert.IsTrue(_user.LastActivity <= DateTime.UtcNow);
        }

        [Test]
        public void CallSaveChanges_WhenEverythingIsOk_OnLogin()
        {
            //Arrange
            var pass = "pass";
            _uowMock.Setup(x => x.UserRepository.GetByUsername(It.IsAny<string>(),
                It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()))
                    .Returns(_user);

            _cryptoMock.Setup(x => x.DecryptPassword(_user.Password, _user.PasswordIV))
                       .Returns(pass);

            //Act
            _sut.Login("user", pass);

            //Assert
            _uowMock.Verify(x => x.SaveChanges(), Times.Once());
        }
        #endregion

        #region Register
        [Test]
        public void CheckForExistingUsername_OnRegister()
        {
            //Arrange
            _uowMock.Setup(x => x.UserRepository.ExistsByUsername(_user.Username))
                    .Returns(false);
            _uowMock.Setup(x => x.UserRepository.ExistsByEmail(_user.Email))
                    .Returns(false);

            //Act
            _sut.Register(_user, _pass, false);

            //Assert
            _uowMock.Verify(x => x.UserRepository.ExistsByUsername(_user.Username), Times.Once());
        }

        [Test]
        public void CheckForExistingEmail_OnRegister()
        {
            //Arrange
            _uowMock.Setup(x => x.UserRepository.ExistsByUsername(_user.Username))
                    .Returns(false);
            _uowMock.Setup(x => x.UserRepository.ExistsByEmail(_user.Email))
                    .Returns(false);

            //Act
            _sut.Register(_user, _pass, false);

            //Assert
            _uowMock.Verify(x => x.UserRepository.ExistsByEmail(_user.Email), Times.Once());
        }

        [Test]
        public void ThrowIfUsernameExists_OnRegister()
        {
            //Arrange
            
            _uowMock.Setup(x => x.UserRepository.ExistsByUsername(_user.Username))
                    .Returns(true);
            _uowMock.Setup(x => x.UserRepository.ExistsByEmail(_user.Email))
                    .Returns(false);

            //Act
            try
            {
                _sut.Register(_user, _pass, false);
                Assert.Fail("Expected exception wasn't thrown");
            }
            catch (UsernameExistsException e)
            {
                //Assert
                Assert.IsInstanceOf<UsernameExistsException>(e);
            }
        }

        [Test]
        public void ThrowIfEmailExists_OnRegister()
        {
            //Arrange
            _uowMock.Setup(x => x.UserRepository.ExistsByUsername(_user.Username))
                    .Returns(false);
            _uowMock.Setup(x => x.UserRepository.ExistsByEmail(_user.Email))
                    .Returns(true);

            //Act
            try
            {
                _sut.Register(_user, _pass, false);
                Assert.Fail("Expected exception wasn't thrown");
            }
            catch (EmailExistsException e)
            {
                //Assert
                Assert.IsInstanceOf<EmailExistsException>(e);
            }
        }

        [Test]
        public void EncryptThePassword_OnRegister()
        {
            //Arrange
            _uowMock.Setup(x => x.UserRepository.ExistsByUsername(_user.Username))
                    .Returns(false);
            _uowMock.Setup(x => x.UserRepository.ExistsByEmail(_user.Email))
                    .Returns(false);

            var ivBuffer = new byte[] { 1, 2 };
            var iv = Convert.ToBase64String(ivBuffer);
            var encryptedPass = new byte[] {7, 8, 9};
            

            _cryptoMock.Setup(x => x.EncryptPassword(_pass, out iv))
                       .Returns(encryptedPass);

            //Act
            _sut.Register(_user, _pass, false);

            //Assert
            _cryptoMock.Verify(x => x.EncryptPassword(_pass, out iv), Times.Once());
            CollectionAssert.AreEqual(encryptedPass, _user.Password);
            CollectionAssert.AreEqual(Convert.FromBase64String(iv), _user.PasswordIV);
        }

        [Test]
        public void GenerateValidationTokenIfRequested_OnRegister()
        {
            //Arrange
            _uowMock.Setup(x => x.UserRepository.ExistsByUsername(_user.Username))
                    .Returns(false);
            _uowMock.Setup(x => x.UserRepository.ExistsByEmail(_user.Email))
                    .Returns(false);
            
            var iv = String.Empty;
            _cryptoMock.Setup(x => x.EncryptPassword(_pass, out iv))
                       .Returns(_password);


            //Act
            _sut.Register(_user, _pass, true);

            //Assert
            Assert.IsNotNull(_user.ValidationToken);
            Assert.AreEqual(DateTime.UtcNow.AddYears(1).Date, _user.ValidationToken.ExpirationDate.Date);
            Assert.IsNotNull(_user.ValidationToken.Token);
        }

        [Test]
        public void SaveUserWhenEverythingIsOk_OnRegister()
        {
            //Arrange
            _uowMock.Setup(x => x.UserRepository.ExistsByUsername(_user.Username))
                    .Returns(false);
            _uowMock.Setup(x => x.UserRepository.ExistsByEmail(_user.Email))
                    .Returns(false);
            var iv = String.Empty;
            _cryptoMock.Setup(x => x.EncryptPassword(_pass, out iv))
                       .Returns(_password);

            //Act
            _sut.Register(_user, _pass, false);

            //Assert
            _uowMock.Verify(x => x.UserRepository.InsertGraph(_user), Times.Once());
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
        public void EncryptTheNewPassword_OnResetPassword()
        {
            //Arrange
            var newPass = "newpass";
            var newPassBuffer = new byte[] {12, 34, 45};
            var iv = new byte[] {67, 89};
            var ivText = Convert.ToBase64String(iv);

            var token = new ValidationToken { Id = 12, Token = Guid.NewGuid(), ExpirationDate = DateTime.UtcNow.AddDays(1) };

            _cryptoMock.Setup(x => x.EncryptPassword(newPass, out ivText))
                       .Returns(() => newPassBuffer);

            _uowMock.Setup(x => x.ValidationTokenRepository.GetById(token.Id))
                .Returns(token);
            _uowMock.Setup(x => x.UserRepository.GetById(token.Id))
                    .Returns(_user);
            _uowMock.Setup(x => x.ValidationTokenRepository.Remove(_user.Id));

            //Act
            _sut.ResetPassword(token.Id, token.Token, newPass);

            //Assert
            _cryptoMock.Verify(x => x.EncryptPassword(newPass, out ivText), Times.Once());
        }

        [Test]
        public void UpdateTheUserCorrectly_OnResetPassword()
        {
            //Arrange
            var newPass = "newpass";
            var newPassBuffer = new byte[] { 12, 34, 45 };
            var iv = new byte[] { 67, 89 };
            var ivText = Convert.ToBase64String(iv);

            var token = new ValidationToken { Id = 12, Token = Guid.NewGuid(), ExpirationDate = DateTime.UtcNow.AddDays(1) };

            _cryptoMock.Setup(x => x.EncryptPassword(newPass, out ivText))
                       .Returns(() => newPassBuffer);

            _uowMock.Setup(x => x.ValidationTokenRepository.GetById(token.Id))
                .Returns(token);
            _uowMock.Setup(x => x.UserRepository.GetById(token.Id))
                    .Returns(_user);
            _uowMock.Setup(x => x.ValidationTokenRepository.Remove(_user.Id));

            //Act
            _sut.ResetPassword(token.Id, token.Token, newPass);

            //Assert
            CollectionAssert.AreEqual(newPassBuffer, _user.Password);
            CollectionAssert.AreEqual(iv, _user.PasswordIV);
            _uowMock.Verify(x => x.SaveChanges(), Times.Once());
        }

        [Test]
        public void RemoveTokenOnSuccessfulReset_OnResetPassword()
        {
            //Arrange
            var newPass = "newpass";
            var newPassBuffer = new byte[] { 12, 34, 45 };
            var iv = new byte[] { 67, 89 };
            var ivText = Convert.ToBase64String(iv);

            var token = new ValidationToken { Id = 12, Token = Guid.NewGuid(), ExpirationDate = DateTime.UtcNow.AddDays(1) };

            _cryptoMock.Setup(x => x.EncryptPassword(newPass, out ivText))
                       .Returns(() => newPassBuffer);

            _uowMock.Setup(x => x.ValidationTokenRepository.GetById(token.Id))
                .Returns(token);
            _uowMock.Setup(x => x.UserRepository.GetById(token.Id))
                    .Returns(_user);
            _uowMock.Setup(x => x.ValidationTokenRepository.Remove(_user.Id));

            //Act
            _sut.ResetPassword(token.Id, token.Token, newPass);

            //Assert
            _uowMock.Verify(x => x.SaveChanges(), Times.Once());
            _uowMock.Verify(x => x.ValidationTokenRepository.Remove(_user.Id));
        }

        #endregion

        #region Change Password

        [Test]
        public void ThrowInvalidPasswordIfCurrentPasswordIsNotCorrect_OnChangePassword()
        {
            //Arrange
            var pass = "pass";
            var newPass = "newPass";
            var invalidPass = "invalid";
            _uowMock.Setup(x => x.UserRepository.GetById(_user.Id))
                    .Returns(_user);
            _cryptoMock.Setup(x => x.DecryptPassword(_user.Password, _user.PasswordIV))
                       .Returns(pass);

            //Act
            try
            {
                _sut.ChangePassword(_user.Id, invalidPass, newPass);
                Assert.Fail("Expected Exception Wasn't thrown");
            }
            catch (InvalidPasswordException e)
            {
                //Assert
                Assert.IsInstanceOf<InvalidPasswordException>(e);
                _uowMock.Verify(x => x.UserRepository.GetById(_user.Id), Times.Once());
                _cryptoMock.Verify(x => x.DecryptPassword(_user.Password, _user.PasswordIV), Times.Once());
            }
        }

        [Test]
        public void EncryptTheNewPasswordIfEverythingIsFine_OnChangePassword()
        {
            //Arrange
            var oldPassBuffer = _user.Password;
            var oldPassIv = _user.PasswordIV;

            var pass = "pass";
            var newPass = "newPass";
            var newPassIv = Convert.ToBase64String(Encoding.UTF8.GetBytes("iv"));
            var newPassBuffer = Encoding.UTF8.GetBytes(newPass);
            var newPassIvBuffer = Convert.FromBase64String(newPassIv);

            _uowMock.Setup(x => x.UserRepository.GetById(_user.Id))
                    .Returns(_user);
            _cryptoMock.Setup(x => x.DecryptPassword(oldPassBuffer, oldPassIv))
                       .Returns(pass);
            _cryptoMock.Setup(x => x.EncryptPassword(newPass, out newPassIv))
                       .Returns(newPassBuffer);

            //Act
            _sut.ChangePassword(_user.Id, pass, newPass);

            //Assert
            _uowMock.Verify(x => x.UserRepository.GetById(_user.Id), Times.Once());
            _cryptoMock.Verify(x => x.DecryptPassword(oldPassBuffer, oldPassIv), Times.Once());
            _cryptoMock.Verify(x => x.EncryptPassword(newPass, out newPassIv), Times.Once());
            
        }

        [Test]
        public void SaveUserCorrectlyWhenEverythingIsFine_OnChangePassword()
        {
            //Arrange
            var pass = "pass";
            var newPass = "newPass";
            var newPassIv = Convert.ToBase64String(Encoding.UTF8.GetBytes("iv"));
            var newPassBuffer = Encoding.UTF8.GetBytes(newPass);
            var newPassIvBuffer = Convert.FromBase64String(newPassIv);

            _uowMock.Setup(x => x.UserRepository.GetById(_user.Id))
                    .Returns(_user);
            _cryptoMock.Setup(x => x.DecryptPassword(_user.Password, _user.PasswordIV))
                       .Returns(pass);
            _cryptoMock.Setup(x => x.EncryptPassword(newPass, out newPassIv))
                       .Returns(newPassBuffer);

            //Act
            _sut.ChangePassword(_user.Id, pass, newPass);

            //Assert
            CollectionAssert.AreEqual(newPassBuffer, _user.Password);
            CollectionAssert.AreEqual(newPassIvBuffer, _user.PasswordIV);
            _uowMock.Verify(x => x.SaveChanges());
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