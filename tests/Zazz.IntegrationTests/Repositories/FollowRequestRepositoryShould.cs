using System;
using System.Data.Entity;
using System.Linq;
using NUnit.Framework;
using Zazz.Core.Models.Data;
using Zazz.Data;
using Zazz.Data.Repositories;

namespace Zazz.IntegrationTests.Repositories
{
    [TestFixture]
    public class FollowRequestRepositoryShould
    {
        private ZazzDbContext _context;
        private FollowRequestRepository _repo;

        [SetUp]
        public void Init()
        {
            _context = new ZazzDbContext(true);
            _repo = new FollowRequestRepository(_context);
        }

        [Test]
        public void ReturnCorrectFollowRequest_OnGetFollowRequest()
        {
            //Arrange
            var userA = Mother.GetUser();
            var userB = Mother.GetUser();
            var userC = Mother.GetUser();
            _context.Users.Add(userA);
            _context.Users.Add(userB);
            _context.Users.Add(userC);
            _context.SaveChanges();

            var aToB = new FollowRequest { FromUserId = userA.Id, ToUserId = userB.Id, RequestDate = DateTime.UtcNow };

            var aToC = new FollowRequest { FromUserId = userA.Id, ToUserId = userC.Id, RequestDate = DateTime.UtcNow };

            var bToA = new FollowRequest { FromUserId = userB.Id, ToUserId = userA.Id, RequestDate = DateTime.UtcNow };

            var bToC = new FollowRequest { FromUserId = userB.Id, ToUserId = userC.Id, RequestDate = DateTime.UtcNow };

            var cToA = new FollowRequest { FromUserId = userC.Id, ToUserId = userA.Id, RequestDate = DateTime.UtcNow };

            var cToB = new FollowRequest { FromUserId = userC.Id, ToUserId = userB.Id, RequestDate = DateTime.UtcNow };

            _context.FollowRequests.Add(aToB);
            _context.FollowRequests.Add(aToC);
            _context.FollowRequests.Add(bToA);
            _context.FollowRequests.Add(bToC);
            _context.FollowRequests.Add(cToA);
            _context.FollowRequests.Add(cToB);
            _context.SaveChanges();

            //Act
            var result = _repo.GetFollowRequest(userA.Id, userB.Id);

            //Assert
            Assert.AreEqual(userA.Id, result.FromUserId);
            Assert.AreEqual(userB.Id, result.ToUserId);
        }

        [Test]
        public void ReturnCorrectNumberOfReceivedRequests_OnGetReceivedRequestsCount()
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

                var requestB = new FollowRequest
                {
                    FromUserId = userB.Id,
                    ToUserId = userA.Id,
                    RequestDate = DateTime.Now
                };

                var requestC = new FollowRequest
                {
                    FromUserId = userC.Id,
                    ToUserId = userA.Id,
                    RequestDate = DateTime.Now
                };

                ctx.FollowRequests.Add(requestB);
                ctx.FollowRequests.Add(requestC);

                ctx.SaveChanges();
            }

            //Act
            var result = _repo.GetReceivedRequestsCount(userA.Id);

            //Assert
            Assert.AreEqual(2, result);
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

                var requestB = new FollowRequest
                                   {
                                       FromUserId = userB.Id,
                                       ToUserId = userA.Id,
                                       RequestDate = DateTime.Now
                                   };

                var requestC = new FollowRequest
                                   {
                                       FromUserId = userC.Id,
                                       ToUserId = userA.Id,
                                       RequestDate = DateTime.Now
                                   };

                ctx.FollowRequests.Add(requestB);
                ctx.FollowRequests.Add(requestC);

                ctx.SaveChanges();
            }

            //Act
            var result = _repo.GetReceivedRequests(userA.Id).ToList();

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

                var requestB = new FollowRequest
                {
                    FromUserId = userA.Id,
                    ToUserId = userB.Id,
                    RequestDate = DateTime.Now
                };

                var requestC = new FollowRequest
                {
                    FromUserId = userA.Id,
                    ToUserId = userC.Id,
                    RequestDate = DateTime.Now
                };

                ctx.FollowRequests.Add(requestB);
                ctx.FollowRequests.Add(requestC);

                ctx.SaveChanges();
            }

            //Act
            var result = _repo.GetSentRequests(userA.Id).ToList();

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

                var requestB = new FollowRequest
                {
                    FromUserId = userA.Id,
                    ToUserId = userB.Id,
                    RequestDate = DateTime.Now
                };

                ctx.FollowRequests.Add(requestB);

                ctx.SaveChanges();
            }

            //Act
            _repo.Remove(userA.Id, userB.Id);
            _context.SaveChanges();

            var check = _repo.Exists(userA.Id, userB.Id);

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

                var requestB = new FollowRequest
                {
                    FromUserId = userA.Id,
                    ToUserId = userB.Id,
                    RequestDate = DateTime.Now
                };

                ctx.FollowRequests.Add(requestB);

                ctx.SaveChanges();
            }

            //Act
            var result = _repo.Exists(userA.Id, userB.Id);

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

                var requestB = new FollowRequest
                {
                    FromUserId = userA.Id,
                    ToUserId = userB.Id,
                    RequestDate = DateTime.Now
                };

                ctx.FollowRequests.Add(requestB);

                ctx.SaveChanges();
            }

            //Act
            var result = _repo.Exists(userB.Id, userA.Id);

            //Assert
            Assert.IsFalse(result);
        }
    }
}