using System.Net;
using System.Threading.Tasks;
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

        [TestCase(null)]
        [TestCase("")]
        public async Task Return400IfQueryIsEmpty_OnGet(string query)
        {
            //Arrange
            ControllerAddress = "/api/v1/search?query=" + query;

            AddValidHMACHeaders("GET");
            SetupMocksForHMACAuth();

            //Act
            var response = await Client.GetAsync(ControllerAddress);

            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            MockRepo.VerifyAll();
        }
    }
}