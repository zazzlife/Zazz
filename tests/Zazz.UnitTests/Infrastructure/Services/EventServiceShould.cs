using System;
using System.Security;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Infrastructure.Services;

namespace Zazz.UnitTests.Infrastructure.Services
{
    [TestFixture]
    public class EventServiceShould
    {
        private Mock<IUoW> _uow;
        private EventService _sut;
        private int _userId;
        private ZazzEvent _zazzEvent;
        private Mock<INotificationService> _notificationService;

        [SetUp]
        public void Init()
        {
            _uow = new Mock<IUoW>();
            _notificationService = new Mock<INotificationService>();
            var commentService = new Mock<ICommentService>();
            _sut = new EventService(_uow.Object, _notificationService.Object, commentService.Object);
            _userId = 21;
            _zazzEvent = new ZazzEvent {UserId = _userId};

            _uow.Setup(x => x.SaveChanges());
        }

        [Test]
        public void ThrowExceptionWhenUserIdIs0_OnCreateEvent()
        {
            //Arrange
            _zazzEvent.UserId = 0;
            //Act
            try
            {
                _sut.CreateEvent(_zazzEvent);
                Assert.Fail("Expected exception was not thrown");
            }
            catch (ArgumentException e)
            {
                //Assert
            }
        }

        [Test]
        public void InsertAndSaveAndCreateFeed_OnCreateEvent()
        {
            //Arrange
            _uow.Setup(x => x.EventRepository.InsertGraph(_zazzEvent));
            _uow.Setup(x => x.FeedRepository.InsertGraph(It.IsAny<Feed>()));

            //Act
            _sut.CreateEvent(_zazzEvent);

            //Assert
            _uow.Verify(x => x.EventRepository.InsertGraph(_zazzEvent), Times.Once());
            _uow.Verify(x => x.FeedRepository.InsertGraph(It.IsAny<Feed>()), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Exactly(2));
        }

        [Test]
        public void CallGetById_OnGetEvent()
        {
            //Arrange
            var id = 123;
            var zazzEvent = new ZazzEvent();
            _uow.Setup(x => x.EventRepository.GetById(id))
                .Returns(zazzEvent);

            //Act
            var result = _sut.GetEvent(id);

            //Assert
            Assert.AreSame(zazzEvent, result);
            _uow.Verify(x => x.EventRepository.GetById(id), Times.Once());
        }

        [Test]
        public void ThrownIfEventIdIs0_OnUpdateEvent()
        {
            //Arrange
            //Act
            try
            {
                _sut.UpdateEvent(_zazzEvent, _userId);
                Assert.Fail("Expected exception was not thrown");
            }
            catch (ArgumentException)
            {
            }

        }

        [Test]
        public void ThrowIfCurrentUserDoesntMatchTheOwner_OnUpdateEvent()
        {
            //Arrange
            _zazzEvent.Id = 444;
            _uow.Setup(x => x.EventRepository.GetOwnerId(_zazzEvent.Id))
                .Returns(() => 123);

            //Act

            try
            {
                _sut.UpdateEvent(_zazzEvent, _userId);
                Assert.Fail("Expected exception was not thrown");
            }
            catch (SecurityException)
            {
            }

            //Assert
            _uow.Verify(x => x.EventRepository.GetOwnerId(_zazzEvent.Id), Times.Once());
        }

        [Test]
        public void SaveUpdatedEvent_OnUpdateEvent()
        {
            //Arrange
            _zazzEvent.Id = 444;
            _uow.Setup(x => x.EventRepository.GetOwnerId(_zazzEvent.Id))
                .Returns(() => _zazzEvent.UserId);

            //Act
            _sut.UpdateEvent(_zazzEvent, _userId);

            //Assert
            _uow.Verify(x => x.EventRepository.InsertOrUpdate(_zazzEvent), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Once());

        }

