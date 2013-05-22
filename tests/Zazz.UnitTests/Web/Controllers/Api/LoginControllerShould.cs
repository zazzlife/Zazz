using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using System.Web.Http.SelfHost;
using Moq;
using NUnit.Framework;
using StructureMap;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Infrastructure.Services;
using Zazz.Web;
using Zazz.Web.DependencyResolution;

namespace Zazz.UnitTests.Web.Controllers.Api
{
    [TestFixture]
    public class LoginControllerShould
    {
        private HttpSelfHostServer _server;
        private HttpClient _client;
        private string _username;
        private string _password;
        private string _loginUrl;
        private MockRepository _mockRepo;
        private Mock<ICryptoService> _cryptoService;
        private Mock<IUserService> _userService;
        private Mock<IApiAppRepository> _appRepo;
        private ApiApp _testApp;
        private const string BASE_ADDRESS = "http://localhost:8080";
        private const string AUTH_SCHEME = "ZazzApi";

        [SetUp]
        public void Init()
        {
            _testApp = new ApiApp
                       {
                           Id = 1,
                           PasswordSigningKey = Convert.FromBase64String("g0uq3k4Qe0cbKO+CSC+hOt+Gkba+cuIWTBk9G9RSmWpv5UTcsAxpWAHub2+PhXYWZsPGnvLQuyAF3hAbae827v8OIza+j1+Gq4mxOZpPZBwOhPZmIkpntUE5VyXBIYzsqFC966VywlQB8XxQMUUe4A8ziH3ux0qsudYL26IuODo="),
                           RequestSigningKey = Convert.FromBase64String("tsz7eTBzJEv9s3oOu9WsICuMbGZcaj8d80LFHjm9ZqmhCM71wIdd5iHcA6Mq/MO+n71qXKidQqrhKDg/aCwOzJrh0AnTLZkCcV/uKt7XmT8kF7dlpop2F/2QVqt9WQQV2hGHbgH4kdkQfQGGFXHI37lqlsWRyiQxaGuJa1hINj4=")
                       };

            _mockRepo = new MockRepository(MockBehavior.Strict);
            _userService = _mockRepo.Create<IUserService>();
            _appRepo = _mockRepo.Create<IApiAppRepository>();

            var config = new HttpSelfHostConfiguration(BASE_ADDRESS);
            JsonConfig.Configure(config);
            WebApiConfig.Register(config);

            _server = new HttpSelfHostServer(config);
            _client = new HttpClient(_server)
                      {
                          BaseAddress = new Uri(BASE_ADDRESS)
                      };

            _client.DefaultRequestHeaders.Date = DateTimeOffset.UtcNow;
            var iocContainer = BuildIoC();
            config.DependencyResolver = new StructureMapDependencyResolver(iocContainer);

            _username = "Soroush";
            _password = "123";
            _loginUrl = "/api/v1/login";
        }

        private IContainer BuildIoC()
        {
            return new Container(x =>
                                 {
                                     x.For<IFilterProvider>().Use<StructureMapFilterProvider>();
                                     x.For<ICryptoService>().Use<CryptoService>();
                                     x.For<IUserService>().Use(_userService.Object);
                                     x.For<IApiAppRepository>().Use(_appRepo.Object);
                                 });
        }

        private void AddUserNameAndPassToUrl(string username, string password)
        {
            _loginUrl += String.Format("?username={0}&password={1}", username, password);
        }

        private string CreateRequestSignature()
        {
            var stringToSign = "GET" + "\n" +
                               _client.DefaultRequestHeaders.Date.Value.ToString("r") + "\n" +
                               _loginUrl + "\n";

            var utf8Buffer = Encoding.UTF8.GetBytes(stringToSign);
            var cryptoService = new CryptoService();
            var signature = cryptoService.GenerateHMACSHA512Hash(utf8Buffer, _testApp.RequestSigningKey);
            return String.Format("{0}:{1}", _testApp.Id, signature);
        }

        [Test]
        public async Task Return403IfAuthorizationHeaderIsMissing()
        {
            //Arrange
            AddUserNameAndPassToUrl(_username, _password);

            //Act
            var response = await _client.GetAsync(_loginUrl);

            //Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
            _mockRepo.VerifyAll();
        }

        [Test]
        public async Task Return404IfRequestSignatureIsOkButUserDoesNotExists()
        {
            //Arrange
            AddUserNameAndPassToUrl(_username, _password);
            var authSignature = CreateRequestSignature();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AUTH_SCHEME, authSignature);

            _appRepo.Setup(x => x.GetById(_testApp.Id))
                    .Returns(_testApp);
            _userService.Setup(x => x.GetUser(_username, true, true, false))
                        .Returns(() => null);

            //Act
            var response = await _client.GetAsync(_loginUrl);

            //Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            _mockRepo.VerifyAll();
        }

        [TearDown]
        public void Cleanup()
        {
            _server.Dispose();
            _client.Dispose();
        }
    }
}