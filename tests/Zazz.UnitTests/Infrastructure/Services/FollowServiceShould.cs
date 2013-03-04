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
        private ClubFollow _clubFollow;
        private int _userBId;
        private UserFollowRequest _userFollowRequest;

        [SetUp]
        public void Init()
        {
            _uow = new Mock<IUoW>();
            _sut = new FollowService(_uow.Object);

            _userAId = 12;
            _userBId = 15;
            _clubId = 13;
            _clubFollow = new ClubFollow { ClubId = _clubId, UserId = _userAId };
            _userFollowRequest = new UserFollowRequest {FromUserId = _userAId, ToUserId = _userBId};


            _uow.Setup(x => x.SaveAsync())
                    .Returns(Task.Run(() => { }));
        }

        [Test]
        public async Task CheckBeforeAddingNewFollow_OnFollowClubAsync()
        {
            //Arrange
            _uow.Setup(x => x.ClubFollowRepository.ExistsAsync(_userAId, _clubId))
                .Returns(() => Task.Run(() => true));
            _uow.Setup(x => x.ClubFollowRepository.InsertGraph(_clubFollow));

            //Act
            await _sut.FollowClubAsync(_userAId, _clubId);

            //Assert
            _uow.Verify(x => x.ClubFollowRepository.ExistsAsync(_userAId, _clubId), Times.Once());

        }

        [Test]
        public async Task NotInsertNewRecordIfFollowExists_OnFollowClubAsync()
        {
            //Arrange
            _uow.Setup(x => x.ClubFollowRepository.ExistsAsync(_userAId, _clubId))
                .Returns(() => Task.Run(() => true));
            _uow.Setup(x => x.ClubFollowRepository.InsertGraph(_clubFollow));

            //Act
            await _sut.FollowClubAsync(_userAId, _clubId);

            //Assert
            _uow.Verify(x => x.ClubFollowRepository.ExistsAsync(_userAId, _clubId), Times.Once());
            _uow.Verify(x => x.ClubFollowRepository.InsertGraph(It.IsAny<ClubFollow>()), Times.Never());
        }

        [Test]
        public async Task InsertAndSaveWhenRecordIsNotExists_OnFollowClubAsync()
        {
            //Arrange
            _uow.Setup(x => x.ClubFollowRepository.ExistsAsync(_userAId, _clubId))
                .Returns(() => Task.Run(() => false));
            _uow.Setup(x => x.ClubFollowRepository.InsertGraph(_clubFollow));

            //Act
            await _sut.FollowClubAsync(_userAId, _clubId);

            //Assert
            _uow.Verify(x => x.ClubFollowRepository.ExistsAsync(_userAId, _clubId), Times.Once());
            _uow.Verify(x => x.ClubFollowRepository.InsertGraph(It.IsAny<ClubFollow>()), Times.Once());
            _uow.Verify(x => x.SaveAsync(), Times.Once());
        }

        [Test]
        public async Task CheckIfRecordExists_OnSendFollowRequest()
        {
            //Arrange
            _uow.Setup(x => x.UserFollowRequestRepository.ExistsAsync(_userAId, _userBId))
                .Returns(() => Task.Run(() => true));
            _uow.Setup(x => x.UserFollowRequestRepository.InsertGraph(_userFollowRequest));

            //Act
            await _sut.SendFollowRequestAsync(_userAId, _userBId);

            //Assert
            _uow.Verify(x => x.UserFollowRequestRepository.ExistsAsync(_userAId, _userBId), Times.Once());
        }

        [Test]
        public async Task NotInsertIfRecordExists_OnSendFollowRequest()
        {
            //Arrange
            _uow.Setup(x => x.UserFollowRequestRepository.ExistsAsync(_userAId, _userBId))
                .Returns(() => Task.Run(() => true));
            _uow.Setup(x => x.UserFollowRequestRepository.InsertGraph(_userFollowRequest));

            //Act
            await _sut.SendFollowRequestAsync(_userAId, _userBId);

            //Assert
            _uow.Verify(x => x.UserFollowRequestRepository.ExistsAsync(_userAId, _userBId), Times.Once());
            _uow.Verify(x => x.UserFollowRequestRepository.InsertGraph(It.IsAny<UserFollowRequest>()), Times.Never());
        }

        [Test]
        public async Task InsertAndSaveCorrectlyWhenNotExists_OnSendFollowRequest()
        {
            //Arrange
            _uow.Setup(x => x.UserFollowRequestRepository.ExistsAsync(_userAId, _userBId))
                .Returns(() => Task.Run(() => false));
            _uow.Setup(x => x.UserFollowRequestRepository.InsertGraph(_userFollowRequest));

            //Act
            await _sut.SendFollowRequestAsync(_userAId, _userBId);

            //Assert
            _uow.Verify(x => x.UserFollowRequestRepository.ExistsAsync(_userAId, _userBId), Times.Once());
            _uow.Verify(x => x.UserFollowRequestRepository.InsertGraph(It.IsAny<UserFollowRequest>()), Times.Once());
            _uow.Verify(x => x.SaveAsync(), Times.Once());
        }

        [Test]
        public async Task AddNewUserFollowAndDeleteRequest_OnAcceptFollowRequest()
        {
            //Arrange
            var followRequestId = 555;
            _uow.Setup(x => x.UserFollowRequestRepository.GetByIdAsync(followRequestId))
                .Returns(() => Task.Run(() => _userFollowRequest));
            _uow.Setup(x => x.UserFollowRepository.InsertGraph(It.IsAny<UserFollow>()));
            _uow.Setup(x => x.UserFollowRequestRepository.Remove(It.IsAny<UserFollowRequest>()));
                
            //Act
            await _sut.AcceptFollowRequestAsync(followRequestId);

            //Assert
            _uow.Verify(x => x.UserFollowRepository.InsertGraph(It.IsAny<UserFollow>()), Times.Once());
            _uow.Verify(x => x.UserFollowRequestRepository.Remove(It.IsAny<UserFollowRequest>()), Times.Once());
            _uow.Verify(x => x.SaveAsync(), Times.Once());
        }

        [Test]
        public async Task RemoveTheRequest_RejectRequest()
        {
            //Arrange
            var followRequestId = 555;
            _uow.Setup(x => x.UserFollowRequestRepository.GetByIdAsync(followRequestId))
                .Returns(() => Task.Run(() => _userFollowRequest));
            _uow.Setup(x => x.UserFollowRepository.InsertGraph(It.IsAny<UserFollow>()));
            _uow.Setup(x => x.UserFollowRequestRepository.Remove(It.IsAny<UserFollowRequest>()));

            //Act
            await _sut.RejectFollowRequestAsync(followRequestId);

            //Assert
            _uow.Verify(x => x.UserFollowRepository.InsertGraph(It.IsAny<UserFollow>()), Times.Never());
            _uow.Verify(x => x.UserFollowRequestRepository.Remove(It.IsAny<UserFollowRequest>()), Times.Once());
            _uow.Verify(x => x.SaveAsync(), Times.Once());
        }

        [Test]
        public async Task ReturnCorrectNumber_OnGetFollowRequestsCount()
        {
            //Arrange
            var count = 42;
            _uow.Setup(x => x.UserFollowRequestRepository.GetReceivedRequestsCountAsync(_userAId))
                .Returns(() => Task.Run(() => count));

            //Act
            var result = await _sut.GetFollowRequestsCountAsync(_userAId);

            //Assert
            _uow.Verify(x => x.UserFollowRequestRepository.GetReceivedRequestsCountAsync(_userAId), Times.Once());
            Assert.AreEqual(count, result);
        }

        [Test]
        public async Task ReturnReceivedRequests_OnGetReceivedRequests()
        {
            //Arrange
            var receivedRequests = new List<UserFollowRequest>();
            _uow.Setup(x => x.UserFollowRequestRepository.GetReceivedRequestsAsync(_userAId))
                .Returns(() => Task.Run(() => receivedRequests));

            //Act
            var result = await _sut.GetFollowRequestsAsync(_userAId);

            //Assert
            _uow.Verify(x => x.UserFollowRequestRepository.GetReceivedRequestsAsync(_userAId), Times.Once());
            Assert.AreSame(receivedRequests, result);

        }


    }
}