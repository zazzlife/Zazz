using System;
using System.Linq;
using NUnit.Framework;
using Zazz.Core.Models.Data;
using Zazz.Data;
using Zazz.Data.Repositories;

namespace Zazz.IntegrationTests.Repositories
{
    [TestFixture]
    public class FollowRepositoryShould
    {
        private ZazzDbContext _context;
        private FollowRepository _repo;
        private User _userA;
        private User _userB;
        private Follow _follow;

        [SetUp]
        public void Init()
        {
            _context = new ZazzDbContext(true);
            _repo = new FollowRepository(_context);

            _userA = Mother.GetUser();
            _userB = Mother.GetUser();

            _context.Users.Add(_userA);
            _context.Users.Add(_userB);

            _context.SaveChanges();

            _follow = new Follow {FromUserId = _userA.Id, ToUserId = _userB.Id};
            _context.Follows.Add(_follow);

            _context.SaveChanges();
        }

        [Test]
        public void ThrowException_OnInsertOrUpdate_WhenFromUserIdIsNotProvided()
        {
            //Arrange
            _follow.Id = 0;
            _follow.FromUserId = 0;

            //Act & Assert
            Assert.Throws<ArgumentException>(() => _repo.InsertOrUpdate(_follow));
        }

        [Test]
        public void ThrowException_OnInsertOrUpdate_WhenToUserIdIsNotProvided()
        {
            //Arrange
            _follow.Id = 0;
            _follow.ToUserId = 0;

            //Act & Assert
            Assert.Throws<ArgumentException>(() => _repo.InsertOrUpdate(_follow));
        }

        [Test]
        public void ReturnFollowersCorrectly()
        {
            //Arrange
            //Act
            var result = _repo.GetUserFollowersAsync(_userB.Id).Result;

            //Assert
            Assert.AreEqual(1, result.Count());
        }

        [Test]
        public void ReturnFollowsCorrectly()
        {
            //Arrange
            //Act
            var result = _repo.GetUserFollowsAsync(_userA.Id).Result;

            //Assert
            Assert.AreEqual(1, result.Count());
        }

        [Test]
        public void ReturnTrue_WhenFollowExists()
        {
            //Arrange
            //Act
            var result = _repo.ExistsAsync(_userA.Id, _userB.Id).Result;

            //Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void ReturnFalse_WhenFollowNotExists()
        {
            //Arrange
            //Act
            var result = _repo.ExistsAsync(_userB.Id, _userA.Id).Result;

            //Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void RemoveCorrectly_OnRemoveAsync()
        {
            //Arrange
            //Act
            _repo.RemoveAsync(_userA.Id, _userB.Id).Wait();
            _context.SaveChanges();

            var check = _repo.ExistsAsync(_userA.Id, _userB.Id).Result;
            //Assert
            Assert.IsFalse(check);
        }
    }
}