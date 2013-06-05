using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.UnitTests.Web.Controllers.Api
{
    [TestFixture]
    public class FollowersControllerShould : BaseHMACTests
    {
        private int _userId;
        private Mock<IFollowService> _followService;

        public override void Init()
        {
            DefaultHttpMethod = HttpMethod.Delete;

            base.Init();

            _userId = 5;
            ControllerAddress = "/api/v1/followers/" + _userId;
            _followService = MockRepo.Create<IFollowService>();

            IocContainer.Configure(x =>
                                   {
                                       x.For<IFollowService>().Use(_followService.Object);
                                   });
        }

        [Test]
        public async Task GetFollowers_OnGet()
        {
            //Arrange
            ControllerAddress = "/api/v1/followers";

            _followService.Setup(x => x.GetFollowers(User.Id))
                          .Returns(new EnumerableQuery<Follow>(Enumerable.Empty<Follow>()));

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