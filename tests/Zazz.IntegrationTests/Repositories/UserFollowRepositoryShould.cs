using NUnit.Framework;
using Zazz.Data;
using Zazz.Data.Repositories;

namespace Zazz.IntegrationTests.Repositories
{
    [TestFixture]
    public class UserFollowRepositoryShould
    {
        private ZazzDbContext _context;
        private UserFollowRepository _repo;

        [SetUp]
        public void Init()
        {
            _context = new ZazzDbContext(true);
            _repo = new UserFollowRepository(_context);
        }


    }
}