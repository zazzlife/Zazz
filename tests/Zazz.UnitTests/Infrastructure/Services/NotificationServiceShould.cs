using Moq;
using NUnit.Framework;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Infrastructure.Services;

namespace Zazz.UnitTests.Infrastructure.Services
{
    [TestFixture]
    public class NotificationServiceShould
    {
        private Mock<IUoW> _uow;
        private NotificationService _sut;
        private Notification _notification;

        [SetUp]
        public void Init()
        {
            _uow = new Mock<IUoW>();
            _sut = new NotificationService(_uow.Object);

            _uow.Setup(x => x.SaveChanges());

            _notification = new Notification();
        }

        [Test]
        public void CreateNotificationAndSaveIfSaveIsNotProvided_OnCreateNotification()
        {
            //Arrange
            _uow.Setup(x => x.NotificationRepository.InsertGraph(_notification));

            //Act
            _sut.CreateNotification(_notification);

            //Assert
            _uow.Verify(x => x.NotificationRepository.InsertGraph(_notification), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Once());
        }

        [Test]
        public void CreateNotificationAndNotSaveIfSaveIsFalse_OnCreateNotification()
        {
            //Arrange
            _uow.Setup(x => x.NotificationRepository.InsertGraph(_notification));

            //Act
            _sut.CreateNotification(_notification, false);

            //Assert
            _uow.Verify(x => x.NotificationRepository.InsertGraph(_notification), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Never());
        }
    }
}