using NUnit.Framework;
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


    }
}