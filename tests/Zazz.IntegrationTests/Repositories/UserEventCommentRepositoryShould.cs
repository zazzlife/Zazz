using System;
using System.Linq;
using NUnit.Framework;
using Zazz.Core.Models.Data;
using Zazz.Data;
using Zazz.Data.Repositories;

namespace Zazz.IntegrationTests.Repositories
{
    [TestFixture]
    public class UserEventCommentRepositoryShould
    {
        private ZazzDbContext _context;
        private UserEventCommentRepository _repo;
        private UserEvent _event1;
        private UserEvent _event2;
        private User _user;

        [SetUp]
        public void Init()
        {
            _context = new ZazzDbContext(true);
            _repo = new UserEventCommentRepository(_context);

            _user = Mother.GetUser();
            _context.Users.Add(_user);
            _context.SaveChanges();

            _event1 = Mother.GetUserEvent();
            _event1.UserId = _user.Id;

            _event2 = Mother.GetUserEvent();
            _event2.UserId = _user.Id;

            _context.UserEvents.Add(_event1);
            _context.UserEvents.Add(_event2);
            _context.SaveChanges();
        }

        [Test]
        public void GetEventCommentsCorrectly()
        {
            //Arrange
            var e1Comment1 = new UserEventComment
                                 {
                                     Date = DateTime.Now,
                                     FromId = _user.Id,
                                     Message = "m",
                                     UserEventId = _event1.Id
                                 };
            var e1Comment2 = new UserEventComment
            {
                Date = DateTime.Now,
                FromId = _user.Id,
                Message = "m",
                UserEventId = _event1.Id
            };

            var e2Comment1 = new UserEventComment
            {
                Date = DateTime.Now,
                FromId = _user.Id,
                Message = "m",
                UserEventId = _event2.Id
            };

            _repo.InsertGraph(e1Comment1);
            _repo.InsertGraph(e1Comment2);
            _repo.InsertGraph(e2Comment1);

            _context.SaveChanges();

            //Act
            var result = _repo.GetEventCommentsAsync(_event1.Id).Result;

            //Assert
            Assert.AreEqual(2, result.Count());
        }


    }
}