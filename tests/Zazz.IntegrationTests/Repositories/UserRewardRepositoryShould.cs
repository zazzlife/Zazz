using System;
using NUnit.Framework;
using Zazz.Core.Models.Data;
using Zazz.Data;
using Zazz.Data.Repositories;

namespace Zazz.IntegrationTests.Repositories
{
    [TestFixture]
    public class UserRewardRepositoryShould
    {
        private ZazzDbContext _context;
        private UserRewardRepository _repo;
        private ClubReward _reward1;
        private ClubReward _reward2;
        private User _user1;
        private User _user2;
        private User _club1;
        private User _club2;

        [SetUp]
        public void Init()
        {
            _context = new ZazzDbContext(true);
            _repo = new UserRewardRepository(_context);

            _user1 = Mother.GetUser();
            _user2 = Mother.GetUser();
            _club1 = Mother.GetUser();
            _club2 = Mother.GetUser();

            _context.Users.Add(_user1);
            _context.Users.Add(_user2);
            _context.Users.Add(_club1);
            _context.Users.Add(_club2);
            _context.SaveChanges();

            _reward1 = new ClubReward { ClubId = _club1.Id, Name = "name1" };
            _reward2 = new ClubReward { ClubId = _club2.Id, Name = "name2" };

            _context.ClubRewards.Add(_reward1);
            _context.ClubRewards.Add(_reward2);
            _context.SaveChanges();

            var userReward1 = new UserReward
                              {
                                  RedeemedDate = DateTime.UtcNow,
                                  UserId = _user1.Id,
                                  RewardId = _reward1.Id,
                              };

            var userReward2 = new UserReward
                              {
                                  RedeemedDate = DateTime.UtcNow,
                                  UserId = _user2.Id,
                                  RewardId = _reward2.Id,
                              };

            _context.UserRewards.Add(userReward1);
            _context.UserRewards.Add(userReward2);
            _context.SaveChanges();
        }

        [Test]
        public void ReturnFalseIfNotExists_OnExists()
        {
            //Arrange
            //Act
            var result = _repo.Exists(_user1.Id, _reward2.Id);

            //Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void ReturnTrueIfExists_OnExists()
        {
            //Arrange
            //Act
            var result = _repo.Exists(_user1.Id, _reward1.Id);

            //Assert
            Assert.IsTrue(result);
        }
    }
}