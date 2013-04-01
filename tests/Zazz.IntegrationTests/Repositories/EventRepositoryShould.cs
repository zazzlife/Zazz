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
            var eventACheck = _repo.GetByIdAsync(eventA.Id).Result;
            var eventBCheck = _repo.GetByIdAsync(eventB.Id).Result;
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
            var result = _repo.GetPageEventIds(page.Id).ToList();

            //Assert
            Assert.AreEqual(2, result.Count);

            CollectionAssert.Contains(result, e1.Id);
            CollectionAssert.Contains(result, e2.Id);
        }
    }
}
