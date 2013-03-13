using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Infrastructure.Services;

namespace Zazz.UnitTests.Infrastructure.Services
{
    [TestFixture]
    public class UserServiceShould
    {
        private Mock<IUoW> _uow;
        private UserService _sut;

        [SetUp]
        public void Init()
        {
            _uow = new Mock<IUoW>();
            _sut = new UserService(_uow.Object);
        }

        [Test]
        public void CallGetIdByUserName_OnGetUserId()
        {
            //Arrange
            var username = "soroush";
            var id = 12;
            _uow.Setup(x => x.UserRepository.GetIdByUsername(username))
                .Returns(id);

            //Act
            var result = _sut.GetUserId(username);

            //Assert
            Assert.AreEqual(id, result);
            _uow.Verify(x => x.UserRepository.GetIdByUsername(username), Times.Once());
        }

        [Test]
        public async Task CallGetByUsername_OnGetUser()
        {
            //Arrange
            var username = "soroush";
            var user = new User();
            _uow.Setup(x => x.UserRepository.GetByUsernameAsync(username))
                .Returns(() => Task.Run(() => user));


            //Act
            var result = await _sut.GetUserAsync(username);

            //Assert
            Assert.AreSame(user, result);
            _uow.Verify(x => x.UserRepository.GetByUsernameAsync(username), Times.Once());

        }

        [Test]
        public void ReturnGetUserAccountTypeFromRepo_OnGetUserAccountType()
        {
            //Arrange
            var id = 123;
            var accountType = AccountType.User;
            _uow.Setup(x => x.UserRepository.GetUserAccountType(id))
                .Returns(accountType);

            //Act
            var result = _sut.GetUserAccountType(id);

            //Assert
            Assert.AreEqual(accountType, result);
            _uow.Verify(x => x.UserRepository.GetUserAccountType(id), Times.Once());
        }


    }
}