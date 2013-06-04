using System.Net.Http;
using NUnit.Framework;
using Zazz.Core.Interfaces;

namespace Zazz.UnitTests.Web.Controllers.Api
{
    [TestFixture]
    public class FollowersControllerShould : BaseHMACTests
    {
        private int _userId;

        public override void Init()
        {
            DefaultHttpMethod = HttpMethod.Delete;

            base.Init();

            _userId = 5;
            ControllerAddress = "/api/v1/followers/" + _userId;

            IocContainer.Configure(x =>
            {
            });
        }
    }
}