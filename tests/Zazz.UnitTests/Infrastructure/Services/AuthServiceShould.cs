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
        private Mock<IUoW> _uow;
        private Mock<ICryptoService> _cryptoService;
        private AuthService _sut;
        private byte[] _passBuffer;
        private byte[] _iv;
        private User _user;
        private string _pass;
        private MockRepository _mockRepo;

        [SetUp]
        public void Init()
        {
            _pass = "pass";
            _passBuffer = new byte[] { 1, 2, 3, 4, 5, 6 };
            _iv = new byte[] { 1, 2, 3, 3, 4, 5 };
            _user = new User
                    {
                        Id = 22,
                        Username = "username",
                        Email = "email",
                        Password = _passBuffer,
                        PasswordIV = _iv,
                    };


            _mockRepo = new MockRepository(MockBehavior.Strict);

            _uow = _mockRepo.Create<IUoW>();
            _cryptoService = _mockRepo.Create<ICryptoService>();

            _sut = new AuthService(_uow.Object, _cryptoService.Object);
        }

        #region Login

        [Test]
        public void ThrowUserNotExist_WhenUserNotExists_OnLogin()
        {
            //Arrange
            _uow.Setup(x => x.UserRepository.GetByUsername(_user.Username, false, false, false, false))
                    .Returns(() => null);

            //Act
            Assert.Throws<NotFoundException>(() => _sut.Login(_user.Username, "pass"));

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void ThrowInvalidPassword_WhenPasswordsDontMatch_OnLogin()
        {
            //Arrange
            _uow.Setup(x => x.UserRepository.GetByUsername(_user.Username, false, false, false, false))
                    .Returns(_user);

            _cryptoService.Setup(x => x.DecryptPassword(_user.Password, _user.PasswordIV))
                       .Returns("valid pass");

            //Act
            Assert.Throws<InvalidPasswordException>(() => _sut.Login(_user.Username, "invalidPassword"));

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void UpdateLastActivity_WhenEverythingIsOk_OnLogin()
        {
            //Arrange
            _user.LastActivity = DateTime.MaxValue;

            _uow.Setup(x => x.UserRepository.GetByUsername(_user.Username, false, false, false, false))
                    .Returns(_user);

            _cryptoService.Setup(x => x.DecryptPassword(_user.Password, _user.PasswordIV))
                       .Returns(_pass);

            _uow.Setup(x => x.SaveChanges());

            //Act
            _sut.Login(_user.Username, _pass);

            //Assert
            Assert.IsTrue(_user.LastActivity <= DateTime.UtcNow);
            _mockRepo.VerifyAll();
        }

        #endregion

        #region Register

        [Test]
        public void ThrowIfPasswordIsMoreThan20Char_OnRegister()
        {
            //Arrange
            _pass = "123456789012345678901";
            Assert.IsTrue(_pass.Length > 20);

            //Act & Assert
            Assert.Throws<PasswordTooLongException>(() => _sut.Register(_user, _pass, false));
        }

        [Test]
        public void CheckForExistingUsername_OnRegister()
        {
            //Arrange
            _uow.Setup(x => x.UserRepository.ExistsByUsername(_user.Username))
                    .Returns(false);
            _uow.Setup(x => x.UserRepository.ExistsByEmail(_user.Email))
                    .Returns(false);

            //Act
            _sut.Register(_user, _pass, false);

            //Assert
            _uow.Verify(x => x.UserRepository.ExistsByUsername(_user.Username), Times.Once());
        }

        [Test]
        public void CheckForExistingEmail_OnRegister()
        {
            //Arrange
            _uow.Setup(x => x.UserRepository.ExistsByUsername(_user.Username))
                    .Returns(false);
            _uow.Setup(x => x.UserRepository.ExistsByEmail(_user.Email))
                    .Returns(false);

            //Act
            _sut.Register(_user, _pass, false);

            //Assert
            _uow.Verify(x => x.UserRepository.ExistsByEmail(_user.Email), Times.Once());
        }

        [Test]
        public void ThrowIfUsernameExists_OnRegister()
        {
            //Arrange

            _uow.Setup(x => x.UserRepository.ExistsByUsername(_user.Username))
                    .Returns(true);
            _uow.Setup(x => x.UserRepository.ExistsByEmail(_user.Email))
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
            _uow.Setup(x => x.UserRepository.ExistsByUsername(_user.Username))
                    .Returns(false);
            _uow.Setup(x => x.UserRepository.ExistsByEmail(_user.Email))
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
            _uow.Setup(x => x.UserRepository.ExistsByUsername(_user.Username))
                    .Returns(false);
            _uow.Setup(x => x.UserRepository.ExistsByEmail(_user.Email))
                    .Returns(false);

            var ivBuffer = new byte[] { 1, 2 };
            var iv = Convert.ToBase64String(ivBuffer);
            var encryptedPass = new byte[] { 7, 8, 9 };


            _cryptoService.Setup(x => x.EncryptPassword(_pass, out iv))
                       .Returns(encryptedPass);

            //Act
            _sut.Register(_user, _pass, false);

            //Assert
            _cryptoService.Verify(x => x.EncryptPassword(_pass, out iv), Times.Once());
            CollectionAssert.AreEqual(encryptedPass, _user.Password);
            CollectionAssert.AreEqual(Convert.FromBase64String(iv), _user.PasswordIV);
        }

        [Test]
        public void GenerateValidationTokenIfRequested_OnRegister()
        {
            //Arrange
            _uow.Setup(x => x.UserRepository.ExistsByUsername(_user.Username))
                    .Returns(false);
            _uow.Setup(x => x.UserRepository.ExistsByEmail(_user.Email))
                    .Returns(false);

            var iv = String.Empty;
            _cryptoService.Setup(x => x.EncryptPassword(_pass, out iv))
                       .Returns(_passBuffer);


            //Act
            _sut.Register(_user, _pass, true);

            //Assert
            Assert.IsNotNull(_user.UserValidationToken);
            Assert.AreEqual(DateTime.UtcNow.AddYears(1).Date, _user.UserValidationToken.ExpirationTime.Date);
            Assert.IsNotNull(_user.UserValidationToken.Token);
        }

        [Test]
        public void SaveUserWhenEverythingIsOk_OnRegister()
        {
            //Arrange
            _uow.Setup(x => x.UserRepository.ExistsByUsername(_user.Username))
                    .Returns(false);
            _uow.Setup(x => x.UserRepository.ExistsByEmail(_user.Email))
                    .Returns(false);
            var iv = String.Empty;
            _cryptoService.Setup(x => x.EncryptPassword(_pass, out iv))
                       .Returns(_passBuffer);

            //Act
            _sut.Register(_user, _pass, false);

            //Assert
            _uow.Verify(x => x.UserRepository.InsertGraph(_user), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Once());
        }
        #endregion

        #region Generate Reset Password Token

        [Test]
        public void GetUserIdByEmail_OnGenerateResetToken()
        {
            //Arrange
            var email = "email";
            _uow.Setup(x => x.UserRepository.GetIdByEmail(email))
                    .Returns(12);
            _uow.Setup(x => x.ValidationTokenRepository.GetById(It.IsAny<int>()))
                    .Returns(() => null);

            //Act
            _sut.GenerateResetPasswordToken(email);

            //Assert
            _uow.Verify(x => x.UserRepository.GetIdByEmail(email), Times.Once());
        }

        [Test]
        public void ThrowEmailNotExists_WhenEmailNotExists_OnGenerateResetToken()
        {
            //Arrange
            var email = "email";
            _uow.Setup(x => x.UserRepository.GetIdByEmail(email))
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
            _uow.Setup(x => x.UserRepository.GetIdByEmail(email))
                    .Returns(userId);
            _uow.Setup(x => x.ValidationTokenRepository.GetById(userId))
                    .Returns(() => null);

            //Act
            var result = _sut.GenerateResetPasswordToken(email);

            //Assert
            Assert.AreEqual(DateTime.UtcNow.AddDays(1).Date, result.ExpirationTime.Date);
            Assert.IsNotNull(result.Token);
            Assert.AreEqual(userId, result.Id);
        }

        [Test]
        public void DeleteOldTokenRecordIfExists_OnGenerateResetToken()
        {
            //Arrange
            var email = "email";
            var userId = 1;
            var token = new UserValidationToken();
            _uow.Setup(x => x.UserRepository.GetIdByEmail(email))
                    .Returns(userId);
            _uow.Setup(x => x.ValidationTokenRepository.GetById(userId))
                    .Returns(token);
            _uow.Setup(x => x.ValidationTokenRepository.Remove(token));

            //Act
            var result = _sut.GenerateResetPasswordToken(email);

            //Assert
            _uow.Verify(x => x.ValidationTokenRepository.GetById(userId), Times.Once());
            _uow.Verify(x => x.ValidationTokenRepository.Remove(token), Times.Once());

        }

        [Test]
        public void NotCallRemoveWhenOldTokenNotExists_OnGenerateResetToken()
        {
            //Arrange
            var email = "email";
            var userId = 1;
            var token = new UserValidationToken();
            _uow.Setup(x => x.UserRepository.GetIdByEmail(email))
                    .Returns(userId);
            _uow.Setup(x => x.ValidationTokenRepository.GetById(userId))
                    .Returns(() => null);
            _uow.Setup(x => x.ValidationTokenRepository.Remove(token));

            //Act
            var result = _sut.GenerateResetPasswordToken(email);

            //Assert
            _uow.Verify(x => x.ValidationTokenRepository.GetById(userId), Times.Once());
            _uow.Verify(x => x.ValidationTokenRepository.Remove(token), Times.Never());
        }

        [Test]
        public void SaveWhenEverythingIsOk_OnGenerateResetToken()
        {
            //Arrange
            var email = "email";
            var userId = 1;
            var token = new UserValidationToken();
            _uow.Setup(x => x.UserRepository.GetIdByEmail(email))
                    .Returns(userId);
            _uow.Setup(x => x.ValidationTokenRepository.GetById(userId))
                    .Returns(() => null);
            _uow.Setup(x => x.ValidationTokenRepository.Remove(token));

            //Act
            var result = _sut.GenerateResetPasswordToken(email);

            //Assert
            _uow.Verify(x => x.ValidationTokenRepository.InsertGraph(result));
            _uow.Verify(x => x.SaveChanges(), Times.Once());
        }

        #endregion

        #region Is Token Valid

        [Test]
        public void RetrunTrueWhenTokenIsValid_OnIsTokenValid()
        {
            //Arrange
            var token = new UserValidationToken { Id = 12, Token = Guid.NewGuid(), ExpirationTime = DateTime.UtcNow.AddDays(1) };
            _uow.Setup(x => x.ValidationTokenRepository.GetById(token.Id))
                    .Returns(() => token);

            //Act
            var result = _sut.IsTokenValid(token.Id, token.Token);

            //Assert
            Assert.IsTrue(result);
            _uow.Verify(x => x.ValidationTokenRepository.GetById(token.Id), Times.Once());
        }

        [Test]
        public void RetrunFalseWhenTokenIsNotValid_OnIsTokenValid()
        {
            //Arrange
            var token = new UserValidationToken { Id = 12, Token = Guid.NewGuid(), ExpirationTime = DateTime.UtcNow.AddDays(1) };
            _uow.Setup(x => x.ValidationTokenRepository.GetById(token.Id))
                    .Returns(() => token);

            //Act
            var result = _sut.IsTokenValid(token.Id, Guid.NewGuid());

            //Assert
            Assert.IsFalse(result);
            _uow.Verify(x => x.ValidationTokenRepository.GetById(token.Id), Times.Once());

        }

        [Test]
        public void ThrowExpiredExceptionWhenTokenIsExpired_OnIsTokenValid()
        {
            //Arrange
            var token = new UserValidationToken { Id = 12, Token = Guid.NewGuid(), ExpirationTime = DateTime.UtcNow.AddDays(-1) };
            _uow.Setup(x => x.ValidationTokenRepository.GetById(token.Id))
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
                _uow.Verify(x => x.ValidationTokenRepository.GetById(token.Id), Times.Once());
                Assert.IsInstanceOf<TokenExpiredException>(e);
            }
        }

        #endregion

        #region Reset Password

        [Test]
        public void ThrowIfPasswordIsTooLong_OnResetPassword()
        {
            //Arrange
            _pass = "123456789012345678901";
            Assert.IsTrue(_pass.Length > 20);

            //Act & Assert
            Assert.Throws<PasswordTooLongException>(() => _sut.ResetPassword(_user.Id, Guid.NewGuid(), _pass));
        }

        [Test]
        public void ThrowInvalidTokenExceptionWhenTokenIsNotValid_OnResetPassword()
        {
            //Arrange
            var token = new UserValidationToken { Id = 12, Token = Guid.NewGuid(), ExpirationTime = DateTime.UtcNow.AddDays(1) };
            _uow.Setup(x => x.ValidationTokenRepository.GetById(token.Id));

            //Act
            try
            {
                _sut.ResetPassword(token.Id, Guid.NewGuid(), "");
                Assert.Fail("Expected Exception wasn't thrown");
            }
            catch (InvalidTokenException e)
            {
                //Assert
                _uow.Verify(x => x.ValidationTokenRepository.GetById(token.Id), Times.Once());
                Assert.IsInstanceOf<InvalidTokenException>(e);
            }
        }

        [Test]
        public void EncryptTheNewPassword_OnResetPassword()
        {
            //Arrange
            var newPass = "newpass";
            var newPassBuffer = new byte[] { 12, 34, 45 };
            var iv = new byte[] { 67, 89 };
            var ivText = Convert.ToBase64String(iv);

            var token = new UserValidationToken { Id = 12, Token = Guid.NewGuid(), ExpirationTime = DateTime.UtcNow.AddDays(1) };

            _cryptoService.Setup(x => x.EncryptPassword(newPass, out ivText))
                       .Returns(() => newPassBuffer);

            _uow.Setup(x => x.ValidationTokenRepository.GetById(token.Id))
                .Returns(token);
            _uow.Setup(x => x.UserRepository.GetById(token.Id, false, false, false, false))
                    .Returns(_user);
            _uow.Setup(x => x.ValidationTokenRepository.Remove(_user.Id));

            //Act
            _sut.ResetPassword(token.Id, token.Token, newPass);

            //Assert
            _cryptoService.Verify(x => x.EncryptPassword(newPass, out ivText), Times.Once());
        }

        [Test]
        public void UpdateTheUserCorrectly_OnResetPassword()
        {
            //Arrange
            var newPass = "newpass";
            var newPassBuffer = new byte[] { 12, 34, 45 };
            var iv = new byte[] { 67, 89 };
            var ivText = Convert.ToBase64String(iv);

            var token = new UserValidationToken { Id = 12, Token = Guid.NewGuid(), ExpirationTime = DateTime.UtcNow.AddDays(1) };

            _cryptoService.Setup(x => x.EncryptPassword(newPass, out ivText))
                       .Returns(() => newPassBuffer);

            _uow.Setup(x => x.ValidationTokenRepository.GetById(token.Id))
                .Returns(token);
            _uow.Setup(x => x.UserRepository.GetById(token.Id, false, false, false, false))
                    .Returns(_user);
            _uow.Setup(x => x.ValidationTokenRepository.Remove(_user.Id));

            //Act
            _sut.ResetPassword(token.Id, token.Token, newPass);

            //Assert
            CollectionAssert.AreEqual(newPassBuffer, _user.Password);
            CollectionAssert.AreEqual(iv, _user.PasswordIV);
            _uow.Verify(x => x.SaveChanges(), Times.Once());
        }

        [Test]
        public void RemoveTokenOnSuccessfulReset_OnResetPassword()
        {
            //Arrange
            var newPass = "newpass";
            var newPassBuffer = new byte[] { 12, 34, 45 };
            var iv = new byte[] { 67, 89 };
            var ivText = Convert.ToBase64String(iv);

            var token = new UserValidationToken { Id = 12, Token = Guid.NewGuid(), ExpirationTime = DateTime.UtcNow.AddDays(1) };

            _cryptoService.Setup(x => x.EncryptPassword(newPass, out ivText))
                       .Returns(() => newPassBuffer);

            _uow.Setup(x => x.ValidationTokenRepository.GetById(token.Id))
                .Returns(token);
            _uow.Setup(x => x.UserRepository.GetById(token.Id, false, false, false, false))
                    .Returns(_user);
            _uow.Setup(x => x.ValidationTokenRepository.Remove(_user.Id));

            //Act
            _sut.ResetPassword(token.Id, token.Token, newPass);

            //Assert
            _uow.Verify(x => x.SaveChanges(), Times.Once());
            _uow.Verify(x => x.ValidationTokenRepository.Remove(_user.Id));
        }

        #endregion

        #region Change Password

        [Test]
        public void ThrowIfPasswordIsTooLong_OnChangePassword()
        {
            //Arrange
            _pass = "123456789012345678901";
            Assert.IsTrue(_pass.Length > 20);

            //Act & Assert
            Assert.Throws<PasswordTooLongException>(() => _sut.ChangePassword(_user.Id, "pass", _pass));
        }

        [Test]
        public void ThrowInvalidPasswordIfCurrentPasswordIsNotCorrect_OnChangePassword()
        {
            //Arrange
            var pass = "pass";
            var newPass = "newPass";
            var invalidPass = "invalid";
            _uow.Setup(x => x.UserRepository.GetById(_user.Id, false, false, false, false))
                    .Returns(_user);
            _cryptoService.Setup(x => x.DecryptPassword(_user.Password, _user.PasswordIV))
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
                _uow.Verify(x => x.UserRepository.GetById(_user.Id, false, false, false, false), Times.Once());
                _cryptoService.Verify(x => x.DecryptPassword(_user.Password, _user.PasswordIV), Times.Once());
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

            _uow.Setup(x => x.UserRepository.GetById(_user.Id, false, false, false, false))
                    .Returns(_user);
            _cryptoService.Setup(x => x.DecryptPassword(oldPassBuffer, oldPassIv))
                       .Returns(pass);
            _cryptoService.Setup(x => x.EncryptPassword(newPass, out newPassIv))
                       .Returns(newPassBuffer);

            //Act
            _sut.ChangePassword(_user.Id, pass, newPass);

            //Assert
            _uow.Verify(x => x.UserRepository.GetById(_user.Id, false, false, false, false), Times.Once());
            _cryptoService.Verify(x => x.DecryptPassword(oldPassBuffer, oldPassIv), Times.Once());
            _cryptoService.Verify(x => x.EncryptPassword(newPass, out newPassIv), Times.Once());

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

            _uow.Setup(x => x.UserRepository.GetById(_user.Id, false, false, false, false))
                    .Returns(_user);
            _cryptoService.Setup(x => x.DecryptPassword(_user.Password, _user.PasswordIV))
                       .Returns(pass);
            _cryptoService.Setup(x => x.EncryptPassword(newPass, out newPassIv))
                       .Returns(newPassBuffer);

            //Act
            _sut.ChangePassword(_user.Id, pass, newPass);

            //Assert
            CollectionAssert.AreEqual(newPassBuffer, _user.Password);
            CollectionAssert.AreEqual(newPassIvBuffer, _user.PasswordIV);
            _uow.Verify(x => x.SaveChanges());
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

            _uow.Setup(x => x.OAuthAccountRepository.GetOAuthAccountByProviderId(providerId, provider))
                    .Returns(oauthAccount);
            _uow.Setup(x => x.UserRepository.GetByEmail(email))
                    .Returns(user);

            //Act
            var result = _sut.GetOAuthUser(oauthAccount, email);

            //Assert
            _uow.Verify(x => x.OAuthAccountRepository.GetOAuthAccountByProviderId(providerId, provider), Times.Once());
            _uow.Verify(x => x.UserRepository.GetByEmail(It.IsAny<string>()), Times.Never());
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

            _uow.Setup(x => x.OAuthAccountRepository.GetOAuthAccountByProviderId(providerId, provider))
                    .Returns(() => null);
            _uow.Setup(x => x.UserRepository.GetByEmail(email))
                    .Returns(user);

            //Act
            var result = _sut.GetOAuthUser(oauthAccount, email);

            //Assert
            _uow.Verify(x => x.OAuthAccountRepository.GetOAuthAccountByProviderId(providerId, provider), Times.Once());
            _uow.Verify(x => x.UserRepository.GetByEmail(email), Times.Once());
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

            _uow.Setup(x => x.OAuthAccountRepository.GetOAuthAccountByProviderId(providerId, provider))
                    .Returns(() => null);
            _uow.Setup(x => x.UserRepository.GetByEmail(email))
                    .Returns(user);
            _uow.Setup(x => x.OAuthAccountRepository.InsertGraph(oauthAccount));

            //Act
            var result = _sut.GetOAuthUser(oauthAccount, email);

            //Assert
            _uow.Verify(x => x.OAuthAccountRepository.GetOAuthAccountByProviderId(providerId, provider), Times.Once());
            _uow.Verify(x => x.UserRepository.GetByEmail(email), Times.Once());
            _uow.Verify(x => x.OAuthAccountRepository.InsertGraph(oauthAccount), Times.Once());
            Assert.AreEqual(user.Id, oauthAccount.UserId);
            _uow.Verify(x => x.SaveChanges(), Times.Once());
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

            _uow.Setup(x => x.OAuthAccountRepository.GetOAuthAccountByProviderId(providerId, provider))
                    .Returns(() => null);
            _uow.Setup(x => x.UserRepository.GetByEmail(email))
                    .Returns(() => null);

            //Act
            var result = _sut.GetOAuthUser(oauthAccount, email);

            //Assert
            _uow.Verify(x => x.OAuthAccountRepository.GetOAuthAccountByProviderId(providerId, provider), Times.Once());
            _uow.Verify(x => x.UserRepository.GetByEmail(email), Times.Once());
            Assert.IsNull(result);
        }

        [Test]
        public void NotAddNewOAuthAccountIfItExists_OnAddOAuthAccount()
        {
            //Arrange
            var providerId = 1234L;
            var provider = OAuthProvider.Facebook;

            var oauthAccount = new OAuthAccount { ProviderUserId = providerId, Provider = provider };

            _uow.Setup(x => x.OAuthAccountRepository.GetOAuthAccountByProviderId(providerId, provider))
                    .Returns(new OAuthAccount());

            //Act
            _sut.AddOAuthAccount(oauthAccount);

            //Assert

            _uow.Verify(x => x.OAuthAccountRepository.GetOAuthAccountByProviderId(providerId, provider),
                            Times.Once());
            _uow.Verify(x => x.OAuthAccountRepository.InsertGraph(It.IsAny<OAuthAccount>()), Times.Never());
            _uow.Verify(x => x.SaveChanges(), Times.Never());
        }

        [Test]
        public void AddNewOAuthAccountIfItNotExists_OnAddOAuthAccount()
        {
            //Arrange
            var providerId = 1234L;
            var provider = OAuthProvider.Facebook;

            var oauthAccount = new OAuthAccount { ProviderUserId = providerId, Provider = provider };

            _uow.Setup(x => x.OAuthAccountRepository.GetOAuthAccountByProviderId(providerId, provider))
                    .Returns(() => null);

            //Act
            _sut.AddOAuthAccount(oauthAccount);

            //Assert

            _uow.Verify(x => x.OAuthAccountRepository.GetOAuthAccountByProviderId(providerId, provider),
                            Times.Once());
            _uow.Verify(x => x.OAuthAccountRepository.InsertGraph(oauthAccount), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Once());
        }

        #endregion

        #region Update Access Token

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void ThrowIfAccessTokenIsInvalid_OnUpdateAccessToken(string token)
        {
            //Arrange
            var provider = OAuthProvider.Facebook;

            //Act
            Assert.Throws<ArgumentNullException>(() => _sut.UpdateAccessToken(_user.Id, provider, token));

            //Assert
            _uow.Verify(x => x.OAuthAccountRepository.GetUserAccount(_user.Id, provider), Times.Never());
        }

        [Test]
        public void ThrowIfOAuthAccountWasNotFound_OnUpdateAccessToken()
        {
            //Arrange
            var provider = OAuthProvider.Facebook;
            var token = "123";
            _uow.Setup(x => x.OAuthAccountRepository.GetUserAccount(_user.Id, provider))
                    .Returns(() => null);

            //Act
            Assert.Throws<NotFoundException>(() => _sut.UpdateAccessToken(_user.Id, provider, token));

            //Assert
            _uow.Verify(x => x.OAuthAccountRepository.GetUserAccount(_user.Id, provider), Times.Once());
        }

        [Test]
        public void SetNewAccessTokenAndSave_OnUpdateAccessToken()
        {
            //Arrange
            var provider = OAuthProvider.Facebook;
            var token = "new token";
            var providerUserId = 444L;

            var oauthAccount = new OAuthAccount
                               {
                                   AccessToken = "old token",
                                   Provider = provider,
                                   ProviderUserId = providerUserId,
                                   UserId = _user.Id
                               };

            _uow.Setup(x => x.OAuthAccountRepository.GetUserAccount(_user.Id, provider))
                    .Returns(oauthAccount);

            //Act
            _sut.UpdateAccessToken(_user.Id, provider, token);

            //Assert
            Assert.AreEqual(token, oauthAccount.AccessToken);

            _uow.Verify(x => x.OAuthAccountRepository.GetUserAccount(_user.Id, provider), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Once());
        }

        #endregion
    }
}