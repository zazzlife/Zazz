using System.Linq;
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
        private User _club2;
        private User _user2;
        private UserPoint _user1Club1Points;
        private UserPoint _user1Club2Points;
        private UserPoint _user2Club1Points;
        private UserPoint _user2Club2Points;

        [SetUp]
        public void Init()
        {
            _context = new ZazzDbContext(true);
            _repo = new UserPointRepository(_context);

            _user = Mother.GetUser();
            _user2 = Mother.GetUser();
            _club = Mother.GetUser();
            _club2 = Mother.GetUser();

            _context.Users.Add(_user);
            _context.Users.Add(_user2);
            _context.Users.Add(_club);
            _context.Users.Add(_club2);
            _context.SaveChanges();

            _user1Club1Points = new UserPoint { ClubId = _club.Id, Points = 11, UserId = _user.Id };
            _user1Club2Points = new UserPoint { ClubId = _club2.Id, Points = 12, UserId = _user.Id };
            _user2Club1Points = new UserPoint { ClubId = _club.Id, Points = 21, UserId = _user2.Id };
            _user2Club2Points = new UserPoint { ClubId = _club2.Id, Points = 22, UserId = _user2.Id };

            _context.UserPoints.Add(_user1Club1Points);
            _context.UserPoints.Add(_user1Club2Points);
            _context.UserPoints.Add(_user2Club1Points);
            _context.UserPoints.Add(_user2Club2Points);
            _context.SaveChanges();
        }

        [Test]
        public void GetAllRecords_OnGetAll()
        {
            //Arrange

            //Act
            var result = _repo.GetAll().ToList();

            //Assert
            Assert.AreEqual(4, result.Count);
        }
    }
}