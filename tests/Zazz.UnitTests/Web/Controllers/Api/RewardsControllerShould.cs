using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Zazz.Core.Exceptions;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Web.OAuthAuthorizationServer;

namespace Zazz.UnitTests.Web.Controllers.Api
{
    [TestFixture]
    public class RewardsControllerShould : BaseOAuthTests
    {
        private Mock<IUoW> _uow;
        private Mock<IClubRewardService> _rewardsService;
        private const int USER_ID = 80;

        public override void Init()
        {
            base.Init();

            _uow = MockRepo.Create<IUoW>();
            _rewardsService = MockRepo.Create<IClubRewardService>();

            IocContainer.Configure(x =>
                                   {
                                       x.For<IUoW>().Use(_uow.Object);
                                       x.For<IClubRewardService>().Use(_rewardsService.Object);
                                   });
        }


        [Test]
        public async Task GetRewardsFromRepository_OnGet()
        {
            //Arrange
            ControllerAddress = "/api/v1/rewards?userId=" + USER_ID;

            CreateValidAccessToken();


            _uow.Setup(x => x.UserRewardRepository.GetRewards(USER_ID, User.Id))
                .Returns(new EnumerableQuery<UserReward>(new List<UserReward>()));

            //Act
            var response = await Client.GetAsync(ControllerAddress);

            //Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task Return204IfRewardWasNotFound_OnDelete()
        {
            //Arrange
            var rewardId = 48729;

            ControllerAddress = "/api/v1/rewards/" + rewardId;

            CreateValidAccessToken();

            _rewardsService.Setup(x => x.RemoveUserReward(rewardId, User.Id))
                           .Throws<NotFoundException>();

            //Act
            var response = await Client.DeleteAsync(ControllerAddress);

            //Assert
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
            MockRepo.VerifyAll();

        }

        [Test]
        public async Task Return403IfNotAllowed_OnDelete()
        {
            //Arrange
            var rewardId = 48729;

            ControllerAddress = "/api/v1/rewards/" + rewardId;

            CreateValidAccessToken();

            _rewardsService.Setup(x => x.RemoveUserReward(rewardId, User.Id))
                           .Throws<SecurityException>();

            //Act
            var response = await Client.DeleteAsync(ControllerAddress);

            //Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
            MockRepo.VerifyAll();

        }

        [Test]
        public async Task Return204WhenOk_OnDelete()
        {
            //Arrange
            var rewardId = 48729;

            ControllerAddress = "/api/v1/rewards/" + rewardId;

            CreateValidAccessToken();

            _rewardsService.Setup(x => x.RemoveUserReward(rewardId, User.Id));

            //Act
            var response = await Client.DeleteAsync(ControllerAddress);

            //Assert
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
            MockRepo.VerifyAll();

        }
    }
}