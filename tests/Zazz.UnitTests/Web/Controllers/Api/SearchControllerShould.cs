using System.Linq;
using System.Net;
using System.Threading.Tasks;
using NUnit.Framework;
using Zazz.Core.Interfaces;
using Zazz.Core.Models;

namespace Zazz.UnitTests.Web.Controllers.Api
{
    [TestFixture]
    public class SearchControllerShould : BaseHMACTests
    {
        private string _query;

        public override void Init()
        {
            base.Init();
            _query = "soroush";
            ControllerAddress = "/api/v1/search?query=" + _query;


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

        [Test]
        public async Task ReturnResult_OnGet()
        {
            //Arrange

            UserService.Setup(x => x.Search(_query))
                       .Returns(Enumerable.Empty<UserSearchResult>());

            AddValidHMACHeaders("GET");
            SetupMocksForHMACAuth();

            //Act
            var response = await Client.GetAsync(ControllerAddress);

            //Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            MockRepo.VerifyAll();

        }


    }
}