        [Test]
        public void ShouldThrowIfEventIdIs0_OnDelete()
        {
            //Arrange
            _uow.Setup(x => x.EventRepository.GetOwnerId(_zazzEvent.Id))
                .Returns(() => 123);

            //Act
            try
            {
                _sut.DeleteEvent(0, _zazzEvent.UserId);
                Assert.Fail("Expected exception was not thrown");
            }
            catch (ArgumentException)
            {
            }

            //Assert
            _uow.Verify(x => x.EventRepository.GetOwnerId(_zazzEvent.Id), Times.Never());
            _uow.Verify(x => x.EventRepository.Remove(_zazzEvent.Id), Times.Never());
            _uow.Verify(x => x.SaveChanges(), Times.Never());
            _uow.Verify(x => x.FeedRepository.RemoveEventFeeds(_zazzEvent.Id), Times.Never());
            _uow.Verify(x => x.CommentRepository.RemoveEventComments(_zazzEvent.Id), Times.Never());
        }

        [Test]
        public void ThrowIfUserIdDoesntMatchTheOwnerId_OnDelete()
        {
            //Arrange
            _zazzEvent.Id = 444;
            _uow.Setup(x => x.EventRepository.GetOwnerId(_zazzEvent.Id))
                .Returns(() => 123);

            //Act

            try
            {
                _sut.DeleteEvent(_zazzEvent.Id, _zazzEvent.UserId);
                Assert.Fail("Expected exception was not thrown");
            }
            catch (SecurityException)
            {
            }

            //Assert
            _uow.Verify(x => x.EventRepository.GetOwnerId(_zazzEvent.Id), Times.Once());
            _uow.Verify(x => x.EventRepository.Remove(_zazzEvent.Id), Times.Never());
            _uow.Verify(x => x.SaveChanges(), Times.Never());
            _uow.Verify(x => x.FeedRepository.RemoveEventFeeds(_zazzEvent.Id), Times.Never());
            _uow.Verify(x => x.CommentRepository.RemoveEventComments(_zazzEvent.Id), Times.Never());
        }

        [Test]
        public void Delete_OnDelete()
        {
            //Arrange
            _zazzEvent.Id = 444;
            _uow.Setup(x => x.EventRepository.GetOwnerId(_zazzEvent.Id))
                .Returns(() => _zazzEvent.UserId);
            _uow.Setup(x => x.EventRepository.Remove(_zazzEvent.Id));
            _uow.Setup(x => x.FeedRepository.RemoveEventFeeds(_zazzEvent.Id));
            _uow.Setup(x => x.CommentRepository.RemoveEventComments(_zazzEvent.Id));

            //Act
            _sut.DeleteEvent(_zazzEvent.Id, _userId);

            //Assert
            _uow.Verify(x => x.EventRepository.Remove(_zazzEvent.Id), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Once());
            _uow.Verify(x => x.FeedRepository.RemoveEventFeeds(_zazzEvent.Id), Times.Once());
            _uow.Verify(x => x.CommentRepository.RemoveEventComments(_zazzEvent.Id), Times.Once());
        }

        [Test]
        public void CallRemoveNotifications_OnDelete()
        {
            //Arrange
            _zazzEvent.Id = 444;
            _uow.Setup(x => x.EventRepository.GetOwnerId(_zazzEvent.Id))
                .Returns(() => _zazzEvent.UserId);
            _uow.Setup(x => x.EventRepository.Remove(_zazzEvent.Id));
            _uow.Setup(x => x.FeedRepository.RemoveEventFeeds(_zazzEvent.Id));
            _uow.Setup(x => x.CommentRepository.RemoveEventComments(_zazzEvent.Id));
            _notificationService.Setup(x => x.RemoveEventNotifications(_zazzEvent.Id));

            //Act
            _sut.DeleteEvent(_zazzEvent.Id, _userId);

            //Assert
            _notificationService.Verify(x => x.RemoveEventNotifications(_zazzEvent.Id), Times.Once());
        }
    }
}