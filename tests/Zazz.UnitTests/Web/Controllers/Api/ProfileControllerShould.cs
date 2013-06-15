using System.Net;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Zazz.Core.Interfaces;
using Zazz.Web.Interfaces;

namespace Zazz.UnitTests.Web.Controllers.Api
{
    [TestFixture]
    public class ProfileControllerShould : BaseHMACTests
    {
        private Mock<IFollowService> _followService;
        private Mock<IFeedHelper> _feedHelper;

        public override void Init()
        {
            base.Init();
            ControllerAddress = "/api/v1/user/" + User.Id + "/profile";

            _followService = MockRepo.Create<IFollowService>(MockBehavior.Strict);
            _feedHelper = MockRepo.Create<IFeedHelper>(MockBehavior.Strict);

            IocContainer.Configure(x =>
                                   {
                                       x.For<IFollowService>().Use(_followService.Object);
                                       x.For<IFeedHelper>().Use(_feedHelper.Object);
                                   });
        }

        [Test]
        public async Task Return400IfIdIsInvalid_OnGet()
        {
            //Arrange
            ControllerAddress = "/api/v1/user/0/profile";

            AddValidHMACHeaders("GET");
            SetupMocksForHMACAuth();

            //Act
            var result = await Client.GetAsync(ControllerAddress);

            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }
    }
}