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
        private PostRepository _repo;
        private ZazzDbContext _context;

        [SetUp]
        public void Init()
        {
            _context = new ZazzDbContext(true);
            _repo = new PostRepository(_context);
        }

        [Test]
        public void ReturnCorrectRow_OnGetByFbId()
        {
            //Arrange
            var user = Mother.GetUser();
            _context.Users.Add(user);
            _context.SaveChanges();

            var post = Mother.GetPost(user.Id);
            post.FacebookId = 5566;

            _context.Posts.Add(post);
            _context.SaveChanges();

            //Act
            var result = _repo.GetByFbId(post.FacebookId);

            //Assert
            Assert.AreEqual(post.Id, result.Id);
        }

        [Test]
        public void ReturnCorrectRecords_OnGetPagePostIds()
        {
            //Arrange
            var user1 = Mother.GetUser();
            var user2 = Mother.GetUser();
            _context.Users.Add(user1);
            _context.Users.Add(user2);
            _context.SaveChanges();

            var page = new FacebookPage
            {
                AccessToken = "asdf",
                FacebookId = "1234",
                Name = "name",
                UserId = user1.Id
            };

            _context.FacebookPages.Add(page);
            _context.SaveChanges();

            var p1 = new Post
                     {
                         CreatedTime = DateTime.UtcNow,
                         Message = "Dsadsad",
                         PageId = page.Id,
                         FromUserId = user1.Id
                     };

            var p2 = new Post
            {
                CreatedTime = DateTime.UtcNow,
                Message = "Dsadsad",
                PageId = page.Id,
                FromUserId = user1.Id
            };

            var p3 = new Post
            {
                CreatedTime = DateTime.UtcNow,
                Message = "Dsadsad",
                FromUserId = user1.Id
            };

            var p4 = new Post
            {
                CreatedTime = DateTime.UtcNow,
                Message = "Dsadsad",
                FromUserId = user2.Id
            };


            _context.Posts.Add(p1);
            _context.Posts.Add(p2);
            _context.Posts.Add(p3);
            _context.Posts.Add(p4);
            _context.SaveChanges();

            //Act
            var result = _repo.GetPagePosts(page.Id).ToList();

            //Assert
            Assert.AreEqual(2, result.Count);

            CollectionAssert.Contains(result, p1.Id);
            CollectionAssert.Contains(result, p2.Id);
        }


    }
}