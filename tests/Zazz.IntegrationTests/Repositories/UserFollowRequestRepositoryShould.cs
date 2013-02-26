using System;
using System.Data.Entity;
using NUnit.Framework;
using Zazz.Core.Models.Data;
using Zazz.Data;
using Zazz.Data.Repositories;

namespace Zazz.IntegrationTests.Repositories
{
    [TestFixture]
    public class UserFollowRequestRepositoryShould
    {
        private ZazzDbContext _context;
        private UserFollowRequestRepository _repo;

        [SetUp]
        public void Init()
        {
            _context = new ZazzDbContext(true);
            _repo = new UserFollowRequestRepository(_context);
        }

        [Test]
        public void ThrowException_OnInsertOrUpdate_WhenFromUserIdIs0()
        {
            //Arrange
            var followRequest = new UserFollowRequest { ToUserId = 21, RequestDate = DateTime.Now };

            //Act & Assert
            Assert.Throws<ArgumentException>(() => _repo.InsertOrUpdate(followRequest));
        }

        [Test]
        public void ThrowException_OnInsertOrUpdate_WhenToUserIdIs0()
        {
            //Arrange
            var followRequest = new UserFollowRequest { FromUserId = 21, RequestDate = DateTime.Now };

            //Act & Assert
            Assert.Throws<ArgumentException>(() => _repo.InsertOrUpdate(followRequest));
        }

        [Test]
        public void GetUserReceivedFollowRequests_OnGetReceivedRequestsAsync()
        {
            //Arrange (user B and C will send a request to user A)
            var userA = Mother.GetUser();
            var userB = Mother.GetUser();
            var userC = Mother.GetUser();

            using (var ctx = new ZazzDbContext())
            {
                ctx.Users.Add(userA);
                ctx.Users.Add(userB);
                ctx.Users.Add(userC);

                ctx.SaveChanges();

                var requestB = new UserFollowRequest
                                   {
                                       FromUserId = userB.Id,
                                       ToUserId = userA.Id,
                                       RequestDate = DateTime.Now
                                   };

                var requestC = new UserFollowRequest
                                   {
                                       FromUserId = userC.Id,
                                       ToUserId = userA.Id,
                                       RequestDate = DateTime.Now
                                   };

                ctx.UserFollowRequests.Add(requestB);
                ctx.UserFollowRequests.Add(requestC);

                ctx.SaveChanges();
            }

            //Act
            var result = _repo.GetReceivedRequestsAsync(userA.Id).Result;

            //Assert
            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public void GetUserSentFollowRequests_OnGetSentRequestsAsync()
        {
            //Arrange (User A sends a request to user B and user C)
            var userA = Mother.GetUser();
            var userB = Mother.GetUser();
            var userC = Mother.GetUser();

            using (var ctx = new ZazzDbContext())
            {
                ctx.Users.Add(userA);
                ctx.Users.Add(userB);
                ctx.Users.Add(userC);

                ctx.SaveChanges();

                var requestB = new UserFollowRequest
                {
                    FromUserId = userA.Id,
                    ToUserId = userB.Id,
                    RequestDate = DateTime.Now
                };

                var requestC = new UserFollowRequest
                {
                    FromUserId = userA.Id,
                    ToUserId = userC.Id,
                    RequestDate = DateTime.Now
                };

                ctx.UserFollowRequests.Add(requestB);
                ctx.UserFollowRequests.Add(requestC);

                ctx.SaveChanges();
            }

            //Act
            var result = _repo.GetSentRequestsAsync(userA.Id).Result;
            
            //Assert
            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public void Remove_OnRemoveAsync()
        {
            //Arrange
            var userA = Mother.GetUser();
            var userB = Mother.GetUser();

            using (var ctx = new ZazzDbContext())
            {
                ctx.Users.Add(userA);
                ctx.Users.Add(userB);

                ctx.SaveChanges();

                var requestB = new UserFollowRequest
                {
                    FromUserId = userA.Id,
                    ToUserId = userB.Id,
                    RequestDate = DateTime.Now
                };

                ctx.UserFollowRequests.Add(requestB);

                ctx.SaveChanges();
            }

            //Act
            _repo.RemoveAsync(userA.Id, userB.Id).Wait();
            _context.SaveChanges();

            var check = _repo.ExistsAsync(userA.Id, userB.Id).Result;

            //Assert
            Assert.IsFalse(check);
        }

        [Test]
        public void ReturnTrue_OnExists_WhenRequestExists()
        {
            //Arrange (user A sends a request to user B)
            var userA = Mother.GetUser();
            var userB = Mother.GetUser();

            using (var ctx = new ZazzDbContext())
            {
                ctx.Users.Add(userA);
                ctx.Users.Add(userB);

                ctx.SaveChanges();

                var requestB = new UserFollowRequest
                {
                    FromUserId = userA.Id,
                    ToUserId = userB.Id,
                    RequestDate = DateTime.Now
                };

                ctx.UserFollowRequests.Add(requestB);

                ctx.SaveChanges();
            }

            //Act
            var result = _repo.ExistsAsync(userA.Id, userB.Id).Result;

            //Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void ReturnFalse_OnExists_WhenNotRequestExists()
        {
            //Arrange (user A sends a request to user B, but we use the user B as sender id)
            var userA = Mother.GetUser();
            var userB = Mother.GetUser();

            using (var ctx = new ZazzDbContext())
            {
                ctx.Users.Add(userA);
                ctx.Users.Add(userB);

                ctx.SaveChanges();

                var requestB = new UserFollowRequest
                {
                    FromUserId = userA.Id,
                    ToUserId = userB.Id,
                    RequestDate = DateTime.Now
                };

                ctx.UserFollowRequests.Add(requestB);

                ctx.SaveChanges();
            }

            //Act
            var result = _repo.ExistsAsync(userB.Id, userA.Id).Result;

            //Assert
            Assert.IsFalse(result);
        }
    }
}