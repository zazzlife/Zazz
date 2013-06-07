using System;
using System.Linq;
using NUnit.Framework;
using Zazz.Core.Models.Data;
using Zazz.Data;
using Zazz.Data.Repositories;

namespace Zazz.IntegrationTests.Repositories
{
    [TestFixture]
    public class UserReceivedVotesRepositoryShould
    {
        private ZazzDbContext _context;
        private UserReceivedVotesRepository _repo;
        private User _user1;
        private User _user2;

        [SetUp]
        public void Init()
        {
            _context = new ZazzDbContext(true);
            _repo = new UserReceivedVotesRepository(_context);

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

            _context.UserReceivedVotes.Add(new UserReceivedVotes
                                           {
                                               Count = user1Count,
                                               LastUpdate = DateTime.UtcNow,
                                               UserId = _user1.Id
                                           });

            _context.UserReceivedVotes.Add(new UserReceivedVotes
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
            var check = _context.UserReceivedVotes.Find(_user1.Id);
            Assert.AreEqual(1, check.Count);
        }

        [Test]
        public void IncrementCountIfRecordExists_OnIncrement()
        {
            //Arrange
            var currentCount = 222;
            using (var ctx = new ZazzDbContext())
            {
                ctx.UserReceivedVotes.Add(new UserReceivedVotes
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
            var check = _context.UserReceivedVotes.SingleOrDefault(u => u.UserId == _user1.Id);
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
            var check = _context.UserReceivedVotes.Find(_user1.Id);
            Assert.AreEqual(0, check.Count);
        }

        [Test]
        public void DecrementCountIfRecordExists_OnDecrement()
        {
            //Arrange
            var currentCount = 222;
            using (var ctx = new ZazzDbContext())
            {
                ctx.UserReceivedVotes.Add(new UserReceivedVotes
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
            var check = _context.UserReceivedVotes.SingleOrDefault(u => u.UserId == _user1.Id);
            Assert.AreEqual((currentCount - 1), check.Count);
        }
    }
}