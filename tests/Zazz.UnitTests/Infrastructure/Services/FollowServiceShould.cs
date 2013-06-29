using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Zazz.Core.Exceptions;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;
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
        private MockRepository _mockRepo;

        [SetUp]
        public void Init()
        {
            _mockRepo = new MockRepository(MockBehavior.Strict);

            _uow = _mockRepo.Create<IUoW>();
            _notificationService = _mockRepo.Create<INotificationService>();
            _sut = new FollowService(_uow.Object, _notificationService.Object);

            _userAId = 12;
            _userBId = 15;
            _clubId = 13;
            _followRequest = new FollowRequest {FromUserId = _userAId, ToUserId = _userBId};
        }

        [Test]
        public void ThrowIfUserIsTryingToFollowHimself_OnFollow()
        {
            //Arrange
            //Act
            Assert.Throws<InvalidFollowException>(() => _sut.Follow(_userAId, _userAId));

            //Assert
            _mockRepo.VerifyAll();
        }



        [Test]
        public void NotCreateFollowIfItAlreadyExists_OnFollow()
        {
            //Arrange
            _uow.Setup(x => x.FollowRepository.Exists(_userAId, _userBId))
                .Returns(true);

            //Act
            _sut.Follow(_userAId, _userBId);

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void AddNewFollowRecordIfAccountTypeIsClub_OnFollow()
        {
            //Arrange
            _uow.Setup(x => x.FollowRepository.Exists(_userAId, _userBId))
                .Returns(false);

            _uow.Setup(x => x.UserRepository.GetUserAccountType(_userBId))
                .Returns(AccountType.Club);

            _uow.Setup(x => x.FollowRepository
                             .InsertGraph(It.Is<Follow>(f => f.FromUserId == _userAId &&
                                                             f.ToUserId == _userBId)));
            _uow.Setup(x => x.SaveChanges());

            //Act
            _sut.Follow(_userAId, _userBId);

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void NotCreateANewFollowRequestIfAccountIsUserAndRequestExists_OnFollow()
        {
            //Arrange
            _uow.Setup(x => x.FollowRepository.Exists(_userAId, _userBId))
                .Returns(false);

            _uow.Setup(x => x.UserRepository.GetUserAccountType(_userBId))
                .Returns(AccountType.User);

            _uow.Setup(x => x.FollowRequestRepository.Exists(_userAId, _userBId))
                .Returns(true);

            //Act
            _sut.Follow(_userAId, _userBId);

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void CreateAFollowRequestIfAccountIsUserAndFollowRequestNotExists_OnFollow()
        {
            //Arrange
            _uow.Setup(x => x.FollowRepository.Exists(_userAId, _userBId))
                .Returns(false);

            _uow.Setup(x => x.UserRepository.GetUserAccountType(_userBId))
                .Returns(AccountType.User);

            _uow.Setup(x => x.FollowRequestRepository.Exists(_userAId, _userBId))
                .Returns(false);

            _uow.Setup(x => x.FollowRequestRepository
                             .InsertGraph(It.Is<FollowRequest>(f => f.FromUserId == _userAId &&
                                                                    f.ToUserId == _userBId)));

            _uow.Setup(x => x.SaveChanges());
            //Act
            _sut.Follow(_userAId, _userBId);

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void ThrowIfCurrentUserIsNotTheTargetUserAndNotCreateNotification_OnAcceptFollowRequest()
        {
            //Arrange
            var maliciousUserId = 999;
            _uow.Setup(x => x.FollowRequestRepository
               .GetFollowRequest(_followRequest.FromUserId, maliciousUserId))
               .Returns(_followRequest);

            //Act
            Assert.Throws<SecurityException>(() => _sut.AcceptFollowRequest(_followRequest.FromUserId, maliciousUserId));

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void AddNewUserFollowAndDeleteRequestAndCreateANotification_OnAcceptFollowRequest()
        {
            //Arrange
            _uow.Setup(x => x.FollowRequestRepository
               .GetFollowRequest(_followRequest.FromUserId, _followRequest.ToUserId))
               .Returns(_followRequest);

            _uow.Setup(x => x.FollowRepository.InsertGraph(It.IsAny<Follow>()));
            _uow.Setup(x => x.FollowRequestRepository.Remove(It.IsAny<FollowRequest>()));
            _notificationService.Setup(x => x.CreateFollowAcceptedNotification(
                _followRequest.FromUserId, _followRequest.ToUserId, false));

            _uow.Setup(x => x.SaveChanges());
            //Act
            _sut.AcceptFollowRequest(_followRequest.FromUserId, _userBId);

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void ThrowIfCurrentUserIdIsNotTheTargetUser_OnRejectRequest()
        {
            //Arrange
            var maliciousUserId = 999;
            _uow.Setup(x => x.FollowRequestRepository
               .GetFollowRequest(_followRequest.FromUserId, maliciousUserId))
               .Returns(_followRequest);

            //Act
            Assert.Throws<SecurityException>(() => _sut.RejectFollowRequest(_followRequest.FromUserId, maliciousUserId));

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void RemoveTheRequest_OnRejectRequest()
        {
            //Arrange
            _uow.Setup(x => x.FollowRequestRepository
               .GetFollowRequest(_followRequest.FromUserId, _followRequest.ToUserId))
               .Returns(_followRequest);

            _uow.Setup(x => x.FollowRequestRepository.Remove(It.IsAny<FollowRequest>()));
            _uow.Setup(x => x.SaveChanges());

            //Act
            _sut.RejectFollowRequest(_followRequest.FromUserId, _userBId);

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void RemoveFollow_OnRemoveFollow()
        {
            //Arrange
            _uow.Setup(x => x.FollowRepository.Remove(_userAId, _userBId));
            _notificationService.Setup(x => x.RemoveFollowAcceptedNotification(_userAId, _userBId, false));
            _uow.Setup(x => x.SaveChanges());

            //Act
            _sut.RemoveFollow(_userAId, _userBId);

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void UseNotificationServiceToRemoveFollowNotification_OnRemoveFollow()
        {
            //Arrange
            _uow.Setup(x => x.FollowRepository.Remove(_userAId, _userBId));
            _notificationService.Setup(x => x.RemoveFollowAcceptedNotification(_userAId, _userBId, false));
            _uow.Setup(x => x.SaveChanges());

            //Act
            _sut.RemoveFollow(_userAId, _userBId);

            //Assert
            _mockRepo.VerifyAll();
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
            _mockRepo.VerifyAll();
            Assert.AreEqual(count, result);
        }

        [Test]
        public void ReturnReceivedRequests_OnGetReceivedRequests()
        {
            //Arrange
            var receivedRequests = new List<FollowRequest>();
            _uow.Setup(x => x.FollowRequestRepository.GetReceivedRequests(_userAId))
                .Returns(receivedRequests.AsQueryable());

            //Act
            var result = _sut.GetFollowRequests(_userAId);

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void GetFollowers_OnGetFollowers()
        {
            //Arrange
            var userId = 5343;
            _uow.Setup(x => x.FollowRepository.GetUserFollowers(userId))
                .Returns(new EnumerableQuery<Follow>(Enumerable.Empty<Follow>()));

            //Act
            _sut.GetFollowers(userId);

            //Assert
            _mockRepo.VerifyAll();
        }
    }
}