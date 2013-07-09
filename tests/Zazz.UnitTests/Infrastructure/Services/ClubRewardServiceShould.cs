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
    public class ClubRewardServiceShould
    {
        private MockRepository _mockRepo;
        private Mock<IUoW> _uow;
        private ClubRewardService _sut;

        [SetUp]
        public void Init()
        {
            _mockRepo = new MockRepository(MockBehavior.Strict);
            _uow = _mockRepo.Create<IUoW>();

            _sut = new ClubRewardService(_uow.Object);
        }

        [Test]
        public void ThrowIfScenarioAlreadyExists_OnAddRewardScenario()
        {
            //Arrange
            var scenario = new ClubPointRewardScenario
                                                         {
                                                             Amount = 12,
                                                             ClubId = 22,
                                                             Scenario = PointRewardScenario.QRCodeSan
                                                         };

            _uow.Setup(x => x.ClubPointRewardScenarioRepository.Exists(scenario.ClubId, scenario.Scenario))
                .Returns(true);

            //Act & Assert

            Assert.Throws<AlreadyExistsException>(() => _sut.AddRewardScenario(scenario));
        }


    }
}