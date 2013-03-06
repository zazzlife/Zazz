using System;
using NUnit.Framework;
using Zazz.Core.Models.Data;
using Zazz.Data;
using Zazz.Data.Repositories;

namespace Zazz.IntegrationTests.Repositories
{
    [TestFixture]
    public class ClubFollowRepositoryShould
    {
        private ZazzDbContext _context;
        private ClubFollowRepository _repo;

        [SetUp]
        public void Init()
        {
            _context = new ZazzDbContext(true);
            _repo = new ClubFollowRepository(_context);
        }

        [Test]
        public void ThrowException_OnInsertOrUpdate_WhenClubIdIs0()
        {
            //Arrange
            var clubFollow = new ClubFollow { UserId = 12 };

            //Act & Assert
            Assert.Throws<ArgumentException>(() => _repo.InsertOrUpdate(clubFollow));
        }

        [Test]
        public void ThrowException_OnInsertOrUpdate_WhenUserIdIs0()
        {
            //Arrange
            var clubFollow = new ClubFollow { ClubId = 12 };

            //Act & Assert
            Assert.Throws<ArgumentException>(() => _repo.InsertOrUpdate(clubFollow));
        }

        [Test]
        public void ReturnTrue_WhenUserFollowsAClub()
        {
            //Arrange
            var user = Mother.GetUser();
            _context.Users.Add(user);
            _context.SaveChanges();

            var club = Mother.GetClub();
            _context.Clubs.Add(club);
            _context.SaveChanges();

            var follow = new ClubFollow { ClubId = club.Id, UserId = user.Id };
            _context.ClubFollows.Add(follow);
            _context.SaveChanges();

            //Act
            var result = _repo.ExistsAsync(user.Id, club.Id).Result;

            //Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void ReturnFalse_WhenUserNotFollowsAClub()
        {
            //Arrange
            var userA = Mother.GetUser();
            var userB = Mother.GetUser();
            _context.Users.Add(userA);
            _context.Users.Add(userB);
            _context.SaveChanges();

            var club = Mother.GetClub();
            _context.Clubs.Add(club);
            _context.SaveChanges();

            var follow = new ClubFollow { ClubId = club.Id, UserId = userA.Id };
            _context.ClubFollows.Add(follow);
            _context.SaveChanges();

            //Act
            var result = _repo.ExistsAsync(userB.Id, club.Id).Result;

            //Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void DeleteOnRemoveAsync()
        {
            //Arrange
            var user = Mother.GetUser();
            _context.Users.Add(user);
            _context.SaveChanges();

            var club = Mother.GetClub();
            _context.Clubs.Add(club);
            _context.SaveChanges();

            var follow = new ClubFollow { ClubId = club.Id, UserId = user.Id };
            _context.ClubFollows.Add(follow);
            _context.SaveChanges();

            Assert.IsTrue(_repo.ExistsAsync(user.Id, club.Id).Result);

            //Act
            _repo.RemoveAsync(user.Id, club.Id).Wait();
            _context.SaveChanges();

            var check = _repo.ExistsAsync(user.Id, club.Id).Result;
            //Assert

            Assert.IsFalse(check);
        }
    }
}