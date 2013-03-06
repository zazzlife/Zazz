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
    public class UserEventServiceShould
    {
        private Mock<IUoW> _uow;
        private UserEventService _sut;
        private int _userId;
        private UserEvent _userEvent;

        [SetUp]
        public void Init()
        {
            _uow = new Mock<IUoW>();
            _sut = new UserEventService(_uow.Object);
            _userId = 21;
            _userEvent = new UserEvent {UserId = _userId};

            _uow.Setup(x => x.SaveAsync())
                .Returns(() => Task.Run(() => { }));
        }

        [Test]
        public async Task ThrowExceptionWhenUserIdIs0_OnCreateEvent()
        {
            //Arrange
            _userEvent.UserId = 0;
            //Act
            try
            {
                await _sut.CreateEventAsync(_userEvent);
                Assert.Fail("Expected exception was not thrown");
            }
            catch (ArgumentException e)
            {
                //Assert
            }
        }

        [Test]
        public async Task InsertAndSave_OnCreateEvent()
        {
            //Arrange
            _uow.Setup(x => x.UserEventRepository.InsertGraph(_userEvent));

            //Act
            await _sut.CreateEventAsync(_userEvent);

            //Assert
            _uow.Verify(x => x.UserEventRepository.InsertGraph(_userEvent), Times.Once());
            _uow.Verify(x => x.SaveAsync());
        }

        [Test]
        public async Task ThrownIfEventIdIs0_OnUpdateEvent()
        {
            //Arrange
            //Act
            try
            {
                await _sut.UpdateEventAsync(_userEvent, _userId);
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
            _userEvent.Id = 444;
            _uow.Setup(x => x.UserEventRepository.GetOwnerIdAsync(_userEvent.Id))
                .Returns(() => Task.Run(() => 123));

            //Act

            try
            {
                await _sut.UpdateEventAsync(_userEvent, _userId);
                Assert.Fail("Expected exception was not thrown");
            }
            catch (SecurityException)
            {
            }

            //Assert
            _uow.Verify(x => x.UserEventRepository.GetOwnerIdAsync(_userEvent.Id), Times.Once());
        }

        [Test]
        public async Task SaveUpdatedEvent_OnUpdateEvent()
        {
            //Arrange
            _userEvent.Id = 444;
            _uow.Setup(x => x.UserEventRepository.GetOwnerIdAsync(_userEvent.Id))
                .Returns(() => Task.Run(() => _userEvent.UserId));

            //Act
            await _sut.UpdateEventAsync(_userEvent, _userId);

            //Assert
            _uow.Verify(x => x.UserEventRepository.InsertOrUpdate(_userEvent), Times.Once());
            _uow.Verify(x => x.SaveAsync(), Times.Once());

        }

        [Test]
        public async Task ShouldThrowIfEventIdIs0_OnDelete()
        {
            //Arrange
            _uow.Setup(x => x.UserEventRepository.GetOwnerIdAsync(_userEvent.Id))
                .Returns(() => Task.Run(() => 123));

            //Act
            try
            {
                await _sut.DeleteEventAsync(0, _userEvent.UserId);
                Assert.Fail("Expected exception was not thrown");
            }
            catch (ArgumentException)
            {
            }

            //Assert

        }

        [Test]
        public async Task ThrowIfUserIdDoesntMatchTheOwnerId_OnDelete()
        {
            //Arrange
            _userEvent.Id = 444;
            _uow.Setup(x => x.UserEventRepository.GetOwnerIdAsync(_userEvent.Id))
                .Returns(() => Task.Run(() => 123));

            //Act

            try
            {
                await _sut.DeleteEventAsync(_userEvent.Id, _userEvent.UserId);
                Assert.Fail("Expected exception was not thrown");
            }
            catch (SecurityException)
            {
            }

            //Assert
            _uow.Verify(x => x.UserEventRepository.GetOwnerIdAsync(_userEvent.Id), Times.Once());
        }

        [Test]
        public async Task Delete_OnDelete()
        {
            //Arrange
            _userEvent.Id = 444;
            _uow.Setup(x => x.UserEventRepository.GetOwnerIdAsync(_userEvent.Id))
                .Returns(() => Task.Run(() =>_userEvent.UserId));
            _uow.Setup(x => x.UserEventRepository.RemoveAsync(_userEvent.Id))
                .Returns(() => Task.Run(() => { }));

            //Act
            await _sut.DeleteEventAsync(_userEvent.Id, _userId);

            //Assert
            _uow.Verify(x => x.UserEventRepository.RemoveAsync(_userEvent.Id), Times.Once());
            _uow.Verify(x => x.SaveAsync(), Times.Once());
        }


    }
}