using System;
using System.Collections.Generic;
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
    public class FollowServiceShould
    {
        private Mock<IUoW> _uow;
        private FollowService _sut;
        private int _userAId;
        private int _clubId;
        private int _userBId;
        private FollowRequest _followRequest;
        private Mock<INotificationService> _notificationService;

        [SetUp]
        public void Init()
        {
            _uow = new Mock<IUoW>();
            _notificationService = new Mock<INotificationService>();
            _sut = new FollowService(_uow.Object, _notificationService.Object);

            _userAId = 12;
            _userBId = 15;
            _clubId = 13;
            _followRequest = new FollowRequest {FromUserId = _userAId, ToUserId = _userBId};


            _uow.Setup(x => x.SaveChanges());
        }

        [Test]
        public void NotCreateFollowIfExists_OnFollowClubAdmin()
        {
            //Arrange
            _uow.Setup(x => x.FollowRepository.Exists(_userAId, _userBId))
                .Returns(true);
            _uow.Setup(x => x.FollowRepository.InsertGraph(It.IsAny<Follow>()));

            //Act
            _sut.FollowClubAdmin(_userAId, _userBId);

            //Assert
            _uow.Verify(x => x.FollowRepository.Exists(_userAId, _userBId), Times.Once());
            _uow.Verify(x => x.FollowRepository.InsertGraph(It.IsAny<Follow>()), Times.Never());
            _uow.Verify(x => x.SaveChanges(), Times.Never());
        }

        [Test]
        public void CreateFollowIfNotExists_OnFollowClubAdmin()
        {
            //Arrange
            _uow.Setup(x => x.FollowRepository.Exists(_userAId, _userBId))
                .Returns(false);
            _uow.Setup(x => x.FollowRepository.InsertGraph(It.IsAny<Follow>()));

            //Act
            _sut.FollowClubAdmin(_userAId, _userBId);

            //Assert
            _uow.Verify(x => x.FollowRepository.Exists(_userAId, _userBId), Times.Once());
            _uow.Verify(x => x.FollowRepository.InsertGraph(It.IsAny<Follow>()), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Once());
        }

        [Test]
        public void CheckIfRecordExists_OnSendFollowRequest()
        {
            //Arrange
            _uow.Setup(x => x.FollowRequestRepository.Exists(_userAId, _userBId))
                .Returns(true);
            _uow.Setup(x => x.FollowRequestRepository.InsertGraph(_followRequest));

            //Act
            _sut.SendFollowRequest(_userAId, _userBId);

            //Assert
            _uow.Verify(x => x.FollowRequestRepository.Exists(_userAId, _userBId), Times.Once());
            _uow.Verify(x => x.FollowRequestRepository.InsertGraph(It.IsAny<FollowRequest>()), Times.Never());
        }

        [Test]
        public void NotInsertIfRecordExists_OnSendFollowRequest()
        {
            //Arrange
            _uow.Setup(x => x.FollowRequestRepository.Exists(_userAId, _userBId))
                .Returns(true);
            _uow.Setup(x => x.FollowRequestRepository.InsertGraph(_followRequest));

            //Act
            _sut.SendFollowRequest(_userAId, _userBId);

            //Assert
            _uow.Verify(x => x.FollowRequestRepository.Exists(_userAId, _userBId), Times.Once());
            _uow.Verify(x => x.FollowRequestRepository.InsertGraph(It.IsAny<FollowRequest>()), Times.Never());
        }

        [Test]
        public void InsertAndSaveCorrectlyWhenNotExists_OnSendFollowRequest()
        {
            //Arrange
            _uow.Setup(x => x.FollowRequestRepository.Exists(_userAId, _userBId))
                .Returns(false);
            _uow.Setup(x => x.FollowRequestRepository.InsertGraph(_followRequest));

            //Act
            _sut.SendFollowRequest(_userAId, _userBId);

            //Assert
            _uow.Verify(x => x.FollowRequestRepository.Exists(_userAId, _userBId), Times.Once());
            _uow.Verify(x => x.FollowRequestRepository.InsertGraph(It.IsAny<FollowRequest>()), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Once());
        }

        [Test]
        public void ThrowIfCurrentUserIsNotTheTargetUserAndNotCreateNotification_OnAcceptFollowRequest()
        {
            //Arrange
            var followRequestId = 555;
            _uow.Setup(x => x.FollowRequestRepository.GetById(followRequestId))
                .Returns(_followRequest);
            _uow.Setup(x => x.FollowRepository.InsertGraph(It.IsAny<Follow>()));
            _uow.Setup(x => x.FollowRequestRepository.Remove(It.IsAny<FollowRequest>()));

            //Act
            try
            {
                _sut.AcceptFollowRequest(followRequestId, 999);
                Assert.Fail("Expected exception wasn't thrown");
            }
            catch (SecurityException)
            {
            }

            //Assert
            _uow.Verify(x => x.FollowRepository.InsertGraph(It.IsAny<Follow>()), Times.Never());
            _uow.Verify(x => x.FollowRequestRepository.Remove(It.IsAny<FollowRequest>()), Times.Never());
            _uow.Verify(x => x.SaveChanges(), Times.Never());
            _notificationService.Verify(x => x.CreateFollowAcceptedNotification(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never());
        }

        [Test]
        public void AddNewUserFollowAndDeleteRequestAndCreateANotification_OnAcceptFollowRequest()
        {
            //Arrange
            var followRequestId = 555;
            _uow.Setup(x => x.FollowRequestRepository.GetById(followRequestId))
                .Returns(_followRequest);
            _uow.Setup(x => x.FollowRepository.InsertGraph(It.IsAny<Follow>()));
            _uow.Setup(x => x.FollowRequestRepository.Remove(It.IsAny<FollowRequest>()));
            _notificationService.Setup(x => x.CreateFollowAcceptedNotification(
                _followRequest.FromUserId, _followRequest.ToUserId, false));
                
            //Act
            _sut.AcceptFollowRequest(followRequestId, _userBId);

            //Assert
            _uow.Verify(x => x.FollowRepository.InsertGraph(It.IsAny<Follow>()), Times.Once());
            _uow.Verify(x => x.FollowRequestRepository.Remove(It.IsAny<FollowRequest>()), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Once());
            _notificationService.Verify(x => x.CreateFollowAcceptedNotification(
                _followRequest.FromUserId, _followRequest.ToUserId, false), Times.Once());
        }

        [Test]
        public void ThrowIfCurrentUserIdIsNotTheTargetUser_OnRejectRequest()
        {
            //Arrange
            var followRequestId = 555;
            _uow.Setup(x => x.FollowRequestRepository.GetById(followRequestId))
                .Returns(_followRequest);
            _uow.Setup(x => x.FollowRepository.InsertGraph(It.IsAny<Follow>()));
            _uow.Setup(x => x.FollowRequestRepository.Remove(It.IsAny<FollowRequest>()));

            //Act

            try
            {
                _sut.RejectFollowRequest(followRequestId, 999);
                Assert.Fail("Expected exception wasn't thrown");
            }
            catch (SecurityException)
            {
            }

            //Assert
            _uow.Verify(x => x.FollowRepository.InsertGraph(It.IsAny<Follow>()), Times.Never());
            _uow.Verify(x => x.FollowRequestRepository.Remove(It.IsAny<FollowRequest>()), Times.Never());
            _uow.Verify(x => x.SaveChanges(), Times.Never());
        }

        [Test]
        public void RemoveTheRequest_OnRejectRequest()
        {
            //Arrange
            var followRequestId = 555;
            _uow.Setup(x => x.FollowRequestRepository.GetById(followRequestId))
                .Returns(_followRequest);
            _uow.Setup(x => x.FollowRepository.InsertGraph(It.IsAny<Follow>()));
            _uow.Setup(x => x.FollowRequestRepository.Remove(It.IsAny<FollowRequest>()));

            //Act
            _sut.RejectFollowRequest(followRequestId, _userBId);

            //Assert
            _uow.Verify(x => x.FollowRepository.InsertGraph(It.IsAny<Follow>()), Times.Never());
            _uow.Verify(x => x.FollowRequestRepository.Remove(It.IsAny<FollowRequest>()), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Once());
        }

        [Test]
        public void RemoveFollow_OnRemoveFollow()
        {
            //Arrange
            _uow.Setup(x => x.FollowRepository.Remove(_userAId, _userBId));

            //Act
            _sut.RemoveFollow(_userAId, _userBId);

            //Assert
            _uow.Verify(x => x.FollowRepository.Remove(_userAId, _userBId), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Once());
        }

        [Test]
        public void UseNotificationServiceToRemoveFollowNotification_OnRemoveFollow()
        {
            //Arrange
            _uow.Setup(x => x.FollowRepository.Remove(_userAId, _userBId));
            _notificationService.Setup(x => x.RemoveFollowAcceptedNotification(_userAId, _userBId, false));

            //Act
            _sut.RemoveFollow(_userAId, _userBId);

            //Assert
            _notificationService.Verify(x => x.RemoveFollowAcceptedNotification(_userAId, _userBId, false), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Once());
        }



        [Test]
        public void ReturnCorrectNumber_OnGetFollowRequestsCount()
        {
            //Arrange
            var count = 42;
            _uow.Setup(x => x.FollowRequestRepository.GetReceivedRequestsCount(_userAId))
                .Returns(count);

            //Act
            var result = _sut.GetFollowRequestsCount(_userAId);

            //Assert
            _uow.Verify(x => x.FollowRequestRepository.GetReceivedRequestsCount(_userAId), Times.Once());
            Assert.AreEqual(count, result);
        }

        [Test]
        public void ReturnReceivedRequests_OnGetReceivedRequests()
        {
            //Arrange
            var receivedRequests = new List<FollowRequest>();
            _uow.Setup(x => x.FollowRequestRepository.GetReceivedRequests(_userAId))
                .Returns(receivedRequests);

            //Act
            var result = _sut.GetFollowRequests(_userAId);

            //Assert
            _uow.Verify(x => x.FollowRequestRepository.GetReceivedRequests(_userAId), Times.Once());
            Assert.AreSame(receivedRequests, result);
        }
    }
}