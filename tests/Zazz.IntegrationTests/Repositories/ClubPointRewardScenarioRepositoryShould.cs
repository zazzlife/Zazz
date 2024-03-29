﻿using System.Data.Entity;
using NUnit.Framework;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;
using Zazz.Data;
using Zazz.Data.Repositories;

namespace Zazz.IntegrationTests.Repositories
{
    [TestFixture]
    public class ClubPointRewardScenarioRepositoryShould
    {
        private ZazzDbContext _context;
        private ClubPointRewardScenarioRepository _repo;
        private User _club;
        private ClubPointRewardScenario _scenario;
        private User _club2;

        [SetUp]
        public void Init()
        {
            _context = new ZazzDbContext(true);
            _repo = new ClubPointRewardScenarioRepository(_context);

            _club = Mother.GetUser();
            _club2 = Mother.GetUser();
            _context.Users.Add(_club);
            _context.Users.Add(_club2);
            _context.SaveChanges();
            
            _scenario = new ClubPointRewardScenario
                        {
                            MondayAmount = 1,
                            TuesdayAmount = 2,
                            WednesdayAmount = 3,
                            ThursdayAmount = 4,
                            FridayAmount = 5,
                            SaturdayAmount = 6,
                            SundayAmount = 7,
                            ClubId = _club.Id,
                            Scenario = PointRewardScenario.QRCodeSan
                        };

            _context.ClubPointRewardScenarios.Add(_scenario);
            _context.SaveChanges();
        }

        [Test]
        public void ReturnTrueWhenScenarioExists_OnExists()
        {
            //Arrange
            //Act
            var result = _repo.Exists(_club.Id, PointRewardScenario.QRCodeSan);

            //Assert
            Assert.IsTrue(result);
        }


        [Test]
        public void ReturnFalseWhenScenarioNotExists_OnExists()
        {
            //Arrange
            //Act
            var result = _repo.Exists(_club2.Id, PointRewardScenario.QRCodeSan);

            //Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void ReturnScenarioIfExists_OnGet()
        {
            //Arrange
            //Act
            var result = _repo.Get(_club.Id, PointRewardScenario.QRCodeSan);

            //Assert
            Assert.IsNotNull(result);
        }

        [Test]
        public void ReturnNullIfNotExists_OnGet()
        {
            //Arrange
            //Act
            var result = _repo.Get(_club2.Id, PointRewardScenario.QRCodeSan);

            //Assert
            Assert.IsNull(result);
        }
    }
}