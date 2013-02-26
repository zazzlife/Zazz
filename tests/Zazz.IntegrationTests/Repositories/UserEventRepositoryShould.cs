using System;
using NUnit.Framework;
using Zazz.Core.Models.Data;
using Zazz.Data;
using Zazz.Data.Repositories;

namespace Zazz.IntegrationTests.Repositories
{
    [TestFixture]
    public class UserEventRepositoryShould
    {
        private ZazzDbContext _context;
        private UserEventRepository _repo;
        private User _user;
        private UserEvent _userEvent;

        [SetUp]
        public void Init()
        {
            _context = new ZazzDbContext(true);
            _repo = new UserEventRepository(_context);

            _user = Mother.GetUser();
            _context.Users.Add(_user);

            _context.SaveChanges();

            _userEvent = Mother.GetUserEvent();
            _userEvent.UserId = _user.Id;

            _context.UserEvents.Add(_userEvent);

            _context.SaveChanges();
        }

        [Test]
        public void ReturnCorrectUserId_OnGetOwner()
        {
            //Arrange
            //Act
            var result = _repo.GetOwnerIdAsync(_userEvent.Id).Result;

            //Assert
            Assert.AreEqual(_user.Id, result);
        }


    }
}