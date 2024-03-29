﻿using System;
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
        public void ThrowIfIdIsInvalid_OnGetWeekly()
        {
            //Arrange
            //Act
            Assert.Throws<ArgumentException>(() => _sut.GetWeekly(0));

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void ThrowIfWeeklyNotExists_OnGetWeekly()
        {
            //Arrange
            var weekly = new Weekly { Id = 45 };
            _uow.Setup(x => x.WeeklyRepository.GetById(weekly.Id))
                .Returns(() => null);

            //Act
            Assert.Throws<NotFoundException>(() => _sut.GetWeekly(weekly.Id));

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void ReturnWeekly_OnGetWeekly()
        {
            //Arrange
            var weekly = new Weekly { Id = 45 };
            _uow.Setup(x => x.WeeklyRepository.GetById(weekly.Id))
                .Returns(weekly);

            //Act
            var result = _sut.GetWeekly(weekly.Id);

            //Assert
            Assert.AreSame(weekly, result);
            _mockRepo.VerifyAll();
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

            _uow.Setup(x => x.UserRepository.GetById(user.Id, false, false, true, false))
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
        public void ThrowIfUserHasAlreadyCreated7Weeklies_OnCreateWeekly()
        {
            //Arrange
            var user = new User
            {
                Id = 2,
                AccountType = AccountType.Club
            };

            for (int i = 0; i < 7; i++)
                user.Weeklies.Add(new Weekly());

            _uow.Setup(x => x.UserRepository.GetById(user.Id, false, false, true, false))
                .Returns(user);

            var weekly = new Weekly
            {
                UserId = user.Id
            };

            //Act & Assert
            Assert.Throws<WeekliesLimitReachedException>(() => _sut.CreateWeekly(weekly));
            _mockRepo.VerifyAll();
        }

        [Test]
        public void InsertANewRecord_OnCreateWeekly()
        {
            //Arrange
            var user = new User
            {
                Id = 2,
                AccountType = AccountType.Club
            };

            _uow.Setup(x => x.UserRepository.GetById(user.Id, false, false, true, false))
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
            _mockRepo.VerifyAll();
        }

        [Test]
        public void ThrowNotFoundExceptionIfWeeklyDoesntExists_OnEdit()
        {
            //Arrange
            var weekly = new Weekly
                         {
                             Id = 5,
                             UserId = 2
                         };

            _uow.Setup(x => x.WeeklyRepository.GetById(weekly.Id))
                .Returns(() => null);

            //Act & Assert
            Assert.Throws<NotFoundException>(() => _sut.EditWeekly(weekly, weekly.UserId));
            _mockRepo.VerifyAll();
        }

        [Test]
        public void ThrowIfUserIsNotTheOwner_OnEdit()
        {
            //Arrange
            var weekly = new Weekly
                         {
                             Id = 5,
                             UserId = 2
                         };

            _uow.Setup(x => x.WeeklyRepository.GetById(weekly.Id))
                .Returns(weekly);

            //Act & Assert
            Assert.Throws<SecurityException>(() => _sut.EditWeekly(weekly, weekly.UserId + 1));
            _mockRepo.VerifyAll();
        }

        [Test]
        public void UpdateTheRecordAndSave_OnEdit()
        {
            //Arrange
            var oldWeekly = new Weekly
                         {
                             Id = 5,
                             UserId = 2
                         };

            var newWeekly = new Weekly
                            {
                                Id = 5,
                                UserId = 2,
                                Description = "new desc",
                                DayOfTheWeek = DayOfTheWeek.Saturday,
                                Name = "new name",
                                PhotoId = 32,
                            };

            _uow.Setup(x => x.WeeklyRepository.GetById(oldWeekly.Id))
                .Returns(oldWeekly);
            _uow.Setup(x => x.SaveChanges());

            //Act
            _sut.EditWeekly(newWeekly, newWeekly.UserId);

            //Assert
            Assert.AreEqual(oldWeekly.Id, newWeekly.Id);
            Assert.AreEqual(oldWeekly.UserId, newWeekly.UserId);
            Assert.AreEqual(oldWeekly.Description, newWeekly.Description);
            Assert.AreEqual(oldWeekly.DayOfTheWeek, newWeekly.DayOfTheWeek);
            Assert.AreEqual(oldWeekly.Name, newWeekly.Name);
            Assert.AreEqual(oldWeekly.PhotoId, newWeekly.PhotoId);
            _mockRepo.VerifyAll();
        }

        [Test]
        public void NotThrowIfRecordDoesntExists_OnRemove()
        {
            //Arrange
            var weekly = new Weekly
                         {
                             Id = 5,
                             UserId = 2
                         };

            _uow.Setup(x => x.WeeklyRepository.GetById(weekly.Id))
                .Returns(() => null);

            //Act
            _sut.RemoveWeekly(weekly.Id, weekly.UserId);

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void ThrowIfUserIsNotOwner_OnRemove()
        {
            //Arrange
            var weekly = new Weekly
            {
                Id = 5,
                UserId = 2
            };

            _uow.Setup(x => x.WeeklyRepository.GetById(weekly.Id))
                .Returns(weekly);

            //Act & Assert
            Assert.Throws<SecurityException>(() => _sut.RemoveWeekly(weekly.Id, weekly.UserId + 1));
            _mockRepo.VerifyAll();
        }

        [Test]
        public void RemoveAndSave_OnRemove()
        {
            //Arrange
            var weekly = new Weekly
            {
                Id = 5,
                UserId = 2
            };

            _uow.Setup(x => x.WeeklyRepository.GetById(weekly.Id))
                .Returns(weekly);
            _uow.Setup(x => x.WeeklyRepository.Remove(weekly));
            _uow.Setup(x => x.SaveChanges());

            //Act
            _sut.RemoveWeekly(weekly.Id, weekly.UserId);

            //Assert
            _mockRepo.VerifyAll();
        }


    }
}