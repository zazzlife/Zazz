using System.Collections.Generic;
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


            _uow.Setup(x => x.SaveAsync())
                    .Returns(Task.Run(() => { }));
        }

        [Test]
        public async Task NotCreateFollowIfExists_OnFollowClubAdmin()
        {
            //Arrange
            _uow.Setup(x => x.FollowRepository.ExistsAsync(_userAId, _userBId))
                .Returns(() => Task.Run(() => true));
            _uow.Setup(x => x.FollowRepository.InsertGraph(It.IsAny<Follow>()));

            //Act
            await _sut.FollowClubAdminAsync(_userAId, _userBId);

            //Assert
            _uow.Verify(x => x.FollowRepository.ExistsAsync(_userAId, _userBId), Times.Once());
            _uow.Verify(x => x.FollowRepository.InsertGraph(It.IsAny<Follow>()), Times.Never());
            _uow.Verify(x => x.SaveAsync(), Times.Never());
        }

        [Test]
        public async Task CreateFollowIfNotExists_OnFollowClubAdmin()
        {
            //Arrange
            _uow.Setup(x => x.FollowRepository.ExistsAsync(_userAId, _userBId))
                .Returns(() => Task.Run(() => false));
            _uow.Setup(x => x.FollowRepository.InsertGraph(It.IsAny<Follow>()));

            //Act
            await _sut.FollowClubAdminAsync(_userAId, _userBId);

            //Assert
            _uow.Verify(x => x.FollowRepository.ExistsAsync(_userAId, _userBId), Times.Once());
            _uow.Verify(x => x.FollowRepository.InsertGraph(It.IsAny<Follow>()), Times.Once());
            _uow.Verify(x => x.SaveAsync(), Times.Once());
        }

        [Test]
        public async Task CheckIfRecordExists_OnSendFollowRequest()
        {
            //Arrange
            _uow.Setup(x => x.FollowRequestRepository.ExistsAsync(_userAId, _userBId))
                .Returns(() => Task.Run(() => true));
            _uow.Setup(x => x.FollowRequestRepository.InsertGraph(_followRequest));

            //Act
            await _sut.SendFollowRequestAsync(_userAId, _userBId);

            //Assert
            _uow.Verify(x => x.FollowRequestRepository.ExistsAsync(_userAId, _userBId), Times.Once());
            _uow.Verify(x => x.FollowRequestRepository.InsertGraph(It.IsAny<FollowRequest>()), Times.Never());
        }

        [Test]
        public async Task NotInsertIfRecordExists_OnSendFollowRequest()
        {
            //Arrange
            _uow.Setup(x => x.FollowRequestRepository.ExistsAsync(_userAId, _userBId))
                .Returns(() => Task.Run(() => true));
            _uow.Setup(x => x.FollowRequestRepository.InsertGraph(_followRequest));

            //Act
            await _sut.SendFollowRequestAsync(_userAId, _userBId);

            //Assert
            _uow.Verify(x => x.FollowRequestRepository.ExistsAsync(_userAId, _userBId), Times.Once());
            _uow.Verify(x => x.FollowRequestRepository.InsertGraph(It.IsAny<FollowRequest>()), Times.Never());
        }

        [Test]
        public async Task InsertAndSaveCorrectlyWhenNotExists_OnSendFollowRequest()
        {
            //Arrange
            _uow.Setup(x => x.FollowRequestRepository.ExistsAsync(_userAId, _userBId))
                .Returns(() => Task.Run(() => false));
            _uow.Setup(x => x.FollowRequestRepository.InsertGraph(_followRequest));

            //Act
            await _sut.SendFollowRequestAsync(_userAId, _userBId);

            //Assert
            _uow.Verify(x => x.FollowRequestRepository.ExistsAsync(_userAId, _userBId), Times.Once());
            _uow.Verify(x => x.FollowRequestRepository.InsertGraph(It.IsAny<FollowRequest>()), Times.Once());
            _uow.Verify(x => x.SaveAsync(), Times.Once());
        }

        [Test]
        public async Task AddNewUserFollowAndDeleteRequest_OnAcceptFollowRequest()
        {
            //Arrange
            var followRequestId = 555;
            _uow.Setup(x => x.FollowRequestRepository.GetByIdAsync(followRequestId))
                .Returns(() => Task.Run(() => _followRequest));
            _uow.Setup(x => x.FollowRepository.InsertGraph(It.IsAny<Follow>()));
            _uow.Setup(x => x.FollowRequestRepository.Remove(It.IsAny<FollowRequest>()));
                
            //Act
            await _sut.AcceptFollowRequestAsync(followRequestId);

            //Assert
            _uow.Verify(x => x.FollowRepository.InsertGraph(It.IsAny<Follow>()), Times.Once());
            _uow.Verify(x => x.FollowRequestRepository.Remove(It.IsAny<FollowRequest>()), Times.Once());
            _uow.Verify(x => x.SaveAsync(), Times.Once());
        }

        [Test]
        public async Task RemoveTheRequest_RejectRequest()
        {
            //Arrange
            var followRequestId = 555;
            _uow.Setup(x => x.FollowRequestRepository.GetByIdAsync(followRequestId))
                .Returns(() => Task.Run(() => _followRequest));
            _uow.Setup(x => x.FollowRepository.InsertGraph(It.IsAny<Follow>()));
            _uow.Setup(x => x.FollowRequestRepository.Remove(It.IsAny<FollowRequest>()));

            //Act
            await _sut.RejectFollowRequestAsync(followRequestId);

            //Assert
            _uow.Verify(x => x.FollowRepository.InsertGraph(It.IsAny<Follow>()), Times.Never());
            _uow.Verify(x => x.FollowRequestRepository.Remove(It.IsAny<FollowRequest>()), Times.Once());
            _uow.Verify(x => x.SaveAsync(), Times.Once());
        }

        [Test]
        public async Task RemoveFollow_OnRemoveFollow()
        {
            //Arrange
            _uow.Setup(x => x.FollowRepository.RemoveAsync(_userAId, _userBId))
                .Returns(() => Task.Run(() => { }));

            //Act
            await _sut.RemoveFollowAsync(_userAId, _userBId);

            //Assert
            _uow.Verify(x => x.FollowRepository.RemoveAsync(_userAId, _userBId), Times.Once());
            _uow.Verify(x => x.SaveAsync(), Times.Once());
        }



        [Test]
        public async Task ReturnCorrectNumber_OnGetFollowRequestsCount()
        {
            //Arrange
            var count = 42;
            _uow.Setup(x => x.FollowRequestRepository.GetReceivedRequestsCountAsync(_userAId))
                .Returns(() => Task.Run(() => count));

            //Act
            var result = await _sut.GetFollowRequestsCountAsync(_userAId);

            //Assert
            _uow.Verify(x => x.FollowRequestRepository.GetReceivedRequestsCountAsync(_userAId), Times.Once());
            Assert.AreEqual(count, result);
        }

        [Test]
        public async Task ReturnReceivedRequests_OnGetReceivedRequests()
        {
            //Arrange
            var receivedRequests = new List<FollowRequest>();
            _uow.Setup(x => x.FollowRequestRepository.GetReceivedRequestsAsync(_userAId))
                .Returns(() => Task.Run(() => receivedRequests));

            //Act
            var result = await _sut.GetFollowRequestsAsync(_userAId);

            //Assert
            _uow.Verify(x => x.FollowRequestRepository.GetReceivedRequestsAsync(_userAId), Times.Once());
            Assert.AreSame(receivedRequests, result);
        }
    }
}