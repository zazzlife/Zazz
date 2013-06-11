using NUnit.Framework;
using Zazz.Core.Interfaces;

namespace Zazz.UnitTests.Web.Controllers.Api
{
    [TestFixture]
    public class SearchControllerShould : BaseHMACTests
    {
        public override void Init()
        {
            base.Init();
            var query = "soroush";
            ControllerAddress = "/api/v1/search?query=" + query;


            IocContainer.Configure(x =>
            {
            });
        }
    }
}