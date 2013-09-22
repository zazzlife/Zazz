using System;
using System.Linq;
using NUnit.Framework;
using Zazz.Core.Models.Data;
using Zazz.Data;
using Zazz.Data.Repositories;

namespace Zazz.IntegrationTests.Repositories
{
    [TestFixture]
    public class UserReceivedLikesRepositoryShould
    {
        private ZazzDbContext _context;
        private UserReceivedLikesRepository _repo;
        private User _user1;
        private User _user2;

        [SetUp]
        public void Init()
        {
            _context = new ZazzDbContext(true);
            _repo = new UserReceivedLikesRepository(_context);

            _user1 = Mother.GetUser();
            _user2 = Mother.GetUser();

            _context.Users.Add(_user1);
            _context.Users.Add(_user2);
            _context.SaveChanges();
        }

        [Test]
        public void GetCount_OnGetCount()
        {
            //Arrange
            var user1Count = 50;
            var user2Count = 49;

            _context.UserReceivedLikes.Add(new UserReceivedLikes
                                           {
                                               Count = user1Count,
                                               LastUpdate = DateTime.UtcNow,
                                               UserId = _user1.Id
                                           });

            _context.UserReceivedLikes.Add(new UserReceivedLikes
                                           {
                                               Count = user2Count,
                                               LastUpdate = DateTime.UtcNow,
                                               UserId = _user2.Id
                                           });

            _context.SaveChanges();

            //Act
            var result = _repo.GetCount(_user1.Id);

            //Assert
            Assert.AreEqual(user1Count, result);
        }

        [Test]
        public void InsertRecordIfNotExists_OnIncrement()
        {
            //Arrange

            //Act
            _repo.Increment(_user1.Id);
            _context.SaveChanges();

            //Assert
            var check = _context.UserReceivedLikes.Find(_user1.Id);
            Assert.AreEqual(1, check.Count);
        }

        [Test]
        public void IncrementCountIfRecordExists_OnIncrement()
        {
            //Arrange
            var currentCount = 222;
            using (var ctx = new ZazzDbContext())
            {
                ctx.UserReceivedLikes.Add(new UserReceivedLikes
                                          {
                                              Count = currentCount,
                                              LastUpdate = DateTime.UtcNow,
                                              UserId = _user1.Id
                                          });

                ctx.SaveChanges();
            }

            //Act
            _repo.Increment(_user1.Id);

            //Assert
            var check = _context.UserReceivedLikes.SingleOrDefault(u => u.UserId == _user1.Id);
            Assert.AreEqual((currentCount + 1), check.Count);
        }

        [Test]
        public void InsertRecordIfNotExists_OnDecrement()
        {
            //Arrange

            //Act
            _repo.Decrement(_user1.Id);
            _context.SaveChanges();

            //Assert
            var check = _context.UserReceivedLikes.Find(_user1.Id);
            Assert.AreEqual(0, check.Count);
        }

        [Test]
        public void DecrementCountIfRecordExists_OnDecrement()
        {
            //Arrange
            var currentCount = 222;
            using (var ctx = new ZazzDbContext())
            {
                ctx.UserReceivedLikes.Add(new UserReceivedLikes
                {
                    Count = currentCount,
                    LastUpdate = DateTime.UtcNow,
                    UserId = _user1.Id
                });

                ctx.SaveChanges();
            }

            //Act
            _repo.Decrement(_user1.Id);

            //Assert
            var check = _context.UserReceivedLikes.SingleOrDefault(u => u.UserId == _user1.Id);
            Assert.AreEqual((currentCount - 1), check.Count);
        }
    }
}