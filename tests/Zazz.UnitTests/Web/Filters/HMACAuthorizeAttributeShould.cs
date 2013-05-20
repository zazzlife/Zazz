using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dependencies;
using System.Web.Http.Filters;
using System.Web.Http.SelfHost;
using Moq;
using NUnit.Framework;
using StructureMap;
using Zazz.Core.Interfaces;
using Zazz.Web.DependencyResolution;
using Zazz.Web.Filters;

namespace Zazz.UnitTests.Web.Filters
{
    [TestFixture]
    public class HMACAuthorizeAttributeShould
    {
        private HttpSelfHostConfiguration _config;
        private HttpSelfHostServer _server;
        private HttpClient _client;
        private MockRepository _mockRepo;
        private Mock<ICryptoService> _cryptoService;
        private Mock<IApiAppRepository> _appRespo;
        private const string BASE_ADDRESS = "http://localhost:8080";

        [SetUp]
        public void Init()
        {
            _mockRepo = new MockRepository(MockBehavior.Strict);
            _cryptoService = _mockRepo.Create<ICryptoService>();
            _appRespo = _mockRepo.Create<IApiAppRepository>();

            var container = SetupIoC();
            _config = new HttpSelfHostConfiguration(BASE_ADDRESS);

            _config.Routes.MapHttpRoute(
                name: "Dummy Route",
                routeTemplate: "{controller}",
                defaults: new
                          {
                              controller = "Dummy"
                          });
            _config.DependencyResolver = new StructureMapDependencyResolver(container);

            _server = new HttpSelfHostServer(_config);
            _client = new HttpClient(_server) { BaseAddress = new Uri(BASE_ADDRESS) };
        }

        private IContainer SetupIoC()
        {
            ObjectFactory.Configure(x =>
                                    {
                                        x.Scan(s =>
                                               {
                                                   s.TheCallingAssembly();
                                                   s.WithDefaultConventions();
                                               });
                                        
                                        x.For<IFilterProvider>().Use<StructureMapFilterProvider>();
                                        x.For<ICryptoService>().Use(_cryptoService.Object);
                                        x.For<IApiAppRepository>().Use(_appRespo.Object);
                                    });

            return ObjectFactory.Container;
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

        [Test]
        public async Task Return403IfAuthorizationHeaderIsMissing()
        {
            //Arrange
            _client.DefaultRequestHeaders.Date = DateTimeOffset.UtcNow;
            _client.DefaultRequestHeaders.Authorization = null;

            //Act
            var result = await _client.GetAsync("");

            //Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, result.StatusCode);
        }

        [TestCase("basic")]
        [TestCase("digest")]
        [TestCase("somethingelse")]
        public async Task Return403IfAuthorizationHeaderIsNotZazzApi(string authorizationScheme)
        {
            //Arrange
            _client.DefaultRequestHeaders.Date = DateTimeOffset.UtcNow;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(authorizationScheme);

            //Act
            var result = await _client.GetAsync("");

            //Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, result.StatusCode);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public async Task Return403IfAuthorizationHeaderValueIsMissing(string authorizationParam)
        {
            //Arrange
            _client.DefaultRequestHeaders.Date = DateTimeOffset.UtcNow;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("ZazzApi", authorizationParam);

            //Act
            var result = await _client.GetAsync("");

            //Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, result.StatusCode);
        }

        [TestCase("a")]
        [TestCase("a:b")]
        [TestCase("a:b:C")]
        public async Task Return403IfAuthorizationHeaderValueIsMissingAnyOfTheRequiredParameters(string auth)
        {
            //Arrange
            _client.DefaultRequestHeaders.Date = DateTimeOffset.UtcNow;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("ZazzApi", auth);

            //Act
            var result = await _client.GetAsync("");

            //Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, result.StatusCode);

        }

        [TestCase("text:sign:1:sign")]
        [TestCase("0:sign:1:sign")]
        [TestCase("-1:sign:1:sign")]
        public async Task Return403IfAppIdIsNotValidInteger(string auth)
        {
            //Arrange
            _client.DefaultRequestHeaders.Date = DateTimeOffset.UtcNow;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("ZazzApi", auth);

            //Act
            var result = await _client.GetAsync("");

            //Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, result.StatusCode);

        }

        [TestCase("1:sign:0:sign")]
        [TestCase("1:sign:text:sign")]
        [TestCase("1:sign:-1:sign")]
        public async Task Return403IfUserIdIsNotValidInteger(string auth)
        {
            //Arrange
            _client.DefaultRequestHeaders.Date = DateTimeOffset.UtcNow;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("ZazzApi", auth);

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