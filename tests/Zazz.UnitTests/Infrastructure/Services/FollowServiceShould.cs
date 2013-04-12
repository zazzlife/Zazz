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

        [SetUp]
        public void Init()
        {
            _uow = new Mock<IUoW>();
            _sut = new FollowService(_uow.Object);

            _userAId = 12;
            _userBId = 15;
            _clubId = 13;
            _followRequest = new FollowRequest {FromUserId = _userAId, ToUserId = _userBId};


            _uow.Setup(x => x.SaveChanges());
        }

        [Test]
        public async Task NotCreateFollowIfExists_OnFollowClubAdmin()
        {
            //Arrange
            _uow.Setup(x => x.FollowRepository.Exists(_userAId, _userBId))
                .Returns(true);
            _uow.Setup(x => x.FollowRepository.InsertGraph(It.IsAny<Follow>()));

            //Act
            await _sut.FollowClubAdminAsync(_userAId, _userBId);

            //Assert
            _uow.Verify(x => x.FollowRepository.Exists(_userAId, _userBId), Times.Once());
            _uow.Verify(x => x.FollowRepository.InsertGraph(It.IsAny<Follow>()), Times.Never());
            _uow.Verify(x => x.SaveChanges(), Times.Never());
        }

        [Test]
        public async Task CreateFollowIfNotExists_OnFollowClubAdmin()
        {
            //Arrange
            _uow.Setup(x => x.FollowRepository.Exists(_userAId, _userBId))
                .Returns(false);
            _uow.Setup(x => x.FollowRepository.InsertGraph(It.IsAny<Follow>()));

            //Act
            await _sut.FollowClubAdminAsync(_userAId, _userBId);

            //Assert
            _uow.Verify(x => x.FollowRepository.Exists(_userAId, _userBId), Times.Once());
            _uow.Verify(x => x.FollowRepository.InsertGraph(It.IsAny<Follow>()), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Once());
        }

        [Test]
        public async Task CheckIfRecordExists_OnSendFollowRequest()
        {
            //Arrange
            _uow.Setup(x => x.FollowRequestRepository.Exists(_userAId, _userBId))
                .Returns(true);
            _uow.Setup(x => x.FollowRequestRepository.InsertGraph(_followRequest));

            //Act
            await _sut.SendFollowRequestAsync(_userAId, _userBId);

            //Assert
            _uow.Verify(x => x.FollowRequestRepository.Exists(_userAId, _userBId), Times.Once());
            _uow.Verify(x => x.FollowRequestRepository.InsertGraph(It.IsAny<FollowRequest>()), Times.Never());
        }

        [Test]
        public async Task NotInsertIfRecordExists_OnSendFollowRequest()
        {
            //Arrange
            _uow.Setup(x => x.FollowRequestRepository.Exists(_userAId, _userBId))
                .Returns(true);
            _uow.Setup(x => x.FollowRequestRepository.InsertGraph(_followRequest));

            //Act
            await _sut.SendFollowRequestAsync(_userAId, _userBId);

            //Assert
            _uow.Verify(x => x.FollowRequestRepository.Exists(_userAId, _userBId), Times.Once());
            _uow.Verify(x => x.FollowRequestRepository.InsertGraph(It.IsAny<FollowRequest>()), Times.Never());
        }

        [Test]
        public async Task InsertAndSaveCorrectlyWhenNotExists_OnSendFollowRequest()
        {
            //Arrange
            _uow.Setup(x => x.FollowRequestRepository.Exists(_userAId, _userBId))
                .Returns(false);
            _uow.Setup(x => x.FollowRequestRepository.InsertGraph(_followRequest));

            //Act
            await _sut.SendFollowRequestAsync(_userAId, _userBId);

            //Assert
            _uow.Verify(x => x.FollowRequestRepository.Exists(_userAId, _userBId), Times.Once());
            _uow.Verify(x => x.FollowRequestRepository.InsertGraph(It.IsAny<FollowRequest>()), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Once());
        }

        [Test]
        public async Task ThrowIfCurrentUserIsNotTheTargetUser_OnAcceptFollowRequest()
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
                await _sut.AcceptFollowRequestAsync(followRequestId, 999);
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
        public async Task AddNewUserFollowAndDeleteRequest_OnAcceptFollowRequest()
        {
            //Arrange
            var followRequestId = 555;
            _uow.Setup(x => x.FollowRequestRepository.GetById(followRequestId))
                .Returns(_followRequest);
            _uow.Setup(x => x.FollowRepository.InsertGraph(It.IsAny<Follow>()));
            _uow.Setup(x => x.FollowRequestRepository.Remove(It.IsAny<FollowRequest>()));
                
            //Act
            await _sut.AcceptFollowRequestAsync(followRequestId, _userBId);

            //Assert
            _uow.Verify(x => x.FollowRepository.InsertGraph(It.IsAny<Follow>()), Times.Once());
            _uow.Verify(x => x.FollowRequestRepository.Remove(It.IsAny<FollowRequest>()), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Once());
        }

        [Test]
        public async Task ThrowIfCurrentUserIdIsNotTheTargetUser_OnRejectRequest()
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
                await _sut.RejectFollowRequestAsync(followRequestId, 999);
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
        public async Task RemoveTheRequest_OnRejectRequest()
        {
            //Arrange
            var followRequestId = 555;
            _uow.Setup(x => x.FollowRequestRepository.GetById(followRequestId))
                .Returns(_followRequest);
            _uow.Setup(x => x.FollowRepository.InsertGraph(It.IsAny<Follow>()));
            _uow.Setup(x => x.FollowRequestRepository.Remove(It.IsAny<FollowRequest>()));

            //Act
            await _sut.RejectFollowRequestAsync(followRequestId, _userBId);

            //Assert
            _uow.Verify(x => x.FollowRepository.InsertGraph(It.IsAny<Follow>()), Times.Never());
            _uow.Verify(x => x.FollowRequestRepository.Remove(It.IsAny<FollowRequest>()), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Once());
        }

        [Test]
        public async Task RemoveFollow_OnRemoveFollow()
        {
            //Arrange
            _uow.Setup(x => x.FollowRepository.Remove(_userAId, _userBId));

            //Act
            await _sut.RemoveFollowAsync(_userAId, _userBId);

            //Assert
            _uow.Verify(x => x.FollowRepository.Remove(_userAId, _userBId), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Once());
        }



        [Test]
        public async Task ReturnCorrectNumber_OnGetFollowRequestsCount()
        {
            //Arrange
            var count = 42;
            _uow.Setup(x => x.FollowRequestRepository.GetReceivedRequestsCount(_userAId))
                .Returns(count);

            //Act
            var result = await _sut.GetFollowRequestsCountAsync(_userAId);

            //Assert
            _uow.Verify(x => x.FollowRequestRepository.GetReceivedRequestsCount(_userAId), Times.Once());
            Assert.AreEqual(count, result);
        }

        [Test]
        public async Task ReturnReceivedRequests_OnGetReceivedRequests()
        {
            //Arrange
            var receivedRequests = new List<FollowRequest>();
            _uow.Setup(x => x.FollowRequestRepository.GetReceivedRequests(_userAId))
                .Returns(receivedRequests);

            //Act
            var result = await _sut.GetFollowRequestsAsync(_userAId);

            //Assert
            _uow.Verify(x => x.FollowRequestRepository.GetReceivedRequests(_userAId), Times.Once());
            Assert.AreSame(receivedRequests, result);
        }
    }
}