using NUnit.Framework;
using Zazz.Data;
using Zazz.Data.Repositories;

namespace Zazz.IntegrationTests.Repositories
{
    [TestFixture]
    public class PhotoVoteRepositoryShould
    {
        private ZazzDbContext _context;
        private PhotoRepository _repo;

        [SetUp]
        public void Init()
        {
            _context = new ZazzDbContext(true);
            _repo = new PhotoRepository(_context);
        }
    }
}