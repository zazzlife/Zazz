using Moq;
using NUnit.Framework;
using Zazz.Core.Interfaces;

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
    }
}