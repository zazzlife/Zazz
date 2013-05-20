using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.SelfHost;
using NUnit.Framework;
using Zazz.Web.Filters;

namespace Zazz.UnitTests.Web.Filters
{
    [TestFixture]
    public class HMACAuthorizeAttributeShould
    {
        private HttpSelfHostConfiguration _config;
        private HttpSelfHostServer _server;
        private HttpClient _client;
        private const string BASE_ADDRESS = "http://localhost:8080";

        [SetUp]
        public void Init()
        {
            _config = new HttpSelfHostConfiguration(BASE_ADDRESS);

            _config.Routes.MapHttpRoute(
                name: "Dummy Route",
                routeTemplate: "{controller}",
                defaults: new
                          {
                              controller = "Dummy"
                          });

            _server = new HttpSelfHostServer(_config);
            _client = new HttpClient(_server) { BaseAddress = new Uri(BASE_ADDRESS) };
        }

        [Test]
        public async Task Return403IfDateHeaderIsNotProvided()
        {
            //Arrange
            _client.DefaultRequestHeaders.Date = null;

            //Act
            var result = await _client.GetAsync("");

            //Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, result.StatusCode);
        }

        [Test]
        public async Task Return403IfDateIsLessThanOneMinutesAgo()
        {
            //Arrange
            _client.DefaultRequestHeaders.Date = DateTimeOffset.UtcNow.AddMinutes(-1);

            //Act
            var result = await _client.GetAsync("");

            //Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, result.StatusCode);
        }

        [Test]
        public async Task Return403IfDateIsGreaterThanUTCNow()
        {
            //Arrange
            _client.DefaultRequestHeaders.Date = DateTimeOffset.UtcNow.AddSeconds(2);

            //Act
            var result = await _client.GetAsync("");

            //Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, result.StatusCode);
        }


        [TearDown]
        public void Cleanup()
        {
            _server.Dispose();
            _client.Dispose();
        }
    }

    public class DummyController : ApiController
    {
        [HMACAuthorize]
        public string Get()
        {
            return String.Empty;
        }
    }
}