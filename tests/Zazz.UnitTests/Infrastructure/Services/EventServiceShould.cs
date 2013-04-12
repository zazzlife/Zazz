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

        [SetUp]
        public void Init()
        {
            _uow = new Mock<IUoW>();
            _sut = new EventService(_uow.Object);
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
        public async Task CallGetById_OnGetEvent()
        {
            //Arrange
            var id = 123;
            var zazzEvent = new ZazzEvent();
            _uow.Setup(x => x.EventRepository.GetById(id))
                .Returns(zazzEvent);

            //Act
            var result = await _sut.GetEventAsync(id);

            //Assert
            Assert.AreSame(zazzEvent, result);
            _uow.Verify(x => x.EventRepository.GetById(id), Times.Once());
        }

        [Test]
        public async Task ThrownIfEventIdIs0_OnUpdateEvent()
        {
            //Arrange
            //Act
            try
            {
                await _sut.UpdateEventAsync(_zazzEvent, _userId);
                Assert.Fail("Expected exception was not thrown");
            }
            catch (ArgumentException)
            {
            }

        }

        [Test]
        public async Task ThrowIfCurrentUserDoesntMatchTheOwner_OnUpdateEvent()
        {
            //Arrange
            _zazzEvent.Id = 444;
            _uow.Setup(x => x.EventRepository.GetOwnerId(_zazzEvent.Id))
                .Returns(() => 123);

            //Act

            try
            {
                await _sut.UpdateEventAsync(_zazzEvent, _userId);
                Assert.Fail("Expected exception was not thrown");
            }
            catch (SecurityException)
            {
            }

            //Assert
            _uow.Verify(x => x.EventRepository.GetOwnerId(_zazzEvent.Id), Times.Once());
        }

        [Test]
        public async Task SaveUpdatedEvent_OnUpdateEvent()
        {
            //Arrange
            _zazzEvent.Id = 444;
            _uow.Setup(x => x.EventRepository.GetOwnerId(_zazzEvent.Id))
                .Returns(() => _zazzEvent.UserId);

            //Act
            await _sut.UpdateEventAsync(_zazzEvent, _userId);

            //Assert
            _uow.Verify(x => x.EventRepository.InsertOrUpdate(_zazzEvent), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Once());

        }

        [Test]
        public async Task ShouldThrowIfEventIdIs0_OnDelete()
        {
            //Arrange
            _uow.Setup(x => x.EventRepository.GetOwnerId(_zazzEvent.Id))
                .Returns(() => 123);

            //Act
            try
            {
                await _sut.DeleteEventAsync(0, _zazzEvent.UserId);
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
        public async Task ThrowIfUserIdDoesntMatchTheOwnerId_OnDelete()
        {
            //Arrange
            _zazzEvent.Id = 444;
            _uow.Setup(x => x.EventRepository.GetOwnerId(_zazzEvent.Id))
                .Returns(() => 123);

            //Act

            try
            {
                await _sut.DeleteEventAsync(_zazzEvent.Id, _zazzEvent.UserId);
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
        public async Task Delete_OnDelete()
        {
            //Arrange
            _zazzEvent.Id = 444;
            _uow.Setup(x => x.EventRepository.GetOwnerId(_zazzEvent.Id))
                .Returns(() => _zazzEvent.UserId);
            _uow.Setup(x => x.EventRepository.Remove(_zazzEvent.Id));
            _uow.Setup(x => x.FeedRepository.RemoveEventFeeds(_zazzEvent.Id));
            _uow.Setup(x => x.CommentRepository.RemoveEventComments(_zazzEvent.Id));

            //Act
            await _sut.DeleteEventAsync(_zazzEvent.Id, _userId);

            //Assert
            _uow.Verify(x => x.EventRepository.Remove(_zazzEvent.Id), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Once());
            _uow.Verify(x => x.FeedRepository.RemoveEventFeeds(_zazzEvent.Id), Times.Once());
            _uow.Verify(x => x.CommentRepository.RemoveEventComments(_zazzEvent.Id), Times.Once());
        }


    }
}