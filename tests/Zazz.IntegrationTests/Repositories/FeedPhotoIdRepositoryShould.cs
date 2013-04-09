using NUnit.Framework;
using Zazz.Data;
using Zazz.Data.Repositories;

namespace Zazz.IntegrationTests.Repositories
{
    [TestFixture]
    public class FeedPhotoIdRepositoryShould
    {
        private ZazzDbContext _context;
        private FeedPhotoIdRepository _repo;

        [SetUp]
        public void Init()
        {
            _context = new ZazzDbContext(true);
            _repo = new FeedPhotoIdRepository(_context);
        }
    }
}