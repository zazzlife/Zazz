using NUnit.Framework;
using Zazz.Data;
using Zazz.Data.Repositories;

namespace Zazz.IntegrationTests.Repositories
{
    [TestFixture]
    public class NotificationRepositoryShould
    {
        private ZazzDbContext _context;
        private NotificationRepository _repo;

        [SetUp]
        public void Init()
        {
            _context = new ZazzDbContext(true);
            _repo = new NotificationRepository(_context);
        }


    }
}