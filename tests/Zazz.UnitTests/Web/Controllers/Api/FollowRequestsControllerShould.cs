using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.UnitTests.Web.Controllers.Api
{
    [TestFixture]
    public class FollowRequestsControllerShould : BaseHMACTests
    {
        private Mock<IFollowService> _followService;
        private int _userId;

        public override void Init()
        {
            base.Init();

            _userId = 83;
            ControllerAddress = "/api/v1/followrequests";
            _followService = MockRepo.Create<IFollowService>();

            IocContainer.Configure(x =>
            {
                x.For<IFollowService>().Use(_followService.Object);
            });
        }

        [Test]
        public async Task ReturnFollowRequests_OnGet()
        {
            //Arrange
            _followService.Setup(x => x.GetFollowRequests(User.Id))
                          .Returns(new EnumerableQuery<FollowRequest>(Enumerable.Empty<FollowRequest>()));

            AddValidHMACHeaders("GET");
            SetupMocksForHMACAuth();

            //Act
            var result = await Client.GetAsync(ControllerAddress);

            //Assert
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            MockRepo.VerifyAll();
        }
    }
}