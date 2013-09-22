using System.Linq;
using NUnit.Framework;
using Zazz.Core.Models.Data;
using Zazz.Data;
using Zazz.Data.Repositories;

namespace Zazz.IntegrationTests.Repositories
{
    [TestFixture]
    public class PhotoLikeRepositoryShould
    {
        private ZazzDbContext _context;
        private PhotoLikeRepository _repo;
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
            _repo = new PhotoLikeRepository(_context);

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
        public void ReturnCorrectRows_OnGetReceivedLikes()
        {
            //Arrange
            //photo 1: 3 likes
            //photo 2: 1 likes

            //photo 3: 1 like
            //photo 4: 2 likes

            var likes = new[]
                        {
                            new PhotoLike
                            {
                                PhotoId = _photo1.Id,
                                UserId = _user1.Id
                            },
                            new PhotoLike
                            {
                                PhotoId = _photo1.Id,
                                UserId = _user2.Id
                            },
                            new PhotoLike
                            {
                                PhotoId = _photo3.Id,
                                UserId = _user2.Id
                            },
                            new PhotoLike
                            {
                                PhotoId = _photo4.Id,
                                UserId = _user1.Id
                            },
                            new PhotoLike
                            {
                                PhotoId = _photo4.Id,
                                UserId = _user3.Id
                            },
                            new PhotoLike
                            {
                                PhotoId = _photo1.Id,
                                UserId = _user3.Id
                            },
                            new PhotoLike
                            {
                                PhotoId = _photo2.Id,
                                UserId = _user3.Id
                            }
                        };

            foreach (var l in likes)
            {
                _context.PhotoLikes.Add(l);
            }

            _context.SaveChanges();

            //Act
            var user1Result = _repo.GetUserReceivedLikes(_user1.Id).Count();
            var user2Result = _repo.GetUserReceivedLikes(_user2.Id).Count();
            var user3Result = _repo.GetUserReceivedLikes(_user3.Id).Count();

            //Assert
            Assert.AreEqual(4, user1Result);
            Assert.AreEqual(3, user2Result);
            Assert.AreEqual(0, user3Result);
        }

        [Test]
        public void InsertRecord_OnInsertGraph()
        {
            //Arrange
            Assert.IsFalse(_context.PhotoLikes.Any(v => v.PhotoId == _photo1.Id && v.UserId == _user1.Id),
                "A like record should not exists.");

            var like = new PhotoLike
                       {
                           PhotoId = _photo1.Id,
                           UserId = _user1.Id
                       };

            //Act
            _repo.InsertGraph(like);
            _context.SaveChanges();

            //Assert
            Assert.IsTrue(_context.PhotoLikes.Any(v => v.PhotoId == _photo1.Id && v.UserId == _user1.Id));
        }

        [Test]
        public void ReturnFalseIfLikeDoesNotExists_OnExists()
        {
            //Arrange
            Assert.IsFalse(_context.PhotoLikes.Any(v => v.PhotoId == _photo1.Id && v.UserId == _user1.Id));

            //Act
            var result = _repo.Exists(_photo1.Id, _user1.Id);

            //Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void ReturnTrueIfLikeExists_OnExists()
        {
            //Arrange
            var like = new PhotoLike
                       {
                           PhotoId = _photo1.Id,
                           UserId = _user1.Id
                       };

            _context.PhotoLikes.Add(like);
            _context.SaveChanges();

            Assert.IsTrue(_context.PhotoLikes.Any(v => v.PhotoId == _photo1.Id && v.UserId == _user1.Id));

            //Act
            var result = _repo.Exists(_photo1.Id, _user1.Id);

            //Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void ReturnCorrectNumber_OnGetPhotoLikesCount()
        {
            //Arrange
            // photo1: 1 like
            // photo2: 2 likes
            // photo3: 3 likes
            // photo3: 0 likes

            var p1v1 = new PhotoLike { PhotoId = _photo1.Id, UserId = _user1.Id };

            var p2v1 = new PhotoLike { PhotoId = _photo2.Id, UserId = _user1.Id };
            var p2v2 = new PhotoLike { PhotoId = _photo2.Id, UserId = _user2.Id };

            var p3v1 = new PhotoLike { PhotoId = _photo3.Id, UserId = _user1.Id };
            var p3v2 = new PhotoLike { PhotoId = _photo3.Id, UserId = _user2.Id };
            var p3v3 = new PhotoLike { PhotoId = _photo3.Id, UserId = _user3.Id };

            _context.PhotoLikes.Add(p1v1);

            _context.PhotoLikes.Add(p2v1);
            _context.PhotoLikes.Add(p2v2);

            _context.PhotoLikes.Add(p3v1);
            _context.PhotoLikes.Add(p3v2);
            _context.PhotoLikes.Add(p3v3);

            _context.SaveChanges();

            //Act
            var p1Result = _repo.GetLikesCount(_photo1.Id);
            var p2Result = _repo.GetLikesCount(_photo2.Id);
            var p3Result = _repo.GetLikesCount(_photo3.Id);
            var p4Result = _repo.GetLikesCount(_photo4.Id);

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
            var like = new PhotoLike
                       {
                           PhotoId = _photo1.Id,
                           UserId = _user1.Id
                       };

            _context.PhotoLikes.Add(like);
            _context.SaveChanges();

            Assert.IsTrue(_context.PhotoLikes.Any(v => v.PhotoId == _photo1.Id && v.UserId == _user1.Id));

            //Act
            _repo.Remove(like);
            _context.SaveChanges();

            //Assert
            Assert.IsFalse(_context.PhotoLikes.Any(v => v.PhotoId == _photo1.Id && v.UserId == _user1.Id));
        }

        [Test]
        public void Remove_OnRemoveById()
        {
            //Arrange
            var like = new PhotoLike
            {
                PhotoId = _photo1.Id,
                UserId = _user1.Id
            };

            _context.PhotoLikes.Add(like);
            _context.SaveChanges();

            Assert.IsTrue(_context.PhotoLikes.Any(v => v.PhotoId == _photo1.Id && v.UserId == _user1.Id));

            //Act
            _repo.Remove(like.PhotoId, like.UserId);
            _context.SaveChanges();

            //Assert
            Assert.IsFalse(_context.PhotoLikes.Any(v => v.PhotoId == _photo1.Id && v.UserId == _user1.Id));
        }
    }
}