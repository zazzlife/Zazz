using System;
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
        public void ShouldThrowException_OnInsertOrUpdate_WhenFromUserIdIs0()
        {
            //Arrange
            var followRequest = new UserFollowRequest {ToUserId = 21, RequestDate = DateTime.Now};

            //Act & Assert
            Assert.Throws<ArgumentException>(() => _repo.InsertOrUpdate(followRequest));
        }

        [Test]
        public void ShouldThrowException_OnInsertOrUpdate_WhenToUserIdIs0()
        {
            //Arrange
            var followRequest = new UserFollowRequest { FromUserId = 21, RequestDate = DateTime.Now };

            //Act & Assert
            Assert.Throws<ArgumentException>(() => _repo.InsertOrUpdate(followRequest));
        }
    }
}