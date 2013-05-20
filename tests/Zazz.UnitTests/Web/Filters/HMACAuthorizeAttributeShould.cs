using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
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
using Zazz.Core.Models.Data;
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
        private Mock<IApiAppRepository> _appRepo;
        private int _appId;
        private string _requestSignature;
        private int _usreId;
        private string _passwordSignature;
        private byte[] _passwordSigningKey;
        private byte[] _requestSigningKey;
        private ApiApp _app;
        private const string BASE_ADDRESS = "http://localhost:8080";
        private const string AUTH_SCHEME = "ZazzApi";


        [SetUp]
        public void Init()
        {
            _mockRepo = new MockRepository(MockBehavior.Strict);
            _cryptoService = _mockRepo.Create<ICryptoService>();
            _appRepo = _mockRepo.Create<IApiAppRepository>();

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

            _appId = 12;
            _requestSignature = "requestSign";
            _usreId = 1000;
            _passwordSignature = "passSign";
            _requestSigningKey = new byte[] {1, 2};
            _passwordSigningKey = new byte[] { 3, 4 };

            _app = new ApiApp
                   {
                       Id = _appId,
                       Name = "test",
                       PasswordSigningKey = _passwordSigningKey,
                       RequestSigningKey = _requestSigningKey
                   };
        }

        private IContainer SetupIoC()
        {
            return new Container(x =>
                                    {
                                        x.Scan(s =>
                                               {
                                                   s.TheCallingAssembly();
                                                   s.WithDefaultConventions();
                                               });
                                        
                                        x.For<IFilterProvider>().Use<StructureMapFilterProvider>();
                                        x.For<ICryptoService>().Use(_cryptoService.Object);
                                        x.For<IApiAppRepository>().Use(_appRepo.Object);
                                    });
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
            _mockRepo.VerifyAll();
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
            _mockRepo.VerifyAll();
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
            _mockRepo.VerifyAll();
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
            _mockRepo.VerifyAll();
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
            _mockRepo.VerifyAll();
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public async Task Return403IfAuthorizationHeaderValueIsMissing(string authorizationParam)
        {
            //Arrange
            _client.DefaultRequestHeaders.Date = DateTimeOffset.UtcNow;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AUTH_SCHEME, authorizationParam);

            //Act
            var result = await _client.GetAsync("");

            //Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, result.StatusCode);
            _mockRepo.VerifyAll();
        }

        [TestCase("a")]
        [TestCase("a:b")]
        [TestCase("a:b:C")]
        public async Task Return403IfAuthorizationHeaderValueIsMissingAnyOfTheRequiredParameters(string auth)
        {
            //Arrange
            _client.DefaultRequestHeaders.Date = DateTimeOffset.UtcNow;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AUTH_SCHEME, auth);

            //Act
            var result = await _client.GetAsync("");

            //Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, result.StatusCode);
            _mockRepo.VerifyAll();
        }

        [TestCase("text:sign:1:sign")]
        [TestCase("0:sign:1:sign")]
        [TestCase("-1:sign:1:sign")]
        public async Task Return403IfAppIdIsNotValidInteger(string auth)
        {
            //Arrange
            _client.DefaultRequestHeaders.Date = DateTimeOffset.UtcNow;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AUTH_SCHEME, auth);

            //Act
            var result = await _client.GetAsync("");

            //Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, result.StatusCode);
            _mockRepo.VerifyAll();
        }

        [TestCase("1:sign:0:sign")]
        [TestCase("1:sign:text:sign")]
        [TestCase("1:sign:-1:sign")]
        public async Task Return403IfUserIdIsNotValidInteger(string auth)
        {
            //Arrange
            _client.DefaultRequestHeaders.Date = DateTimeOffset.UtcNow;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AUTH_SCHEME, auth);

            //Act
            var result = await _client.GetAsync("");

            //Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, result.StatusCode);
            _mockRepo.VerifyAll();
        }

        [Test]
        public async Task Return403IfAppNotExists()
        {
            //Arrange
            var authHeader = String.Format("{0}:{1}:{2}:{3}"
                , _appId, "invalidSign", _usreId, _passwordSignature);

            _client.DefaultRequestHeaders.Date = DateTimeOffset.UtcNow;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AUTH_SCHEME, authHeader);

            _appRepo.Setup(x => x.GetById(_appId))
                    .Returns(() => null);

            //Act
            var result = await _client.GetAsync("");

            //Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, result.StatusCode);
            _mockRepo.VerifyAll();
        }

        [Test]
        public async Task Return403IfRequestSignatureIsInvalidForGetRequest()
        {
            //Arrange
            var authHeader = String.Format("{0}:{1}:{2}:{3}"
                , _appId, "invalidSign", _usreId, _passwordSignature);

            _client.DefaultRequestHeaders.Date = DateTimeOffset.UtcNow;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AUTH_SCHEME, authHeader);

            var stringToSign = "GET" + "\n" +
                               _client.DefaultRequestHeaders.Date.Value.ToString("r") + "\n" +
                               "/" + "\n";

            var signBuffer = Encoding.UTF8.GetBytes(stringToSign);
            _appRepo.Setup(x => x.GetById(_appId))
                    .Returns(_app);
            _cryptoService.Setup(x => x.GenerateHMACSHA512Hash(signBuffer, _requestSigningKey))
                          .Returns(_requestSignature);

            //Act
            var result = await _client.GetAsync("");

            //Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, result.StatusCode);
            _mockRepo.VerifyAll();
        }

        [Test]
        public async Task Return403IfRequestSignatureIsInvalidForPostRequest()
        {
            //Arrange
            var authHeader = String.Format("{0}:{1}:{2}:{3}"
                , _appId, "invalidSign", _usreId, _passwordSignature);

            _client.DefaultRequestHeaders.Date = DateTimeOffset.UtcNow;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AUTH_SCHEME, authHeader);

            var body = "body";

            var stringToSign = "POST" + "\n" +
                               _client.DefaultRequestHeaders.Date.Value.ToString("r") + "\n" +
                               "/" + "\n" +
                               body;

            var signBuffer = Encoding.UTF8.GetBytes(stringToSign);
            _appRepo.Setup(x => x.GetById(_appId))
                    .Returns(_app);
            _cryptoService.Setup(x => x.GenerateHMACSHA512Hash(signBuffer, _requestSigningKey))
                          .Returns(_requestSignature);

            //Act
            var result = await _client.PostAsync("", new StringContent(body));

            //Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, result.StatusCode);
            _mockRepo.VerifyAll();
        }

        [Test]
        public async Task Return403IfRequestSignatureIsInvalidForPutRequest()
        {
            //Arrange
            var authHeader = String.Format("{0}:{1}:{2}:{3}"
                , _appId, "invalidSign", _usreId, _passwordSignature);

            _client.DefaultRequestHeaders.Date = DateTimeOffset.UtcNow;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AUTH_SCHEME, authHeader);

            var body = "body";

            var stringToSign = "PUT" + "\n" +
                               _client.DefaultRequestHeaders.Date.Value.ToString("r") + "\n" +
                               "/?id=25" + "\n" +
                               body;

            var signBuffer = Encoding.UTF8.GetBytes(stringToSign);
            _appRepo.Setup(x => x.GetById(_appId))
                    .Returns(_app);
            _cryptoService.Setup(x => x.GenerateHMACSHA512Hash(signBuffer, _requestSigningKey))
                          .Returns(_requestSignature);
            //Act
            var result = await _client.PutAsync("/?id=25", new StringContent(body));

            //Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, result.StatusCode);
            _mockRepo.VerifyAll();
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

        [HMACAuthorize]
        public int Post()
        {
            return 1;
        }

        [HMACAuthorize]
        public void Put()
        {
        }

        [HMACAuthorize]
        public void Delete()
        {
        }
    }
}