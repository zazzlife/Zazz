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
        public void ReturnCorrectEvents_OnGetUserEvents()
        {
            //Arrange
            var user2 = Mother.GetUser();
            _context.Users.Add(user2);

            var user1Event = Mother.GetEvent(_user.Id);

            var event1 = Mother.GetEvent(user2.Id);
            var event2 = Mother.GetEvent(user2.Id);
            var event3 = Mother.GetEvent(user2.Id);

            _context.Events.Add(user1Event);
            _context.Events.Add(event1);
            _context.Events.Add(event2);
            _context.Events.Add(event3);
            _context.SaveChanges();

            //Act
            var result = _repo.GetUserEvents(user2.Id, Int32.MaxValue).ToList();

            //Assert
            Assert.AreEqual(3, result.Count);
            Assert.IsTrue(result.Any(e => e.Id == event1.Id));
            Assert.IsTrue(result.Any(e => e.Id == event2.Id));
            Assert.IsTrue(result.Any(e => e.Id == event3.Id));
        }

        [Test]
        public void ReturnLatestEventsFirst_OnGetUserEvents()
        {
            //Arrange
            var user2 = Mother.GetUser();
            _context.Users.Add(user2);

            var user1Event = Mother.GetEvent(_user.Id);

            var event1 = Mother.GetEvent(user2.Id);
            event1.CreatedDate = DateTime.UtcNow.AddDays(-1);

            var event2 = Mother.GetEvent(user2.Id);
            event2.CreatedDate = DateTime.UtcNow.AddHours(-1);

            var event3 = Mother.GetEvent(user2.Id);

            _context.Events.Add(user1Event);
            _context.Events.Add(event1);
            _context.Events.Add(event2);
            _context.Events.Add(event3);
            _context.SaveChanges();

            //Act
            var result = _repo.GetUserEvents(user2.Id, Int32.MaxValue).ToList();

            //Assert
            Assert.AreEqual(event3.Id, result[0].Id);
            Assert.AreEqual(event2.Id, result[1].Id);
            Assert.AreEqual(event1.Id, result[2].Id);
        }

        [Test]
        public void SkipEventsIfProvided_OnGetUserEvents()
        {
            //Arrange
            var user2 = Mother.GetUser();
            _context.Users.Add(user2);

            var user1Event = Mother.GetEvent(_user.Id);

            var event1 = Mother.GetEvent(user2.Id);
            var event2 = Mother.GetEvent(user2.Id);
            var event3 = Mother.GetEvent(user2.Id);
            var event4 = Mother.GetEvent(user2.Id);
            var event5 = Mother.GetEvent(user2.Id);

            _context.Events.Add(user1Event);
            _context.Events.Add(event1);
            _context.Events.Add(event2);
            _context.Events.Add(event3);
            _context.Events.Add(event4);
            _context.Events.Add(event5);
            _context.SaveChanges();

            //Act
            var result = _repo.GetUserEvents(user2.Id, Int32.MaxValue, lastEventId: event4.Id).ToList();

            //Assert
            Assert.AreEqual(3, result.Count);
            Assert.IsTrue(result.Any(e => e.Id == event1.Id));
            Assert.IsTrue(result.Any(e => e.Id == event2.Id));
            Assert.IsTrue(result.Any(e => e.Id == event3.Id));
        }

        [Test]
        public void ReturnCorrectUserId_OnGetOwner()
        {
            //Arrange
            //Act
            var result = _repo.GetOwnerId(_zazzEvent.Id);

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
            var result = _repo.GetEventRange(@from.Date, to.Date, null, null).ToList();

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
            var eventACheck = _repo.GetById(eventA.Id);
            var eventBCheck = _repo.GetById(eventB.Id);
            Assert.IsNull(eventACheck.PhotoId);
            Assert.IsNull(eventBCheck.PhotoId);
        }

        [Test]
        public void GetCorrectEvent_OnGetByFacebookId()
        {
            //Arrange
            var facebookEventId = 1234L;

            var user = Mother.GetUser();
            _context.Users.Add(user);
            _context.SaveChanges();

            var e1 = Mother.GetEvent();
            e1.FacebookEventId = facebookEventId;
            e1.UserId = user.Id;

            var e2 = Mother.GetEvent();
            e2.UserId = user.Id;

            _context.Events.Add(e1);
            _context.Events.Add(e2);
            _context.SaveChanges();

            //Act
            var result = _repo.GetByFacebookId(facebookEventId);

            //Assert
            Assert.AreEqual(e1.Id, result.Id);
        }

        [Test]
        public void ReturnCorrectIds_OnGetEventIdsByPageId()
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

            var e1 = new ZazzEvent
                     {
                         UserId = user1.Id,
                         CreatedDate = DateTime.Now,
                         Name = "name",
                         PageId = page.Id,
                         Time = DateTime.UtcNow,
                         TimeUtc = DateTime.UtcNow,
                         Description = "Desc"
                     };

            var e2 = new ZazzEvent
            {
                UserId = user1.Id,
                CreatedDate = DateTime.Now,
                Name = "name",
                PageId = page.Id,
                Time = DateTime.UtcNow,
                TimeUtc = DateTime.UtcNow,
                Description = "Desc"
            };

            var e3 = new ZazzEvent
            {
                UserId = user1.Id,
                CreatedDate = DateTime.Now,
                Name = "name",
                Time = DateTime.UtcNow,
                TimeUtc = DateTime.UtcNow,
                Description = "Desc"
            };

            var e4 = new ZazzEvent
            {
                UserId = user2.Id,
                CreatedDate = DateTime.Now,
                Name = "name",
                Time = DateTime.UtcNow,
                TimeUtc = DateTime.UtcNow,
                Description = "Desc"
            };

            _context.Events.Add(e1);
            _context.Events.Add(e2);
            _context.Events.Add(e3);
            _context.Events.Add(e4);
            _context.SaveChanges();

            //Act
            var result = _repo.GetPageEvents(page.Id).ToList();

            //Assert
            Assert.AreEqual(2, result.Count);

            Assert.IsTrue(result.Any(e => e.Id == e1.Id));
            Assert.IsTrue(result.Any(e => e.Id == e2.Id));
        }

        [Test]
        public void ReturnCorrectCount_OnGetUpcomingEventsCount()
        {
            //Arrange
            var user = Mother.GetUser();
            var user2 = Mother.GetUser();

            _context.Users.Add(user);
            _context.Users.Add(user2);
            _context.SaveChanges();

            var event1 = Mother.GetEvent(user.Id);
            event1.TimeUtc = DateTime.UtcNow;

            var event2 = Mother.GetEvent(user.Id);
            event2.TimeUtc = DateTime.UtcNow.AddDays(1);

            var event3 = Mother.GetEvent(user.Id);
            event3.TimeUtc = DateTime.UtcNow.AddDays(-1);

            var event4 = Mother.GetEvent(user2.Id);
            event4.TimeUtc = DateTime.UtcNow.AddDays(1);

            _context.Events.Add(event1);
            _context.Events.Add(event2);
            _context.Events.Add(event3);
            _context.Events.Add(event4);
            _context.SaveChanges();

            Assert.IsTrue(_context.Events.Count() >= 4);

            //Act
            var result = _repo.GetUpcomingEventsCount(user.Id);

            //Assert
            Assert.AreEqual(2, result);
        }
    }
}
