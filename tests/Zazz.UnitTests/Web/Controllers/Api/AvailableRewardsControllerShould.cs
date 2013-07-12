using NUnit.Framework;

namespace Zazz.UnitTests.Web.Controllers.Api
{
    [TestFixture]
    public class AvailableRewardsControllerShould : BaseOAuthTests
    {
        public override void Init()
        {
            base.Init();

            ControllerAddress = "/api/v1/availablerewards";
        }
    }
}