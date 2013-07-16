using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Zazz.Core.Interfaces;
using Zazz.Core.Models;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;
using Zazz.Web.Interfaces;
using Zazz.Web.Models;

namespace Zazz.UnitTests.Web.Controllers.Api
{
    [TestFixture]
    public class ProfileControllerShould : BaseOAuthTests
    {
        private Mock<IFollowService> _followService;
        private Mock<IFeedHelper> _feedHelper;

        public override void Init()
        {
            base.Init();
            ControllerAddress = "/api/v1/users/" + User.Id + "/profile";

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
            ControllerAddress = "/api/v1/users/0/profile";

            CreateValidAccessToken();

            //Act
            var result = await Client.GetAsync(ControllerAddress);

            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task Return404IfUserDoesNotExists_OnGet()
        {
            //Arrange
            CreateValidAccessToken();

            UserService.Setup(x => x.GetUser(User.Id, true, true, true, false))
                       .Returns(() => null);

            //Act
            var result = await Client.GetAsync(ControllerAddress);

            //Assert
            Assert.AreEqual(HttpStatusCode.NotFound, result.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task ReturnUser_OnGet()
        {
            //Arrange
            CreateValidAccessToken();

            User.AccountType = AccountType.User;
            User.ReceivedVotesCount = new UserReceivedVotes { Count = 15, LastUpdate = DateTime.UtcNow };
            User.UserDetail = new UserDetail();

            UserService.Setup(x => x.GetUser(User.Id, true, true, true, false))
                       .Returns(User);


            UserService.Setup(x => x.GetUserDisplayName(User.Id))
                       .Returns("display name");

            PhotoService.Setup(x => x.GetUserImageUrl(User.Id))
                        .Returns(new PhotoLinks("link"));

            PhotoService.Setup(x => x.GetLatestUserPhotos(User.Id, It.IsAny<int>()))
                        .Returns(new EnumerableQuery<Photo>(Enumerable.Empty<Photo>()));

            _feedHelper.Setup(x => x.GetUserActivityFeed(User.Id, User.Id, 0))
                       .Returns(new List<FeedViewModel>());

            _followService.Setup(x => x.GetFollowersCount(User.Id))
                          .Returns(15);

            //Act
            var result = await Client.GetAsync(ControllerAddress);

            //Assert
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            MockRepo.VerifyAll();
        }


    }
}