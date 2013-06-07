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
    }
}