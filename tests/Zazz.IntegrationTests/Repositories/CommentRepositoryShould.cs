using System;
using System.Linq;
using NUnit.Framework;
using Zazz.Core.Models.Data;
using Zazz.Data;
using Zazz.Data.Repositories;

namespace Zazz.IntegrationTests.Repositories
{
    [TestFixture]
    public class CommentRepositoryShould
    {
        private ZazzDbContext _context;
        private CommentRepository _repo;
        private Post _event1;
        private Post _event2;
        private User _user;

        [SetUp]
        public void Init()
        {
            _context = new ZazzDbContext(true);
            _repo = new CommentRepository(_context);

            _user = Mother.GetUser();
            _context.Users.Add(_user);
            _context.SaveChanges();

            _event1 = Mother.GetPost();
            _event1.UserId = _user.Id;

            _event2 = Mother.GetPost();
            _event2.UserId = _user.Id;

            _context.Posts.Add(_event1);
            _context.Posts.Add(_event2);
            _context.SaveChanges();
        }

        [Test]
        public void GetCommentsCorrectly()
        {
            //Arrange
            var e1Comment1 = new Comment
                                 {
                                     Date = DateTime.Now,
                                     FromId = _user.Id,
                                     Message = "m",
                                     PostId = _event1.Id
                                 };
            var e1Comment2 = new Comment
            {
                Date = DateTime.Now,
                FromId = _user.Id,
                Message = "m",
                PostId = _event1.Id
            };

            var e2Comment1 = new Comment
            {
                Date = DateTime.Now,
                FromId = _user.Id,
                Message = "m",
                PostId = _event2.Id
            };

            _repo.InsertGraph(e1Comment1);
            _repo.InsertGraph(e1Comment2);
            _repo.InsertGraph(e2Comment1);

            _context.SaveChanges();

            //Act
            var result = _repo.GetCommentsAsync(_event1.Id).Result;

            //Assert
            Assert.AreEqual(2, result.Count());
        }


    }
}