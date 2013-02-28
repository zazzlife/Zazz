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
        public async Task ThrowUserNotExist_WhenUserNotExists()
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
            _sut.RegisterAsync(user).Wait();

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
                await _sut.RegisterAsync(user);
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
            _sut.RegisterAsync(user).Wait();

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
                await _sut.RegisterAsync(user);
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

            var user = new User { Email = "email", Username = "username", Password = clearPass};
            
            _uowMock.Setup(x => x.UserRepository.ExistsByUsernameAsync(user.Username))
                    .Returns(() => Task.Run(() => false));
            _uowMock.Setup(x => x.UserRepository.ExistsByEmailAsync(user.Email))
                    .Returns(() => Task.Run(() => false));
            _cryptoMock.Setup(x => x.GeneratePasswordHash(user.Password))
                       .Returns(hashedPass);

            //Act
            _sut.RegisterAsync(user).Wait();

            //Assert
            _cryptoMock.Verify(x=>x.GeneratePasswordHash(clearPass), Times.Once());
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
            _sut.RegisterAsync(user).Wait();

            //Assert
            Assert.IsTrue(user.JoinedDate <= DateTime.UtcNow);
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
            _sut.RegisterAsync(user).Wait();

            //Assert
            _uowMock.Verify(x => x.UserRepository.InsertGraph(user), Times.Once());
            _uowMock.Verify(x => x.SaveAsync(), Times.Once());
        }


    }
}