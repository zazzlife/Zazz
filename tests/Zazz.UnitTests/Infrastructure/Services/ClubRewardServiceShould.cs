using System;
using System.Security;
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
            _mockRepo.VerifyAll();
        }

        [Test]
        public void SaveNewScenario_OnAddRewardScenario()
        {
            //Arrange
            var scenario = new ClubPointRewardScenario
                           {
                               Amount = 12,
                               ClubId = 22,
                               Scenario = PointRewardScenario.QRCodeSan
                           };

            _uow.Setup(x => x.ClubPointRewardScenarioRepository.Exists(scenario.ClubId, scenario.Scenario))
                .Returns(false);

            _uow.Setup(x => x.ClubPointRewardScenarioRepository.InsertGraph(scenario));
            _uow.Setup(x => x.SaveChanges());

            //Act
            _sut.AddRewardScenario(scenario);

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void ThrowIfScenarioNotExists_OnChangeRewardAmount()
        {
            //Arrange
            var scenario = new ClubPointRewardScenario
                           {
                               Id = 555,
                               Amount = 12,
                               ClubId = 22,
                               Scenario = PointRewardScenario.QRCodeSan
                           };

            var newAmount = 444;

            _uow.Setup(x => x.ClubPointRewardScenarioRepository.GetById(scenario.Id))
                .Returns(() => null);

            //Act
            Assert.Throws<NotFoundException>(() => _sut.ChangeRewardAmount(scenario.Id, scenario.ClubId, newAmount));

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void ThrowIfUserIsNotAllowedToChangeScenario_OnChangeRewardAmount()
        {
            //Arrange
            var scenario = new ClubPointRewardScenario
                           {
                               Id = 555,
                               Amount = 12,
                               ClubId = 22,
                               Scenario = PointRewardScenario.QRCodeSan
                           };

            var newAmount = 444;

            _uow.Setup(x => x.ClubPointRewardScenarioRepository.GetById(scenario.Id))
                .Returns(scenario);

            //Act
            Assert.Throws<SecurityException>(() => _sut.ChangeRewardAmount(scenario.Id, 123, newAmount));

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void UpdateScenario_OnChangeRewardAmount()
        {
            //Arrange
            var scenario = new ClubPointRewardScenario
                           {
                               Id = 555,
                               Amount = 12,
                               ClubId = 22,
                               Scenario = PointRewardScenario.QRCodeSan
                           };

            var newAmount = 444;

            _uow.Setup(x => x.ClubPointRewardScenarioRepository.GetById(scenario.Id))
                .Returns(scenario);

            _uow.Setup(x => x.SaveChanges());

            //Act
            _sut.ChangeRewardAmount(scenario.Id, scenario.ClubId, newAmount);

            //Assert
            Assert.AreEqual(newAmount, scenario.Amount);
            _mockRepo.VerifyAll();
        }

        [Test]
        public void NotDoAnythingIfScenarioDoesntExists_OnRemoveRewardScenario()
        {
            //Arrange
            var scenario = new ClubPointRewardScenario
                           {
                               Id = 555,
                               Amount = 12,
                               ClubId = 22,
                               Scenario = PointRewardScenario.QRCodeSan
                           };

            _uow.Setup(x => x.ClubPointRewardScenarioRepository.GetById(scenario.Id))
                .Returns(() => null);

            //Act
            _sut.RemoveRewardScenario(scenario.Id, scenario.ClubId);

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void ThrowIfUserIsNotAllowedToRemove_OnRemoveRewardScenario()
        {
            //Arrange
            var scenario = new ClubPointRewardScenario
                           {
                               Id = 555,
                               Amount = 12,
                               ClubId = 22,
                               Scenario = PointRewardScenario.QRCodeSan
                           };

            _uow.Setup(x => x.ClubPointRewardScenarioRepository.GetById(scenario.Id))
                .Returns(scenario);

            //Act
            Assert.Throws<SecurityException>(() => _sut.RemoveRewardScenario(scenario.Id, 1));

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void RemoveScenario_OnRemoveRewardScenario()
        {
            //Arrange
            var scenario = new ClubPointRewardScenario
                           {
                               Id = 555,
                               Amount = 12,
                               ClubId = 22,
                               Scenario = PointRewardScenario.QRCodeSan
                           };

            _uow.Setup(x => x.ClubPointRewardScenarioRepository.GetById(scenario.Id))
                .Returns(scenario);

            _uow.Setup(x => x.ClubPointRewardScenarioRepository.Remove(scenario));
            _uow.Setup(x => x.SaveChanges());

            //Act
            _sut.RemoveRewardScenario(scenario.Id, scenario.ClubId);

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void ThrowIfClubIdIsMissing_OnAddClubReward()
        {
            //Arrange
            var reward = new ClubReward
                         {
                             Name = "name",
                             Description = "desc",
                             IsEnabled = true
                         };

            //Act
            Assert.Throws<ArgumentException>(() => _sut.AddClubReward(reward));

            //Assert
            _mockRepo.VerifyAll();
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void ThrowIfNameIsMissing_OnAddClubReward(string name)
        {
            //Arrange
            var reward = new ClubReward
            {
                ClubId = 232,
                Name = name,
                Description = "desc",
                IsEnabled = true
            };

            //Act
            Assert.Throws<ArgumentException>(() => _sut.AddClubReward(reward));

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void AddClubReward_OnAddClubReward()
        {
            //Arrange
            var reward = new ClubReward
                         {
                             ClubId = 33,
                             Name = "name",
                             Description = "desc",
                             IsEnabled = true
                         };

            _uow.Setup(x => x.ClubRewardRepository.InsertGraph(reward));
            _uow.Setup(x => x.SaveChanges());

            //Act
            _sut.AddClubReward(reward);

            //Assert
            _mockRepo.VerifyAll();
        }
    }
}