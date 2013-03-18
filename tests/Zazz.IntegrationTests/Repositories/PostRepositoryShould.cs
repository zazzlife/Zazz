using System;
using System.Linq;
using NUnit.Framework;
using Zazz.Core.Models.Data;
using Zazz.Data;
using Zazz.Data.Repositories;

namespace Zazz.IntegrationTests.Repositories
{
    [TestFixture]
    public class PostRepositoryShould
    {
        private ZazzDbContext _context;
        private PostRepository _repo;
        private User _user;
        private Post _post;

        [SetUp]
        public void Init()
        {
            _context = new ZazzDbContext(true);
            _repo = new PostRepository(_context);

            _user = Mother.GetUser();
            _context.Users.Add(_user);

            _context.SaveChanges();

            _post = Mother.GetPost();
            _post.UserId = _user.Id;

            _context.Posts.Add(_post);

            _context.SaveChanges();
        }

        [Test]
        public void ReturnCorrectUserId_OnGetOwner()
        {
            //Arrange
            //Act
            var result = _repo.GetOwnerIdAsync(_post.Id).Result;

            //Assert
            Assert.AreEqual(_user.Id, result);
        }

        [Test]
        public void GetCorrectEvents_OnGetEventRange()
        {
            //Arrange
            var user = Mother.GetUser();
            _context.Users.Add(user);
            _context.SaveChanges();

            var postA = Mother.GetPost();
            postA.UserId = user.Id;
            postA.IsEvent = true;
            postA.EventDetail = new EventDetail
                                {
                                    Time = DateTime.UtcNow,
                                    TimeUtc = DateTime.UtcNow.AddDays(2)
                                };

            var postB = Mother.GetPost();
            postB.UserId = user.Id;
            postB.IsEvent = true;
            postB.EventDetail = new EventDetail
            {
                Time = DateTime.UtcNow,
                TimeUtc = DateTime.UtcNow
            };

            var postC = Mother.GetPost();
            postC.UserId = user.Id;
            postC.IsEvent = true;
            postC.EventDetail = new EventDetail
            {
                Time = DateTime.UtcNow,
                TimeUtc = DateTime.UtcNow.AddDays(-2)
            };

            _context.Posts.Add(postA);
            _context.Posts.Add(postB);
            _context.Posts.Add(postC);
            _context.SaveChanges();

            var from = DateTime.UtcNow;
            var to = DateTime.UtcNow.AddDays(2);

            //Act
            var result = _repo.GetEventRange(from.Date, to.Date).ToList();

            //Assert
            CollectionAssert.Contains(result, postA);
            CollectionAssert.Contains(result, postB);
            CollectionAssert.DoesNotContain(result, postC);
        }

        [Test]
        public void ResetPhotoIdOnAllPosts_OnResetPhotoId()
        {
            //Arrange
            var photoId = 123;
            var user = Mother.GetUser();
            _context.Users.Add(user);
            _context.SaveChanges();

            var postA = Mother.GetPost();
            postA.PhotoId = photoId;
            postA.UserId = user.Id;
            postA.IsEvent = true;

            var postB = Mother.GetPost();
            postB.PhotoId = photoId;
            postB.UserId = user.Id;
            postB.IsEvent = true;

            _context.Posts.Add(postA);
            _context.Posts.Add(postB);
            _context.SaveChanges();

            //Act
            _repo.ResetPhotoId(photoId);

            //Assert
            var postACheck = _repo.GetByIdAsync(postA.Id).Result;
            var postBCheck = _repo.GetByIdAsync(postB.Id).Result;
            Assert.IsNull(postACheck.PhotoId);
            Assert.IsNull(postBCheck.PhotoId);
        }
    }
}
