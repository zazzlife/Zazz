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
        private int _userId;
        private int _clubId;
        private ClubFollow _follow;

        [SetUp]
        public void Init()
        {
            _uow = new Mock<IUoW>();
            _sut = new FollowService(_uow.Object);

            _userId = 12;
            _clubId = 13;
            _follow = new ClubFollow { ClubId = _clubId, UserId = _userId };

            _uow.Setup(x => x.SaveAsync())
                    .Returns(Task.Run(() => { }));
        }

        [Test]
        public async Task CheckBeforeAddingNewFollow_OnFollowClubAsync()
        {
            //Arrange
            _uow.Setup(x => x.ClubFollowRepository.ExistsAsync(_userId, _clubId))
                .Returns(() => Task.Run(() => true));
            _uow.Setup(x => x.ClubFollowRepository.InsertGraph(_follow));

            //Act
            await _sut.FollowClubAsync(_userId, _clubId);

            //Assert
            _uow.Verify(x => x.ClubFollowRepository.ExistsAsync(_userId, _clubId), Times.Once());

        }

        [Test]
        public async Task NotInsertNewRecordIfFollowExists_OnFollowClubAsync()
        {
            //Arrange
            _uow.Setup(x => x.ClubFollowRepository.ExistsAsync(_userId, _clubId))
                .Returns(() => Task.Run(() => true));
            _uow.Setup(x => x.ClubFollowRepository.InsertGraph(_follow));

            //Act
            await _sut.FollowClubAsync(_userId, _clubId);

            //Assert
            _uow.Verify(x => x.ClubFollowRepository.ExistsAsync(_userId, _clubId), Times.Once());
            _uow.Verify(x => x.ClubFollowRepository.InsertGraph(It.IsAny<ClubFollow>()), Times.Never());
        }

        [Test]
        public async Task InsertAndSaveWhenRecordIsNotExists_OnFollowClubAsync()
        {
            //Arrange
            _uow.Setup(x => x.ClubFollowRepository.ExistsAsync(_userId, _clubId))
                .Returns(() => Task.Run(() => false));
            _uow.Setup(x => x.ClubFollowRepository.InsertGraph(_follow));

            //Act
            await _sut.FollowClubAsync(_userId, _clubId);

            //Assert
            _uow.Verify(x => x.ClubFollowRepository.ExistsAsync(_userId, _clubId), Times.Once());
            _uow.Verify(x => x.ClubFollowRepository.InsertGraph(It.IsAny<ClubFollow>()), Times.Once());
            _uow.Verify(x => x.SaveAsync(), Times.Once());
        }
    }
}