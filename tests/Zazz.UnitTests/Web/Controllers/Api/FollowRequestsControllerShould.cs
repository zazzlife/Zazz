using System;
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
    public class FollowRequestsControllerShould : BaseOAuthTests
    {
        private Mock<IFollowService> _followService;
        private int _userId;

        public override void Init()
        {
            base.Init();

            _userId = 83;
            ControllerAddress = "/api/v1/followrequests/" + _userId;
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
            ControllerAddress = "/api/v1/followrequests";

            _followService.Setup(x => x.GetFollowRequests(User.Id))
                          .Returns(new EnumerableQuery<FollowRequest>(Enumerable.Empty<FollowRequest>()));

            CreateValidAccessToken();

            //Act
            var result = await Client.GetAsync(ControllerAddress);

            //Assert
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task Return400IfIdIs0_OnGet()
        {
            //Arrange
            ControllerAddress = "/api/v1/followrequests/0?action=accept";

            CreateValidAccessToken();

            //Act
            var response = await Client.DeleteAsync(ControllerAddress);

            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task Return400IfActionIsNotValid_OnGet()
        {
            //Arrange
            ControllerAddress = "/api/v1/followrequests/444?action=randomaction";

            CreateValidAccessToken();

            //Act
            var response = await Client.DeleteAsync(ControllerAddress);

            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task AcceptFollowRequestWhenActionIsAccept_OnGet()
        {
            //Arrange
            ControllerAddress = String.Format("/api/v1/followrequests/{0}?action=accept", _userId);

            _followService.Setup(x => x.AcceptFollowRequest(_userId, User.Id));

            CreateValidAccessToken();

            //Act
            var response = await Client.DeleteAsync(ControllerAddress);

            //Assert
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task RejectFollowRequestWhenActionIsReject_OnGet()
        {
            //Arrange
            ControllerAddress = String.Format("/api/v1/followrequests/{0}?action=reject", _userId);

            _followService.Setup(x => x.RejectFollowRequest(_userId, User.Id));

            CreateValidAccessToken();

            //Act
            var response = await Client.DeleteAsync(ControllerAddress);

            //Assert
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
            MockRepo.VerifyAll();
        }
    }
}