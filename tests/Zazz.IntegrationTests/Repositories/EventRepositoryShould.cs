using System;
using System.Linq;
using NUnit.Framework;
using Zazz.Core.Models.Data;
using Zazz.Data;
using Zazz.Data.Repositories;

namespace Zazz.IntegrationTests.Repositories
{
    [TestFixture]
    public class EventRepositoryShould
    {
        private ZazzDbContext _context;
        private EventRepository _repo;
        private User _user;
        private ZazzEvent _zazzEvent;

        [SetUp]
        public void Init()
        {
            _context = new ZazzDbContext(true);
            _repo = new EventRepository(_context);

            _user = Mother.GetUser();
            _context.Users.Add(_user);

            _context.SaveChanges();

            _zazzEvent = Mother.GetEvent();
            _zazzEvent.UserId = _user.Id;

            _context.Events.Add(_zazzEvent);

            _context.SaveChanges();
        }

        [Test]
        public void ReturnCorrectUserId_OnGetOwner()
        {
            //Arrange
            //Act
            var result = _repo.GetOwnerIdAsync(_zazzEvent.Id).Result;

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

            var eventA = Mother.GetEvent();
            eventA.UserId = user.Id;
            eventA.Time = DateTime.UtcNow;
            eventA.TimeUtc = DateTime.UtcNow.AddDays(2);

            var eventB = Mother.GetEvent();
            eventB.UserId = user.Id;
            eventB.Time = DateTime.UtcNow;
            eventB.TimeUtc = DateTime.UtcNow;

            var eventC = Mother.GetEvent();
            eventC.UserId = user.Id;
            eventC.Time = DateTime.UtcNow;
            eventC.TimeUtc = DateTime.UtcNow.AddDays(-2);

            _context.Events.Add(eventA);
            _context.Events.Add(eventB);
            _context.Events.Add(eventC);
            _context.SaveChanges();

            var from = DateTime.UtcNow;
            var to = DateTime.UtcNow.AddDays(2);

            //Act
            var result = _repo.GetEventRange(from.Date, to.Date).ToList();

            //Assert
            CollectionAssert.Contains(result, eventA);
            CollectionAssert.Contains(result, eventB);
            CollectionAssert.DoesNotContain(result, eventC);
        }

        [Test]
        public void ResetPhotoIdOnAllEvents_OnResetPhotoId()
        {
            //Arrange
            var photoId = 123;
            var user = Mother.GetUser();
            _context.Users.Add(user);
            _context.SaveChanges();

            var eventA = Mother.GetEvent();
            eventA.PhotoId = photoId;
            eventA.UserId = user.Id;

            var eventB = Mother.GetEvent();
            eventB.PhotoId = photoId;
            eventB.UserId = user.Id;

            _context.Events.Add(eventA);
            _context.Events.Add(eventB);
            _context.SaveChanges();

            //Act
            _repo.ResetPhotoId(photoId);

            //Assert
            var eventACheck = _repo.GetByIdAsync(eventA.Id).Result;
            var eventBCheck = _repo.GetByIdAsync(eventB.Id).Result;
            Assert.IsNull(eventACheck.PhotoId);
            Assert.IsNull(eventBCheck.PhotoId);
        }
    }
}
