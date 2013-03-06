using System;
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

            _club = new Club {Id = 15};
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
            _uow.Verify(x => x.SaveAsync(), Times.Once());

        }

        [Test]
        public async Task ReturnTrueIfUserIsClubAdmin_OnIsAuthorized()
        {
            //Arrange
            _uow.Setup(x => x.ClubAdminRepository.ExistsAsync(_userId, _club.Id))
                .Returns(() => Task.Run(() => true));

            //Act
            var result = await _sut.IsUserAuthorized(_userId, _club.Id);

            //Assert
            Assert.IsTrue(result);
            _uow.Verify(x => x.ClubAdminRepository.ExistsAsync(_userId, _club.Id), Times.Once());
        }

        [Test]
        public async Task ReturnFalseIfUserIsNotClubAdmin_OnIsAuthorized()
        {
            //Arrange
            _uow.Setup(x => x.ClubAdminRepository.ExistsAsync(_userId, _club.Id))
                .Returns(() => Task.Run(() => false));

            //Act
            var result = await _sut.IsUserAuthorized(_userId, _club.Id);

            //Assert
            Assert.IsFalse(result);
            _uow.Verify(x => x.ClubAdminRepository.ExistsAsync(_userId, _club.Id), Times.Once());
        }

        [Test]
        public async Task ThrowIfClubIdIs0_OnUpdateClub()
        {
            //Arrange
            _club.Id = 0;

            //Act
            try
            {
                await _sut.UpdateClubAsync(_club, _userId);
                Assert.Fail("Expected exception was not thrown");
            }
            catch (ArgumentException)
            {
            }

            //Assert
        }

        [Test]
        public async Task ThrowIfUserIsNotAuthorized_OnUpdateClub()
        {
            //Arrange
            _uow.Setup(x => x.ClubAdminRepository.ExistsAsync(_userId, _club.Id))
                .Returns(() => Task.Run(() => false));

            //Act
            try
            {
                await _sut.UpdateClubAsync(_club, _userId);
                Assert.Fail("Expected exception was not thrown");
            }
            catch (SecurityException)
            {
            }

            //Assert
            _uow.Verify(x => x.ClubAdminRepository.ExistsAsync(_userId, _club.Id), Times.Once());
        }

        [Test]
        public async Task UpdateTheClub_OnUpdateClub()
        {
            //Arrange
            _uow.Setup(x => x.ClubAdminRepository.ExistsAsync(_userId, _club.Id))
                .Returns(() => Task.Run(() => true));
            _uow.Setup(x => x.ClubRepository.InsertOrUpdate(_club));

            //Act
            await _sut.UpdateClubAsync(_club, _userId);

            //Assert
            _uow.Verify(x => x.ClubRepository.InsertOrUpdate(_club), Times.Once());
            _uow.Verify(x => x.SaveAsync(), Times.Once());

        }


    }
}