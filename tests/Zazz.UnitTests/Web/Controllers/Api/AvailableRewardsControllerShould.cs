using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Web.OAuthAuthorizationServer;

namespace Zazz.UnitTests.Web.Controllers.Api
{
    [TestFixture]
    public class AvailableRewardsControllerShould : BaseOAuthTests
    {
        private Mock<IUoW> _uow;
        private const int USER_ID = 80;

        public override void Init()
        {
            base.Init();

            ControllerAddress = "/api/v1/availablerewards/" + USER_ID;

            _uow = MockRepo.Create<IUoW>();
            IocContainer.Configure(x =>
                                   {
                                       x.For<IUoW>().Use(_uow.Object);
                                   });
        }


        [Test]
        public async Task GetRewardsFromRepository_OnGet()
        {
            //Arrange
            CreateValidAccessToken();

            
            _uow.Setup(x => x.UserRewardRepository.GetRewards(USER_ID, User.Id))
                .Returns(new EnumerableQuery<UserReward>(new List<UserReward>()));

            //Act
            var response = await Client.GetAsync(ControllerAddress);

            //Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            MockRepo.VerifyAll();
        }

    }
}