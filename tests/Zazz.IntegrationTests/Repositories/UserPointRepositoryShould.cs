using NUnit.Framework;
using Zazz.Core.Models.Data;
using Zazz.Data;
using Zazz.Data.Repositories;

namespace Zazz.IntegrationTests.Repositories
{
    [TestFixture]
    public class UserPointRepositoryShould
    {
        private ZazzDbContext _context;
        private UserPointRepository _repo;
        private User _user;
        private User _club;

        [SetUp]
        public void Init()
        {
            _context = new ZazzDbContext(true);
            _repo = new UserPointRepository(_context);

            _user = Mother.GetUser();
            _club = Mother.GetUser();

            _context.Users.Add(_user);
            _context.Users.Add(_club);
            _context.SaveChanges();
        }
    }
}