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
        private User _user3;

        [SetUp]
        public void Init()
        {
            _context = new ZazzDbContext(true);
            _repo = new PhotoVoteRepository(_context);

            _user1 = Mother.GetUser();
            _user2 = Mother.GetUser();
            _user3 = Mother.GetUser();

            _context.Users.Add(_user1);
            _context.Users.Add(_user2);
            _context.Users.Add(_user3);
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

        [Test]
        public void ReturnFalseIfVoteDoesNotExists_OnExists()
        {
            //Arrange
            Assert.IsFalse(_context.PhotoVotes.Any(v => v.PhotoId == _photo1.Id && v.UserId == _user1.Id));

            //Act
            var result = _repo.Exists(_photo1.Id, _user1.Id);

            //Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void ReturnTrueIfVoteExists_OnExists()
        {
            //Arrange
            var vote = new PhotoVote
                       {
                           PhotoId = _photo1.Id,
                           UserId = _user1.Id
                       };

            _context.PhotoVotes.Add(vote);
            _context.SaveChanges();

            Assert.IsTrue(_context.PhotoVotes.Any(v => v.PhotoId == _photo1.Id && v.UserId == _user1.Id));

            //Act
            var result = _repo.Exists(_photo1.Id, _user1.Id);

            //Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void ReturnCorrectNumber_OnGetPhotoVotesCount()
        {
            //Arrange
            // photo1: 1 vote
            // photo2: 2 votes
            // photo3: 3 votes
            // photo3: 0 votes

            var p1v1 = new PhotoVote { PhotoId = _photo1.Id, UserId = _user1.Id };

            var p2v1 = new PhotoVote { PhotoId = _photo2.Id, UserId = _user1.Id };
            var p2v2 = new PhotoVote { PhotoId = _photo2.Id, UserId = _user2.Id };

            var p3v1 = new PhotoVote { PhotoId = _photo3.Id, UserId = _user1.Id };
            var p3v2 = new PhotoVote { PhotoId = _photo3.Id, UserId = _user2.Id };
            var p3v3 = new PhotoVote { PhotoId = _photo3.Id, UserId = _user3.Id };

            _context.PhotoVotes.Add(p1v1);

            _context.PhotoVotes.Add(p2v1);
            _context.PhotoVotes.Add(p2v2);

            _context.PhotoVotes.Add(p3v1);
            _context.PhotoVotes.Add(p3v2);
            _context.PhotoVotes.Add(p3v3);

            _context.SaveChanges();

            //Act
            var p1Result = _repo.GetPhotoVotesCount(_photo1.Id);
            var p2Result = _repo.GetPhotoVotesCount(_photo2.Id);
            var p3Result = _repo.GetPhotoVotesCount(_photo3.Id);
            var p4Result = _repo.GetPhotoVotesCount(_photo4.Id);

            //Assert
            Assert.AreEqual(1, p1Result);
            Assert.AreEqual(2, p2Result);
            Assert.AreEqual(3, p3Result);
            Assert.AreEqual(0, p4Result);
        }

        [Test]
        public void Remove_OnRemoveByObject()
        {
            //Arrange
            var vote = new PhotoVote
                       {
                           PhotoId = _photo1.Id,
                           UserId = _user1.Id
                       };

            _context.PhotoVotes.Add(vote);
            _context.SaveChanges();

            Assert.IsTrue(_context.PhotoVotes.Any(v => v.PhotoId == _photo1.Id && v.UserId == _user1.Id));

            //Act
            _repo.Remove(vote);
            _context.SaveChanges();

            //Assert
            Assert.IsFalse(_context.PhotoVotes.Any(v => v.PhotoId == _photo1.Id && v.UserId == _user1.Id));
        }

        [Test]
        public void Remove_OnRemoveById()
        {
            //Arrange
            var vote = new PhotoVote
            {
                PhotoId = _photo1.Id,
                UserId = _user1.Id
            };

            _context.PhotoVotes.Add(vote);
            _context.SaveChanges();

            Assert.IsTrue(_context.PhotoVotes.Any(v => v.PhotoId == _photo1.Id && v.UserId == _user1.Id));

            //Act
            _repo.Remove(vote.PhotoId, vote.UserId);
            _context.SaveChanges();

            //Assert
            Assert.IsFalse(_context.PhotoVotes.Any(v => v.PhotoId == _photo1.Id && v.UserId == _user1.Id));
        }
    }
}