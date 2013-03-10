using Moq;
using NUnit.Framework;
using Zazz.Core.Interfaces;
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


    }
}