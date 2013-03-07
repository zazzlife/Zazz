using System;
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


    }
}