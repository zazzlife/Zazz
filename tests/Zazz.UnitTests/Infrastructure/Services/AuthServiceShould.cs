using System;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Zazz.Core.Exceptions;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Services;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;
using Zazz.Infrastructure.Helpers;
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
        private byte[] _token;
        private Mock<IFacebookService> _fbService;

        [SetUp]
        public void Init()
        {
            _token = new byte[] {44, 44, 55, 66};
            _pass = "pass";
            _passBuffer = new byte[] { 1, 2, 3, 4, 5, 6 };
            _iv = new byte[] { 1, 2, 3, 3, 4, 5 };
            _user = new User
                    {
                        Id = 22,
                        Username = "username",
                        Email = "test@test.com",
                        Password = "Password",
                    };


            _mockRepo = new MockRepository(MockBehavior.Strict);
            
            _uow = _mockRepo.Create<IUoW>();
            _cryptoService = _mockRepo.Create<ICryptoService>();
            _fbService = _mockRepo.Create<IFacebookService>();

            _sut = new AuthService(_uow.Object, _cryptoService.Object, _fbService.Object);
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
            var pass = "invalidPassword";
            _uow.Setup(x => x.UserRepository.GetByUsername(_user.Username, false, false, false, false))
                    .Returns(_user);

            _cryptoService.Setup(x => x.GeneratePasswordHash(pass))
                       .Returns("valid pass");

            //Act
            Assert.Throws<InvalidPasswordException>(() => _sut.Login(_user.Username, pass));

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

            _cryptoService.Setup(x => x.GeneratePasswordHash(_pass))
                       .Returns(_user.Password);

            _uow.Setup(x => x.SaveChanges());

            //Act
            _sut.Login(_user.Username, _pass);

            //Assert
            Assert.IsTrue(_user.LastActivity <= DateTime.UtcNow);
            _mockRepo.VerifyAll();
        }

        #endregion

        #region Register

        [TestCase("joe")] // should fail
        [TestCase("joe@home")] // should fail
        [TestCase("a@b.c")] // should fail because .c is only one character but must be 2-4 characters
        [TestCase("joe-bob[at]home.com")] // should fail because [at] is not valid
        [TestCase("joe@his.home.place")] // should fail because place is 5 characters but must be 2-4 characters
        [TestCase("joe.@bob.com")] // should fail because there is a dot at the end of the local-part
        [TestCase(".joe@bob.com")] // should fail because there is a dot at the beginning of the local-part
        [TestCase("john..doe@bob.com")] // should fail because there are two dots in the local-part
        [TestCase("john.doe@bob..com")] // should fail because there are two dots in the domain
        [TestCase("joe<>bob@bob.com")] // should fail because <> are not valid
        [TestCase("joe@his.home.com.")] // should fail because it can't end with a period
        [TestCase("a@10.1.100.1a")] // Should fail because of the extra character
        [TestCase("joe<>bob@bob.com\n")] // should fail because it end with \n
        [TestCase("joe<>bob@bob.com\r")] // should fail because it ends with \r
        public void ThrowIfEmailIsInvalid_OnRegister(string email)
        {
            //Arrange
            _user.Email = email;

            //Act & Assert
            Assert.Throws<InvalidEmailException>(() => _sut.Register(_user, _pass, false));
            _mockRepo.VerifyAll();
        }

        [Test]
        public void ThrowIfPasswordIsMoreThan20Char_OnRegister()
        {
            //Arrange
            _pass = "123456789012345678901";
            Assert.IsTrue(_pass.Length > 20);

            //Act & Assert
            Assert.Throws<PasswordTooLongException>(() => _sut.Register(_user, _pass, false));
            _mockRepo.VerifyAll();
        }

        [Test]
        public void ThrowIfUsernameExists_OnRegister()
        {
            //Arrange
            _uow.Setup(x => x.UserRepository.ExistsByUsername(_user.Username))
                    .Returns(true);

            //Act
            Assert.Throws<UsernameExistsException>(() => _sut.Register(_user, _pass, false));

            //Assert
            _mockRepo.VerifyAll();
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
            Assert.Throws<EmailExistsException>(() => _sut.Register(_user, _pass, false));

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void HashThePassword_OnRegister()
        {
            //Arrange
            _uow.Setup(x => x.UserRepository.ExistsByUsername(_user.Username))
                    .Returns(false);
            _uow.Setup(x => x.UserRepository.ExistsByEmail(_user.Email))
                    .Returns(false);

            var passwordHash = "something!";

            _cryptoService.Setup(x => x.GeneratePasswordHash(_pass))
                       .Returns(passwordHash);

            _uow.Setup(x => x.UserRepository.InsertGraph(It.IsAny<User>()));
            _uow.Setup(x => x.SaveChanges());

            //Act
            _sut.Register(_user, _pass, false);

            //Assert
            _mockRepo.VerifyAll();
            CollectionAssert.AreEqual(passwordHash, _user.Password);
        }

        [Test]
        public void GenerateValidationTokenIfRequested_OnRegister()
        {
            //Arrange
            _uow.Setup(x => x.UserRepository.ExistsByUsername(_user.Username))
                    .Returns(false);
            _uow.Setup(x => x.UserRepository.ExistsByEmail(_user.Email))
                    .Returns(false);

            var passwordHash = "passwordHash";
            _cryptoService.Setup(x => x.GeneratePasswordHash(_pass))
                       .Returns(passwordHash);

            _cryptoService.Setup(x => x.GenerateKey(It.IsAny<int>(), It.IsAny<bool>()))
                          .Returns(_token);

            _uow.Setup(x => x.UserRepository.InsertGraph(It.IsAny<User>()));
            _uow.Setup(x => x.SaveChanges());

            //Act
            _sut.Register(_user, _pass, true);

            //Assert
            _mockRepo.VerifyAll();

            Assert.IsNotNull(_user.UserValidationToken);
            Assert.AreEqual(DateTime.UtcNow.AddYears(1).Date, _user.UserValidationToken.ExpirationTime.Date);
            Assert.IsNotNull(_user.UserValidationToken.Token);
        }

        #endregion

        #region Generate Reset Password Token

        [Test]
        public void ThrowEmailNotExists_WhenEmailNotExists_OnGenerateResetToken()
        {
            //Arrange
            var email = "email";
            _uow.Setup(x => x.UserRepository.GetIdByEmail(email))
                    .Returns(0);

            //Act
            Assert.Throws<NotFoundException>(() => _sut.GenerateResetPasswordToken(email));

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void InsertNewTokenIfNotExists_OnGenerateResetToken()
        {
            //Arrange
            var userId = 12;
            var email = "email";
            _uow.Setup(x => x.UserRepository.GetIdByEmail(email))
                    .Returns(userId);
            _uow.Setup(x => x.ValidationTokenRepository.GetById(userId))
                    .Returns(() => null);

            _uow.Setup(x => x.ValidationTokenRepository.InsertGraph(It.Is<UserValidationToken>(t => t.Id == userId)));
            _uow.Setup(x => x.SaveChanges());

            _cryptoService.Setup(x => x.GenerateKey(It.IsAny<int>(), It.IsAny<bool>()))
                          .Returns(_token);

            //Act
            var result = _sut.GenerateResetPasswordToken(email);

            //Assert
            Assert.AreEqual(DateTime.UtcNow.AddDays(1).Date, result.ExpirationTime.Date);
            Assert.IsNotNull(result.Token);
            Assert.AreEqual(userId, result.Id);
            _mockRepo.VerifyAll();
        }

        [Test]
        public void UpdateTokenIfExists_OnGenerateResetToken()
        {
            //Arrange
            var userId = 12;
            var email = "email";
            var oldTokenExpirationTime = DateTime.UtcNow.AddDays(-1);
            var oldToken = new byte[] {99, 66};

            var token = new UserValidationToken
                        {
                            Id = userId,
                            ExpirationTime = oldTokenExpirationTime,
                            Token = oldToken,
                        };

            _uow.Setup(x => x.UserRepository.GetIdByEmail(email))
                    .Returns(userId);
            _uow.Setup(x => x.ValidationTokenRepository.GetById(userId))
                    .Returns(token);

            _cryptoService.Setup(x => x.GenerateKey(It.IsAny<int>(), It.IsAny<bool>()))
                          .Returns(_token);

            _uow.Setup(x => x.SaveChanges());

            //Act
            var result = _sut.GenerateResetPasswordToken(email);

            //Assert
            Assert.AreEqual(DateTime.UtcNow.AddDays(1).Date, result.ExpirationTime.Date);
            CollectionAssert.AreNotEqual(oldToken, result.Token);
            Assert.AreEqual(userId, result.Id);
            _mockRepo.VerifyAll();
        }

        #endregion

        #region Is Token Valid

        [Test]
        public void RetrunTrueWhenTokenIsValid_OnIsTokenValid()
        {
            //Arrange
            var token = new UserValidationToken
                        {
                            Id = 12,
                            Token = _token,
                            ExpirationTime = DateTime.UtcNow.AddDays(1)
                        };

            _uow.Setup(x => x.ValidationTokenRepository.GetById(token.Id))
                    .Returns(() => token);

            var base64Token = Base64Helper.Base64UrlEncode(_token);

            //Act
            var result = _sut.IsTokenValid(token.Id, base64Token);

            //Assert
            Assert.IsTrue(result);
            _mockRepo.VerifyAll();
        }

        [Test]
        public void RetrunFalseWhenTokenIsNotValid_OnIsTokenValid()
        {
            //Arrange
            var token = new UserValidationToken
                        {
                            Id = 12,
                            Token = _token,
                            ExpirationTime = DateTime.UtcNow.AddDays(1)
                        };
            _uow.Setup(x => x.ValidationTokenRepository.GetById(token.Id))
                    .Returns(() => token);

            var base64Token = Base64Helper.Base64UrlEncode(new byte[] {1, 2, 3});

            //Act
            var result = _sut.IsTokenValid(token.Id, base64Token);

            //Assert
            Assert.IsFalse(result);
            _mockRepo.VerifyAll();

        }

        [Test]
        public void ThrowExpiredExceptionWhenTokenIsExpired_OnIsTokenValid()
        {
            //Arrange
            var token = new UserValidationToken
                        {
                            Id = 12,
                            Token = _token,
                            ExpirationTime = DateTime.UtcNow.AddDays(-1)
                        };

            _uow.Setup(x => x.ValidationTokenRepository.GetById(token.Id))
                    .Returns(() => token);

            var base64Token = Base64Helper.Base64UrlEncode(_token);
            //Act
            Assert.Throws<TokenExpiredException>(() => _sut.IsTokenValid(token.Id, base64Token));

            //Assert
            _mockRepo.VerifyAll();
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
            Assert.Throws<PasswordTooLongException>(
                () => _sut.ResetPassword(_user.Id, Base64Helper.Base64UrlEncode(_token), _pass));

            _mockRepo.VerifyAll();
        }

        [Test]
        public void ThrowInvalidTokenExceptionWhenTokenIsNotValid_OnResetPassword()
        {
            //Arrange
            var token = new UserValidationToken { Id = _user.Id, Token = _token, ExpirationTime = DateTime.UtcNow.AddDays(1) };
            _user.UserValidationToken = token;
            _uow.Setup(x => x.UserRepository.GetById(_user.Id, false, false, false, false))
                .Returns(_user);

            var base64 = Base64Helper.Base64UrlEncode(new byte[] {1});

            //Act & Assert
            Assert.Throws<InvalidTokenException>(() =>  _sut.ResetPassword(_user.Id, base64, ""));
            _mockRepo.VerifyAll();
        }

        [Test]
        public void HashTheNewPassword_OnResetPassword()
        {
            //Arrange
            var newPass = "newpass";
            var newPassHash = "newPassHash";

            var token = new UserValidationToken { Id = _user.Id, Token = _token, ExpirationTime = DateTime.UtcNow.AddDays(1) };
            _user.UserValidationToken = token;
            _cryptoService.Setup(x => x.GeneratePasswordHash(newPass))
                       .Returns(newPassHash);

            _uow.Setup(x => x.UserRepository.GetById(_user.Id, false, false, false, false))
                    .Returns(_user);
            _uow.Setup(x => x.ValidationTokenRepository.Remove(_user.Id));
            _uow.Setup(x => x.SaveChanges());

            //Act
            _sut.ResetPassword(_user.Id, Base64Helper.Base64UrlEncode(_token), newPass);

            //Assert
            CollectionAssert.AreEqual(newPassHash, _user.Password);
            _mockRepo.VerifyAll();
        }

        [Test]
        public void RemoveTokenOnSuccessfulReset_OnResetPassword()
        {
            //Arrange
            var newPass = "newpass";
            var passwordHash = "passwordHash";

            var token = new UserValidationToken { Id = _user.Id, Token = _token, ExpirationTime = DateTime.UtcNow.AddDays(1) };
            _user.UserValidationToken = token;
            _cryptoService.Setup(x => x.GeneratePasswordHash(newPass))
                       .Returns(passwordHash);

            _uow.Setup(x => x.UserRepository.GetById(_user.Id, false, false, false, false))
                    .Returns(_user);
            _uow.Setup(x => x.ValidationTokenRepository.Remove(_user.Id));
            _uow.Setup(x => x.SaveChanges());

            //Act
            _sut.ResetPassword(_user.Id, Base64Helper.Base64UrlEncode(_token), newPass);

            //Assert
            _mockRepo.VerifyAll();
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
            _mockRepo.VerifyAll();
        }

        [Test]
        public void ThrowInvalidPasswordIfCurrentPasswordIsNotCorrect_OnChangePassword()
        {
            //Arrange
            var pass = "pass";
            var newPass = "newPass";
            var invalidPass = "invalid";
            var invalidHash = "invalidHash";

            _uow.Setup(x => x.UserRepository.GetById(_user.Id, false, false, false, false))
                    .Returns(_user);
            _cryptoService.Setup(x => x.GeneratePasswordHash(invalidPass))
                       .Returns(invalidHash);

            //Act
            Assert.Throws<InvalidPasswordException>(() => _sut.ChangePassword(_user.Id, invalidPass, newPass));

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void HashTheNewPasswordIfEverythingIsFine_OnChangePassword()
        {
            //Arrange
            var pass = "pass";
            var passHash = "passHash";
            var newPass = "newPass";
            var newPassHash = newPass + "hash";

            _uow.Setup(x => x.UserRepository.GetById(_user.Id, false, false, false, false))
                    .Returns(_user);
            _cryptoService.Setup(x => x.GeneratePasswordHash(pass))
                       .Returns(_user.Password);
            _cryptoService.Setup(x => x.GeneratePasswordHash(newPass))
                       .Returns(newPassHash);

            _uow.Setup(x => x.SaveChanges());

            //Act
            _sut.ChangePassword(_user.Id, pass, newPass);

            //Assert
            Assert.AreEqual(newPassHash, _user.Password);
            _mockRepo.VerifyAll();
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
            var oauthAccount = new LinkedAccount {ProviderUserId = providerId, Provider = provider, User = new User()};

            _uow.Setup(x => x.LinkedAccountRepository.GetOAuthAccountByProviderId(providerId, provider))
                    .Returns(oauthAccount);

            //Act
            var result = _sut.GetOAuthUser(oauthAccount, email);

            //Assert
            Assert.IsNotNull(result);
            _mockRepo.VerifyAll();
        }

        [Test]
        public void CheckForUserWithEmailIFOAuthAccountNotExists_OnGetOAuthUserAsync()
        {
            //Arrange
            var providerId = 1234L;
            var provider = OAuthProvider.Facebook;
            var email = "email";
            var user = new User { Email = email };
            var oauthAccount = new LinkedAccount { ProviderUserId = providerId, Provider = provider };

            _uow.Setup(x => x.LinkedAccountRepository.GetOAuthAccountByProviderId(providerId, provider))
                    .Returns(() => null);
            _uow.Setup(x => x.UserRepository.GetByEmail(email))
                    .Returns(user);

            //Act
            var result = _sut.GetOAuthUser(oauthAccount, email);

            //Assert
            Assert.IsNotNull(result);
            _mockRepo.VerifyAll();
        }

        [Test]
        public void ReturnNullIfNotUserNorOAuthAccountExists_OnGetOAuthUserAsync()
        {
            //Arrange
            var providerId = 1234L;
            var provider = OAuthProvider.Facebook;
            var email = "email";
            var user = new User { Email = email };
            var oauthAccount = new LinkedAccount { ProviderUserId = providerId, Provider = provider };

            _uow.Setup(x => x.LinkedAccountRepository.GetOAuthAccountByProviderId(providerId, provider))
                    .Returns(() => null);
            _uow.Setup(x => x.UserRepository.GetByEmail(email))
                    .Returns(() => null);

            //Act
            var result = _sut.GetOAuthUser(oauthAccount, email);

            //Assert
            _mockRepo.VerifyAll();
            Assert.IsNull(result);
        }

        [Test]
        public void NotAddNewOAuthAccountIfItExists_OnAddOrUpdateOAuthAccount()
        {
            //Arrange
            var providerId = 1234L;
            var provider = OAuthProvider.Facebook;

            var oauthAccount = new LinkedAccount { ProviderUserId = providerId, Provider = provider };
            _uow.Setup(x => x.LinkedAccountRepository.GetOAuthAccountByProviderId(providerId, provider))
                    .Returns(new LinkedAccount
                             {
                                 User = new User {AccountType = AccountType.User}
                             });

            _uow.Setup(x => x.SaveChanges());

            //Act
            _sut.AddOrUpdateOAuthAccount(oauthAccount);

            //Assert

           _mockRepo.VerifyAll();
        }

        [Test]
        public void AddNewOAuthAccountIfItNotExists_OnAddOrUpdateOAuthAccount()
        {
            //Arrange
            var providerId = 1234L;
            var provider = OAuthProvider.Facebook;

            var oauthAccount = new LinkedAccount { ProviderUserId = providerId, Provider = provider, UserId = 23};

            _uow.Setup(x => x.LinkedAccountRepository.GetOAuthAccountByProviderId(providerId, provider))
                    .Returns(() => null);

            _uow.Setup(x => x.LinkedAccountRepository
                             .InsertGraph(It.Is<LinkedAccount>(a => a.AccessToken == oauthAccount.AccessToken &&
                                                                   a.Provider == provider &&
                                                                   a.ProviderUserId == providerId &&
                                                                   a.UserId == oauthAccount.UserId)));

            _uow.Setup(x => x.SaveChanges());

            //Act
            _sut.AddOrUpdateOAuthAccount(oauthAccount);

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void UpdateAccessTokenIfOAuthAccountExists_OnAddOrUpdateOAuthAccount()
        {
            //Arrange
            var providerId = 1234L;
            var provider = OAuthProvider.Facebook;

            var oldOAuthAccount = new LinkedAccount
                                  {
                                      ProviderUserId = providerId,
                                      Provider = provider,
                                      UserId = 23,
                                      AccessToken = "old token",
                                      User = new User
                                             {
                                                 AccountType = AccountType.User
                                             }
                                  };

            var newOauthAccount = new LinkedAccount
                                  {
                                      ProviderUserId = providerId,
                                      Provider = provider,
                                      UserId = 23,
                                      AccessToken = "new token"
                                  };


            _uow.Setup(x => x.LinkedAccountRepository.GetOAuthAccountByProviderId(providerId, provider))
                    .Returns(oldOAuthAccount);


            _uow.Setup(x => x.SaveChanges());

            //Act
            _sut.AddOrUpdateOAuthAccount(newOauthAccount);

            //Assert
            Assert.AreEqual(newOauthAccount.AccessToken, oldOAuthAccount.AccessToken);

            _mockRepo.VerifyAll();
        }

        [Test]
        public void UpdatePagesAccessTokenIfAccountTypeIsClub_OnAddOrUpdateOAuthAccount()
        {
            //Arrange
            var providerId = 1234L;
            var provider = OAuthProvider.Facebook;

            var oldOAuthAccount = new LinkedAccount
            {
                ProviderUserId = providerId,
                Provider = provider,
                UserId = 23,
                AccessToken = "old token",
                User = new User
                {
                    AccountType = AccountType.Club
                }
            };

            var newOauthAccount = new LinkedAccount
            {
                ProviderUserId = providerId,
                Provider = provider,
                UserId = 23,
                AccessToken = "new token"
            };


            _uow.Setup(x => x.LinkedAccountRepository.GetOAuthAccountByProviderId(providerId, provider))
                    .Returns(oldOAuthAccount);

            _fbService.Setup(x => x.UpdatePagesAccessToken(oldOAuthAccount.UserId, newOauthAccount.AccessToken));

            _uow.Setup(x => x.SaveChanges());

            //Act
            _sut.AddOrUpdateOAuthAccount(newOauthAccount);

            //Assert
            Assert.AreEqual(newOauthAccount.AccessToken, oldOAuthAccount.AccessToken);

            _mockRepo.VerifyAll();
        }

        #endregion
    }
}