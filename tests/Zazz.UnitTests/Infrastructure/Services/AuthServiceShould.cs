using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Zazz.Core.Exceptions;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
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

            _uowMock.Setup(x => x.SaveAsync())
                    .Returns(Task.Run(() => { }));
        }

        #region Login
        [Test]
        public void GetHashOfPassword_OnLogin()
        {
            //Arrange
            var pass = "password";
            _uowMock.Setup(x => x.UserRepository.GetByUsernameAsync(It.IsAny<string>()))
                    .Returns(() => Task.Run(() => new User { Password = pass }));
            _cryptoMock.Setup(x => x.GeneratePasswordHash(It.IsAny<string>()))
                       .Returns(pass);


            //Act
            _sut.LoginAsync("user", pass).Wait();

            //Assert
            _cryptoMock.Verify(x => x.GeneratePasswordHash(pass), Times.Once());
        }

        [Test]
        public void GetUserPasswordFromDB_OnLogin()
        {
            //Arrange
            var username = "username";
            var pass = "pass";
            _uowMock.Setup(x => x.UserRepository.GetByUsernameAsync(username))
                    .Returns(() => Task.Run(() => new User { Password = pass }));
            _cryptoMock.Setup(x => x.GeneratePasswordHash(It.IsAny<string>()))
                       .Returns(pass);
            //Act
            _sut.LoginAsync(username, pass).Wait();

            //Assert
            _uowMock.Verify(x => x.UserRepository.GetByUsernameAsync(username), Times.Once());
        }

        [Test]
        public async Task ThrowUserNotExist_WhenUserNotExists_OnLogin()
        {
            //Arrange
            _uowMock.Setup(x => x.UserRepository.GetByUsernameAsync(It.IsAny<string>()))
                    .Returns(() => Task.Factory.StartNew<User>(() => null));

            //Act
            try
            {
                await _sut.LoginAsync("user", "pass");
                Assert.Fail("Expected exception wasn't thrown");
            }
            catch (UserNotExistsException e)
            {
                //Assert
                Assert.IsInstanceOf<UserNotExistsException>(e);
            }
        }

        [Test]
        public async Task ThrowInvalidPassword_WhenPasswordsDontMatch_OnLogin()
        {
            //Arrange
            _uowMock.Setup(x => x.UserRepository.GetByUsernameAsync(It.IsAny<string>()))
                    .Returns(() => Task.Run(() => new User { Password = "password" }));
            _cryptoMock.Setup(x => x.GeneratePasswordHash(It.IsAny<string>()))
                       .Returns("invalidPass");
            //Act
            try
            {
                await _sut.LoginAsync("user", "invalidPassword");
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
            _uowMock.Setup(x => x.UserRepository.GetByUsernameAsync(It.IsAny<string>()))
                    .Returns(() => Task.Run(() => user));
            _cryptoMock.Setup(x => x.GeneratePasswordHash(It.IsAny<string>()))
                       .Returns(pass);

            //Act
            _sut.LoginAsync("user", pass).Wait();

            //Assert
            Assert.IsTrue(user.LastActivity <= DateTime.UtcNow);
        }

        [Test]
        public void CallSaveChanges_WhenEverythingIsOk_OnLogin()
        {
            //Arrange
            var pass = "pass";
            var user = new User { Password = pass, LastActivity = DateTime.MaxValue };
            _uowMock.Setup(x => x.UserRepository.GetByUsernameAsync(It.IsAny<string>()))
                    .Returns(() => Task.Run(() => user));

            _cryptoMock.Setup(x => x.GeneratePasswordHash(It.IsAny<string>()))
                       .Returns(pass);

            //Act
            _sut.LoginAsync("user", pass).Wait();

            //Assert
            _uowMock.Verify(x => x.SaveAsync(), Times.Once());
        }
        #endregion

        #region Register
        [Test]
        public void CheckForExistingUser_OnRegister()
        {
            //Arrange
            var user = new User { Email = "email", Username = "username", Password = "pass" };
            _uowMock.Setup(x => x.UserRepository.ExistsByUsernameAsync(user.Username))
                    .Returns(() => Task.Run(() => false));
            _uowMock.Setup(x => x.UserRepository.ExistsByEmailAsync(user.Email))
                    .Returns(() => Task.Run(() => false));

            //Act
            _sut.RegisterAsync(user, false).Wait();

            //Assert
            _uowMock.Verify(x => x.UserRepository.ExistsByUsernameAsync(user.Username), Times.Once());
        }

        [Test]
        public async Task ThrowIfUsernameExists_OnRegister()
        {
            //Arrange
            var user = new User { Email = "email", Username = "username", Password = "pass" };
            _uowMock.Setup(x => x.UserRepository.ExistsByUsernameAsync(user.Username))
                    .Returns(() => Task.Run(() => true));
            _uowMock.Setup(x => x.UserRepository.ExistsByEmailAsync(user.Email))
                    .Returns(() => Task.Run(() => false));

            //Act
            try
            {
                await _sut.RegisterAsync(user, false);
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
            var user = new User { Email = "email", Username = "username", Password = "pass" };
            _uowMock.Setup(x => x.UserRepository.ExistsByUsernameAsync(user.Username))
                    .Returns(() => Task.Run(() => false));
            _uowMock.Setup(x => x.UserRepository.ExistsByEmailAsync(user.Email))
                    .Returns(() => Task.Run(() => false));

            //Act
            _sut.RegisterAsync(user, false).Wait();

            //Assert
            _uowMock.Verify(x => x.UserRepository.ExistsByEmailAsync(user.Email), Times.Once());

        }

        [Test]
        public async Task ThrowIfEmailExists_OnRegister()
        {
            //Arrange
            var user = new User { Email = "email", Username = "username", Password = "pass" };
            _uowMock.Setup(x => x.UserRepository.ExistsByUsernameAsync(user.Username))
                    .Returns(() => Task.Run(() => false));
            _uowMock.Setup(x => x.UserRepository.ExistsByEmailAsync(user.Email))
                    .Returns(() => Task.Run(() => true));

            //Act
            try
            {
                await _sut.RegisterAsync(user, false);
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

            var user = new User { Email = "email", Username = "username", Password = clearPass };

            _uowMock.Setup(x => x.UserRepository.ExistsByUsernameAsync(user.Username))
                    .Returns(() => Task.Run(() => false));
            _uowMock.Setup(x => x.UserRepository.ExistsByEmailAsync(user.Email))
                    .Returns(() => Task.Run(() => false));
            _cryptoMock.Setup(x => x.GeneratePasswordHash(user.Password))
                       .Returns(hashedPass);

            //Act
            _sut.RegisterAsync(user, false).Wait();

            //Assert
            _cryptoMock.Verify(x => x.GeneratePasswordHash(clearPass), Times.Once());
            Assert.AreEqual(user.Password, hashedPass);
        }

        [Test]
        public void AssignUTCDateTimeAsRegiterDate_OnRegister()
        {
            //Arrange
            var user = new User { Email = "email", Username = "username", Password = "pass", JoinedDate = DateTime.MaxValue };
            _uowMock.Setup(x => x.UserRepository.ExistsByUsernameAsync(user.Username))
                    .Returns(() => Task.Run(() => false));
            _uowMock.Setup(x => x.UserRepository.ExistsByEmailAsync(user.Email))
                    .Returns(() => Task.Run(() => false));
            _cryptoMock.Setup(x => x.GeneratePasswordHash(user.Password))
                       .Returns(user.Password);

            //Act
            _sut.RegisterAsync(user, false).Wait();

            //Assert
            Assert.IsTrue(user.JoinedDate <= DateTime.UtcNow);
        }

        [Test]
        public void GenerateValidationTokenIfRequested_OnRegister()
        {
            //Arrange
            var user = new User { Email = "email", Username = "username", Password = "pass", JoinedDate = DateTime.MaxValue };
            _uowMock.Setup(x => x.UserRepository.ExistsByUsernameAsync(user.Username))
                    .Returns(() => Task.Run(() => false));
            _uowMock.Setup(x => x.UserRepository.ExistsByEmailAsync(user.Email))
                    .Returns(() => Task.Run(() => false));
            _cryptoMock.Setup(x => x.GeneratePasswordHash(user.Password))
                       .Returns(user.Password);


            //Act
            _sut.RegisterAsync(user, true).Wait();

            //Assert
            Assert.IsNotNull(user.ValidationToken);
            Assert.AreEqual(DateTime.UtcNow.AddYears(1).Date, user.ValidationToken.ExpirationDate.Date);
            Assert.IsNotNull(user.ValidationToken.Token);
        }



        [Test]
        public void SaveUserWhenEverythingIsOk_OnRegister()
        {
            //Arrange
            var user = new User { Email = "email", Username = "username", Password = "pass", JoinedDate = DateTime.MaxValue };
            _uowMock.Setup(x => x.UserRepository.ExistsByUsernameAsync(user.Username))
                    .Returns(() => Task.Run(() => false));
            _uowMock.Setup(x => x.UserRepository.ExistsByEmailAsync(user.Email))
                    .Returns(() => Task.Run(() => false));
            _cryptoMock.Setup(x => x.GeneratePasswordHash(user.Password))
                       .Returns(user.Password);

            //Act
            _sut.RegisterAsync(user, false).Wait();

            //Assert
            _uowMock.Verify(x => x.UserRepository.InsertGraph(user), Times.Once());
            _uowMock.Verify(x => x.SaveAsync(), Times.Once());
        }
        #endregion

        #region Generate Reset Password Token

        [Test]
        public void GetUserIdByEmail_OnGenerateResetToken()
        {
            //Arrange
            var email = "email";
            _uowMock.Setup(x => x.UserRepository.GetIdByEmailAsync(email))
                    .Returns(() => Task.Run(() => 12));
            _uowMock.Setup(x => x.ValidationTokenRepository.GetByIdAsync(It.IsAny<int>()))
                    .Returns(() => Task.Factory.StartNew<ValidationToken>(() => null));

            //Act
            _sut.GenerateResetPasswordTokenAsync(email).Wait();

            //Assert
            _uowMock.Verify(x => x.UserRepository.GetIdByEmailAsync(email), Times.Once());
        }

        [Test]
        public async Task ThrowEmailNotExists_WhenEmailNotExists_OnGenerateResetToken()
        {
            //Arrange
            var email = "email";
            _uowMock.Setup(x => x.UserRepository.GetIdByEmailAsync(email))
                    .Returns(() => Task.Run(() => 0));

            try
            {
                //Act
                await _sut.GenerateResetPasswordTokenAsync(email);
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
            _uowMock.Setup(x => x.UserRepository.GetIdByEmailAsync(email))
                    .Returns(() => Task.Run(() => userId));
            _uowMock.Setup(x => x.ValidationTokenRepository.GetByIdAsync(userId))
                    .Returns(() => Task.Factory.StartNew<ValidationToken>(() => null));

            //Act
            var result = _sut.GenerateResetPasswordTokenAsync(email).Result;

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
            _uowMock.Setup(x => x.UserRepository.GetIdByEmailAsync(email))
                    .Returns(() => Task.Run(() => userId));
            _uowMock.Setup(x => x.ValidationTokenRepository.GetByIdAsync(userId))
                    .Returns(() => Task.Run(() => token));
            _uowMock.Setup(x => x.ValidationTokenRepository.Remove(token));

            //Act
            var result = _sut.GenerateResetPasswordTokenAsync(email).Result;

            //Assert
            _uowMock.Verify(x => x.ValidationTokenRepository.GetByIdAsync(userId), Times.Once());
            _uowMock.Verify(x => x.ValidationTokenRepository.Remove(token), Times.Once());

        }

        [Test]
        public void NotCallRemoveWhenOldTokenNotExists_OnGenerateResetToken()
        {
            //Arrange
            var email = "email";
            var userId = 1;
            var token = new ValidationToken();
            _uowMock.Setup(x => x.UserRepository.GetIdByEmailAsync(email))
                    .Returns(() => Task.Run(() => userId));
            _uowMock.Setup(x => x.ValidationTokenRepository.GetByIdAsync(userId))
                    .Returns(() => Task.Factory.StartNew<ValidationToken>(() => null));
            _uowMock.Setup(x => x.ValidationTokenRepository.Remove(token));

            //Act
            var result = _sut.GenerateResetPasswordTokenAsync(email).Result;

            //Assert
            _uowMock.Verify(x => x.ValidationTokenRepository.GetByIdAsync(userId), Times.Once());
            _uowMock.Verify(x => x.ValidationTokenRepository.Remove(token), Times.Never());
        }

        [Test]
        public void SaveWhenEverythingIsOk_OnGenerateResetToken()
        {
            //Arrange
            var email = "email";
            var userId = 1;
            var token = new ValidationToken();
            _uowMock.Setup(x => x.UserRepository.GetIdByEmailAsync(email))
                    .Returns(() => Task.Run(() => userId));
            _uowMock.Setup(x => x.ValidationTokenRepository.GetByIdAsync(userId))
                    .Returns(() => Task.Factory.StartNew<ValidationToken>(() => null));
            _uowMock.Setup(x => x.ValidationTokenRepository.Remove(token));

            //Act
            var result = _sut.GenerateResetPasswordTokenAsync(email).Result;

            //Assert
            _uowMock.Verify(x => x.ValidationTokenRepository.InsertGraph(result));
            _uowMock.Verify(x => x.SaveAsync(), Times.Once());
        }

        #endregion

        #region Is Token Valid

        [Test]
        public void RetrunTrueWhenTokenIsValid_OnIsTokenValid()
        {
            //Arrange
            var token = new ValidationToken { Id = 12, Token = Guid.NewGuid(), ExpirationDate = DateTime.UtcNow.AddDays(1) };
            _uowMock.Setup(x => x.ValidationTokenRepository.GetByIdAsync(token.Id))
                    .Returns(() => Task.Run(() => token));

            //Act
            var result = _sut.IsTokenValidAsync(token.Id, token.Token).Result;

            //Assert
            Assert.IsTrue(result);
            _uowMock.Verify(x => x.ValidationTokenRepository.GetByIdAsync(token.Id), Times.Once());
        }

        [Test]
        public void RetrunFalseWhenTokenIsNotValid_OnIsTokenValid()
        {
            //Arrange
            var token = new ValidationToken { Id = 12, Token = Guid.NewGuid(), ExpirationDate = DateTime.UtcNow.AddDays(1) };
            _uowMock.Setup(x => x.ValidationTokenRepository.GetByIdAsync(token.Id))
                    .Returns(() => Task.Run(() => token));

            //Act
            var result = _sut.IsTokenValidAsync(token.Id, Guid.NewGuid()).Result;

            //Assert
            Assert.IsFalse(result);
            _uowMock.Verify(x => x.ValidationTokenRepository.GetByIdAsync(token.Id), Times.Once());

        }

        [Test]
        public async Task ThrowExpiredExceptionWhenTokenIsExpired_OnIsTokenValid()
        {
            //Arrange
            var token = new ValidationToken { Id = 12, Token = Guid.NewGuid(), ExpirationDate = DateTime.UtcNow.AddDays(-1) };
            _uowMock.Setup(x => x.ValidationTokenRepository.GetByIdAsync(token.Id))
                    .Returns(() => Task.Run(() => token));

            //Act
            try
            {
                var result = await _sut.IsTokenValidAsync(token.Id, token.Token);
                Assert.Fail("Expected Exception wasn't thrown");
            }
            catch (TokenExpiredException e)
            {
                //Assert
                _uowMock.Verify(x => x.ValidationTokenRepository.GetByIdAsync(token.Id), Times.Once());
                Assert.IsInstanceOf<TokenExpiredException>(e);
            }
        }

        #endregion

        #region Reset Password

        [Test]
        public async Task ThrowInvalidTokenExceptionWhenTokenIsNotValid_OnResetPassword()
        {
            //Arrange
            var token = new ValidationToken { Id = 12, Token = Guid.NewGuid(), ExpirationDate = DateTime.UtcNow.AddDays(1) };
            _uowMock.Setup(x => x.ValidationTokenRepository.GetByIdAsync(token.Id))
                    .Returns(() => Task.Run(() => token));

            //Act
            try
            {
                await _sut.ResetPasswordAsync(token.Id, Guid.NewGuid(), "");
                Assert.Fail("Expected Exception wasn't thrown");
            }
            catch (InvalidTokenException e)
            {
                //Assert
                _uowMock.Verify(x => x.ValidationTokenRepository.GetByIdAsync(token.Id), Times.Once());
                Assert.IsInstanceOf<InvalidTokenException>(e);
            }
        }

        [Test]
        public async Task HashTheNewPassword_OnResetPassword()
        {
            //Arrange
            var newPass = "newpass";
            var newPassHash = "hash";
            var token = new ValidationToken { Id = 12, Token = Guid.NewGuid(), ExpirationDate = DateTime.UtcNow.AddDays(1) };
            _cryptoMock.Setup(x => x.GeneratePasswordHash(newPass))
                       .Returns(() => newPassHash);
            _uowMock.Setup(x => x.ValidationTokenRepository.GetByIdAsync(token.Id))
                    .Returns(() => Task.Run(() => token));
            _uowMock.Setup(x => x.UserRepository.GetByIdAsync(token.Id))
                    .Returns(() => Task.Run(() => new User()));

            //Act
            await _sut.ResetPasswordAsync(token.Id, token.Token, newPass);

            //Assert
            _cryptoMock.Verify(x => x.GeneratePasswordHash(newPass));
        }

        [Test]
        public async Task UpdateTheUserCorrectly_OnResetPassword()
        {
            //Arrange
            var newPass = "newpass";
            var newPassHash = "hash";
            var user = new User { Password = "pass" };
            var token = new ValidationToken { Id = 12, Token = Guid.NewGuid(), ExpirationDate = DateTime.UtcNow.AddDays(1) };

            _cryptoMock.Setup(x => x.GeneratePasswordHash(newPass))
                       .Returns(() => newPassHash);
            _uowMock.Setup(x => x.ValidationTokenRepository.GetByIdAsync(token.Id))
                    .Returns(() => Task.Run(() => token));

            _uowMock.Setup(x => x.UserRepository.GetByIdAsync(token.Id))
                    .Returns(() => Task.Run(() => user));
            //Act
            await _sut.ResetPasswordAsync(token.Id, token.Token, newPass);

            //Assert
            Assert.AreEqual(newPassHash, user.Password);
            _uowMock.Verify(x => x.SaveAsync(), Times.Once());
        }

        #endregion

        #region Change Password

        [Test]
        public async Task HashTheCurrentPasswordBeforeComparing_OnChangePassword()
        {
            //Arrange
            var userId = 12;
            var newPassword = "newPass";
            var newPassHash = "hashpass";
            var oldPassHash = "oldHash";
            var oldPass = "oldPass";
            var user = new User { Password = oldPassHash };

            _uowMock.Setup(x => x.UserRepository.GetByIdAsync(userId))
                    .Returns(() => Task.Run(() => user));
            _cryptoMock.Setup(x => x.GeneratePasswordHash(newPassword))
                       .Returns(newPassHash);
            _cryptoMock.Setup(x => x.GeneratePasswordHash(oldPass))
                       .Returns(oldPassHash);

            //Act
            await _sut.ChangePasswordAsync(userId, oldPass, newPassword);

            //Assert
            _cryptoMock.Verify(x => x.GeneratePasswordHash(oldPass), Times.Once());
        }

        [Test]
        public async Task ThrowInvalidPasswordIfCurrentPasswordIsNotCorrect_OnChangePassword()
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

            _uowMock.Setup(x => x.UserRepository.GetByIdAsync(userId))
                    .Returns(() => Task.Run(() => user));
            _cryptoMock.Setup(x => x.GeneratePasswordHash(newPassword))
                       .Returns(newPassHash);
            _cryptoMock.Setup(x => x.GeneratePasswordHash(invalidPass))
                       .Returns(invalidPassHash);

            //Act
            try
            {
                await _sut.ChangePasswordAsync(userId, invalidPass, newPassword);
                Assert.Fail("Expected Exception Wasn't thrown");
            }
            catch (InvalidPasswordException e)
            {
                //Assert
                Assert.IsInstanceOf<InvalidPasswordException>(e);
                _uowMock.Verify(x => x.UserRepository.GetByIdAsync(userId), Times.Once());
                _cryptoMock.Verify(x => x.GeneratePasswordHash(invalidPass), Times.Once());
            }
        }

        [Test]
        public async Task HashTheNewPasswordIfEverythingIsFine_OnChangePassword()
        {
            //Arrange
            var userId = 12;
            var newPassword = "newPass";
            var newPassHash = "hashpass";
            var oldPassHash = "oldHash";
            var oldPass = "oldPass";
            var user = new User { Password = oldPassHash };

            _uowMock.Setup(x => x.UserRepository.GetByIdAsync(userId))
                    .Returns(() => Task.Run(() => user));
            _cryptoMock.Setup(x => x.GeneratePasswordHash(newPassword))
                       .Returns(newPassHash);
            _cryptoMock.Setup(x => x.GeneratePasswordHash(oldPass))
                       .Returns(oldPassHash);

            //Act
            await _sut.ChangePasswordAsync(userId, oldPass, newPassword);

            //Assert
            _uowMock.Verify(x => x.UserRepository.GetByIdAsync(userId), Times.Once());
            _cryptoMock.Verify(x => x.GeneratePasswordHash(newPassword), Times.Once());
            _cryptoMock.Verify(x => x.GeneratePasswordHash(oldPass), Times.Once());
        }

        [Test] public async Task SaveUserCorrectlyWhenEverythingIsFine_OnChangePassword()
        {
            //Arrange
            var userId = 12;
            var newPassword = "newPass";
            var newPassHash = "hashpass";
            var oldPassHash = "oldHash";
            var oldPass = "oldPass";
            var user = new User { Password = oldPassHash };

            _uowMock.Setup(x => x.UserRepository.GetByIdAsync(userId))
                    .Returns(() => Task.Run(() => user));
            _cryptoMock.Setup(x => x.GeneratePasswordHash(newPassword))
                       .Returns(newPassHash);
            _cryptoMock.Setup(x => x.GeneratePasswordHash(oldPass))
                       .Returns(oldPassHash);

            //Act
            await _sut.ChangePasswordAsync(userId, oldPass, newPassword);

            //Assert
            
            Assert.AreEqual(newPassHash, user.Password);
            _uowMock.Verify(x => x.SaveAsync());
            _uowMock.Verify(x => x.UserRepository.GetByIdAsync(userId), Times.Once());
            _cryptoMock.Verify(x => x.GeneratePasswordHash(newPassword), Times.Once());
            _cryptoMock.Verify(x => x.GeneratePasswordHash(oldPass), Times.Once());
        }

        #endregion
    }
}