using System.Linq;
using NUnit.Framework;
using Zazz.Core.Models.Data;
using Zazz.Data;
using Zazz.Data.Repositories;

namespace Zazz.IntegrationTests.Repositories
{
    [TestFixture]
    public class PhotoVoteRepositoryShould
    {
        private ZazzDbContext _context;
        private PhotoVoteRepository _repo;
        private User _user1;
        private User _user2;
        private Photo _photo1;
        private Photo _photo2;
        private Photo _photo3;
        private Photo _photo4;

        [SetUp]
        public void Init()
        {
            _context = new ZazzDbContext(true);
            _repo = new PhotoVoteRepository(_context);

            _user1 = Mother.GetUser();
            _user2 = Mother.GetUser();

            _context.Users.Add(_user1);
            _context.Users.Add(_user2);
            _context.SaveChanges();

            _photo1 = Mother.GetPhoto(_user1.Id);
            _photo2 = Mother.GetPhoto(_user1.Id);
            _photo3 = Mother.GetPhoto(_user2.Id);
            _photo4 = Mother.GetPhoto(_user2.Id);

            _context.Photos.Add(_photo1);
            _context.Photos.Add(_photo2);
            _context.Photos.Add(_photo3);
            _context.Photos.Add(_photo4);

            _context.SaveChanges();
        }

        [Test]
        public void InsertRecord_OnInsertGraph()
        {
            //Arrange
            Assert.IsFalse(_context.PhotoVotes.Any(v => v.PhotoId == _photo1.Id && v.UserId == _user1.Id),
                "A vote record should not exists.");

            var vote = new PhotoVote
                       {
                           PhotoId = _photo1.Id,
                           UserId = _user1.Id
                       };

            //Act
            _repo.InsertGraph(vote);
            _context.SaveChanges();

            //Assert
            Assert.IsTrue(_context.PhotoVotes.Any(v => v.PhotoId == _photo1.Id && v.UserId == _user1.Id));
        }


    }
}