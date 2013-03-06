using System;
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


    }
}