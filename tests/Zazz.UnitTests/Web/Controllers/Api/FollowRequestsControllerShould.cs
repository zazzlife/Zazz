﻿using System.Linq;
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

            AddValidHMACHeaders("GET");
            SetupMocksForHMACAuth();

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

            AddValidHMACHeaders("DELETE");
            SetupMocksForHMACAuth();

            //Act
            var response = await Client.DeleteAsync(ControllerAddress);

            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        public async Task Return400IfActionIsNotValid_OnGet()
        {
            //Arrange
            ControllerAddress = "/api/v1/followrequests/444?action=randomaction";

            AddValidHMACHeaders("DELETE");
            SetupMocksForHMACAuth();

            //Act
            var response = await Client.DeleteAsync(ControllerAddress);

            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}