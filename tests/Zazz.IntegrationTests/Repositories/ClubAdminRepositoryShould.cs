using System;
using NUnit.Framework;
using Zazz.Core.Models.Data;
using Zazz.Data;
using Zazz.Data.Repositories;

namespace Zazz.IntegrationTests.Repositories
{
    [TestFixture]
    public class ClubAdminRepositoryShould
    {
        private ZazzDbContext _context;
        private ClubAdminRepository _repo;

        [SetUp]
        public void Init()
        {
            _context = new ZazzDbContext(true);
            _repo = new ClubAdminRepository(_context);
        }

        [Test]
        public void ThrowException_OnInsertOrUpdate_WhenClubIdIs0()
        {
            //Arrange
            var clubAdmin = new ClubAdmin { AssignedByUserId = 12, UserId = 33 };

            //Act & Assert
            Assert.Throws<ArgumentException>(() => _repo.InsertOrUpdate(clubAdmin));
        }

        [Test]
        public void ThrowException_OnInsertOrUpdate_WhenUserIdIs0()
        {
            //Arrange
            var clubAdmin = new ClubAdmin { AssignedByUserId = 12, ClubId = 33 };

            //Act & Assert
            Assert.Throws<ArgumentException>(() => _repo.InsertOrUpdate(clubAdmin));
        }

        [Test]
        public void ReturnTrue_WhenAdminExists()
        {
            //Arrange
            var user = Mother.GetUser();
            _context.Users.Add(user);
            _context.SaveChanges();

            var club = Mother.GetClub();
            _context.Clubs.Add(club);
            _context.SaveChanges();

            var clubAdmin = new ClubAdmin { AssignedByUserId = user.Id, ClubId = club.Id, UserId = user.Id };
            _context.ClubAdmins.Add(clubAdmin);
            _context.SaveChanges();

            //Act
            var result = _repo.ExistsAsync(user.Id, club.Id).Result;

            //Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void ReturnFalse_WhenAdminNotExists()
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

            var clubAdmin = new ClubAdmin { AssignedByUserId = userA.Id, ClubId = club.Id, UserId = userA.Id };
            _context.ClubAdmins.Add(clubAdmin);
            _context.SaveChanges();

            //Act
            var result = _repo.ExistsAsync(userB.Id, club.Id).Result;

            //Assert
            Assert.IsFalse(result);
        }
    }
}