using System;
using Moq;
using NUnit.Framework;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;
using Zazz.Infrastructure.Services;

namespace Zazz.UnitTests.Infrastructure.Services
{
    [TestFixture]
    public class WeeklyServiceShould
    {
        private MockRepository _mockRepo;
        private Mock<IUoW> _uow;
        private WeeklyService _sut;

        [SetUp]
        public void Init()
        {
            _mockRepo = new MockRepository(MockBehavior.Strict);
            _uow = _mockRepo.Create<IUoW>();
            _sut = new WeeklyService(_uow.Object);
        }

        [Test]
        public void ThrowIfUserIsNotClubAdmin_OnCreateWeekly()
        {
            //Arrange
            var user = new User
                       {
                           Id = 2,
                           AccountType = AccountType.User
                       };

            _uow.Setup(x => x.UserRepository.GetById(user.Id))
                .Returns(user);

            var weekly = new Weekly
                         {
                             UserId = user.Id
                         };

            //Act & Assert
            Assert.Throws<InvalidOperationException>(() => _sut.CreateWeekly(weekly));
            _mockRepo.VerifyAll();
        }

        [Test]
        public void InsertANewRecord_OnCreateWeekly()
        {
            //Arrange
            var user = new User
            {
                Id = 2,
                AccountType = AccountType.ClubAdmin
            };

            _uow.Setup(x => x.UserRepository.GetById(user.Id))
                .Returns(user);
            _uow.Setup(x => x.SaveChanges());

            var weekly = new Weekly
                         {
                             UserId = user.Id
                         };
            Assert.AreEqual(0, user.Weeklies.Count);
            //Act
            _sut.CreateWeekly(weekly);

            //Assert
            Assert.AreEqual(1, user.Weeklies.Count);
        }
    }
}