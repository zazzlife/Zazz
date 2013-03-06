using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Infrastructure.Services;

namespace Zazz.UnitTests.Infrastructure.Services
{
    [TestFixture]
    public class ClubServiceShould
    {
        private Mock<IUoW> _uow;
        private ClubService _sut;
        private Club _club;
        private int _userId;

        [SetUp]
        public void Init()
        {
            _uow = new Mock<IUoW>();
            _sut = new ClubService(_uow.Object);

            _club = new Club();
            _userId = 12;

            _uow.Setup(x => x.SaveAsync())
                .Returns(() => Task.Run(() => { }));
        }

        [Test]
        public async Task AddNewClub_OnCreateNewClub()
        {
            //Arrange
            _uow.Setup(x => x.ClubRepository.InsertGraph(_club));


            //Act
            await _sut.CreateClubAsync(_club);

            //Assert
            _uow.Verify(x => x.ClubRepository.InsertGraph(_club), Times.Once());
            _uow.Verify(x => x.SaveAsync());

        }

        [Test]
        public async Task ReturnTrueIfUserIsClubAdmin_OnIsAuthorized()
        {
            //Arrange
            _club.Id = 15;
            _uow.Setup(x => x.ClubAdminRepository.ExistsAsync(_userId, _club.Id))
                .Returns(() => Task.Run(() => true));

            //Act
            var result = await _sut.IsUserAuthorized(_userId, _club.Id);

            //Assert
            Assert.IsTrue(result);
            _uow.Verify(x => x.ClubAdminRepository.ExistsAsync(_userId, _club.Id));
        }

        [Test]
        public async Task ReturnFalseIfUserIsNotClubAdmin_OnIsAuthorized()
        {
            //Arrange
            _club.Id = 15;
            _uow.Setup(x => x.ClubAdminRepository.ExistsAsync(_userId, _club.Id))
                .Returns(() => Task.Run(() => false));

            //Act
            var result = await _sut.IsUserAuthorized(_userId, _club.Id);

            //Assert
            Assert.IsFalse(result);
            _uow.Verify(x => x.ClubAdminRepository.ExistsAsync(_userId, _club.Id));
        }


    }
}