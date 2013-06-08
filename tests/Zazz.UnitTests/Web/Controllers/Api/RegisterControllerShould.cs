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
using Newtonsoft.Json;
using StructureMap;
using Zazz.Core.Interfaces;
using Zazz.Core.Models;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;
using Zazz.Infrastructure.Services;
using Zazz.Web;
using Zazz.Web.DependencyResolution;
using Zazz.Web.Models.Api;

namespace Zazz.UnitTests.Web.Controllers.Api
{
    [TestFixture]
    public class RegisterControllerShould
    {
        private HttpSelfHostServer _server;
        private HttpClient _client;
        private string _registerUrl;
        private MockRepository _mockRepo;
        private Mock<IUserService> _userService;
        private Mock<IApiAppRepository> _appRepo;
        private ApiApp _testApp;
        private CryptoService _cryptoService;
        private Mock<IPhotoService> _photoService;
        private Mock<IAppRequestTokenService> _appRequestTokenService;
        private const string BASE_ADDRESS = "http://localhost:8080";
        private const string AUTH_SCHEME = "ZazzApi";

        [SetUp]
        public void Init()
        {
            _testApp = Mother.GetApiApp();

            _mockRepo = new MockRepository(MockBehavior.Strict);
            _userService = _mockRepo.Create<IUserService>();
            _appRepo = _mockRepo.Create<IApiAppRepository>();
            _photoService = _mockRepo.Create<IPhotoService>();
            _appRequestTokenService = _mockRepo.Create<IAppRequestTokenService>();
            

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

            _cryptoService = new CryptoService();
            _registerUrl = "/api/v1/register";
        }

        private IContainer BuildIoC()
        {
            return new Container(x =>
            {
                x.For<IFilterProvider>().Use<StructureMapFilterProvider>();
                x.For<ICryptoService>().Use<CryptoService>();
                x.For<IUserService>().Use(_userService.Object);
                x.For<IApiAppRepository>().Use(_appRepo.Object);
                x.For<IPhotoService>().Use(_photoService.Object);
                x.For<IAppRequestTokenService>().Use(_appRequestTokenService.Object);
            });
        }

        private string CreateRequestSignature()
        {
            var stringToSign = "GET" + "\n" +
                               _client.DefaultRequestHeaders.Date.Value.ToString("r") + "\n" +
                               _registerUrl + "\n";

            var utf8Buffer = Encoding.UTF8.GetBytes(stringToSign);
            var signature = _cryptoService.GenerateHMACSHA512Hash(utf8Buffer, _testApp.RequestSigningKey);
            return String.Format("{0}:{1}", _testApp.Id, signature);
        }

        [Test]
        public async Task Return403IfAuthorizationHeaderIsMissing()
        {
            //Arrange
            //Act
            var response = await _client.GetAsync(_registerUrl);

            //Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
            _mockRepo.VerifyAll();
        }

        [Test]
        public async Task Return403IfDateHeaderIsInvalid()
        {
            //Arrange
            _client.DefaultRequestHeaders.Date = DateTimeOffset.UtcNow.AddMinutes(-2);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AUTH_SCHEME,
                                                                                        CreateRequestSignature());

            //Act
            var response = await _client.GetAsync(_registerUrl);

            //Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
            _mockRepo.VerifyAll();
        }

        [Test]
        public async Task Return403IfRequestSignatureIsInvalid()
        {
            //Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AUTH_SCHEME,
                                                                                        "invalid sign");

            //Act
            var response = await _client.GetAsync(_registerUrl);

            //Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
            _mockRepo.VerifyAll();
        }
    }
}