using Moq;
using NUnit.Framework;
using Zazz.Core.Interfaces;

namespace Zazz.UnitTests.Web.Controllers.Api
{
    [TestFixture]
    public class WeekliesControllerShould : BaseHMACTests
    {
        private int _weeklyId;
        private Mock<IWeeklyService> _weeklyService;

        public override void Init()
        {
            base.Init();
            
            _weeklyId = 21;
            ControllerAddress = "/api/v1/weeklies/" + _weeklyId;

            _weeklyService = MockRepo.Create<IWeeklyService>();
            IocContainer.Configure(x =>
                                   {
                                       x.For<IWeeklyService>().Use(_weeklyService.Object);
                                   });
        }
    }
}