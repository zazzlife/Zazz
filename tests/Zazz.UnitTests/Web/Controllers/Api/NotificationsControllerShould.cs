using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Zazz.Core.Interfaces;
using Zazz.Core.Models;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;

namespace Zazz.UnitTests.Web.Controllers.Api
{
    [TestFixture]
    public class NotificationsControllerShould : BaseHMACTests
    {
        private Mock<INotificationService> _notificationService;
        private Notification _notification;

        public override void Init()
        {
            base.Init();

            ControllerAddress = "/api/v1/notifications";

            _notification = new Notification
                            {
                                Id = 32,
                                Time = DateTime.UtcNow,
                                UserId = User.Id,
                                NotificationType = NotificationType.FollowRequestAccepted
                            };

            _notificationService = MockRepo.Create<INotificationService>();

            IocContainer.Configure(x =>
            {
                x.For<INotificationService>().Use(_notificationService.Object);
            });
        }

        [Test]
        public async Task Return200_OnGet()
        {
            //Arrange
            var notifications = new List<Notification> { _notification };
            _notificationService.Setup(x => x.GetUserNotifications(User.Id, null))
                                .Returns(notifications.AsQueryable());

            UserService.Setup(x => x.GetUserDisplayName(It.IsAny<int>()))
                       .Returns("asdf");
            PhotoService.Setup(x => x.GetUserImageUrl(It.IsAny<int>()))
                        .Returns(new PhotoLinks(""));

            AddValidHMACHeaders("GET");
            SetupMocksForHMACAuth();

            //Act
            var response = await Client.GetAsync(ControllerAddress);

            //Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            MockRepo.VerifyAll();
        }

        [TestCase(0)]
        [TestCase(-1)]
        public async Task Return400ForBadId_OnDelete(int id)
        {
            //Arrange
            ControllerAddress += "/" + id;

            AddValidHMACHeaders("DELETE");
            SetupMocksForHMACAuth();

            //Act
            var response = await Client.DeleteAsync(ControllerAddress);

            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task Return403ForSecurityException_OnDelete()
        {
            //Arrange
            var notificationId = 66;
            ControllerAddress += "/" + notificationId;

            AddValidHMACHeaders("DELETE");
            SetupMocksForHMACAuth();

            _notificationService.Setup(x => x.Remove(notificationId, User.Id))
                                .Throws<SecurityException>();

            //Act
            var response = await Client.DeleteAsync(ControllerAddress);

            //Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task Return204OnSuccess_OnDelete()
        {
            //Arrange
            var notificationId = 66;
            ControllerAddress += "/" + notificationId;

            AddValidHMACHeaders("DELETE");
            SetupMocksForHMACAuth();

            _notificationService.Setup(x => x.Remove(notificationId, User.Id));

            //Act
            var response = await Client.DeleteAsync(ControllerAddress);

            //Assert
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
            MockRepo.VerifyAll();
        }
    }
}