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
    public abstract class BaseHMACTests
    {
        protected HttpSelfHostServer _server;
        protected HttpClient _client;
        protected MockRepository _mockRepo;
        protected Mock<IUserService> UserService;
        protected Mock<IApiAppRepository> AppRepo;
        protected Mock<IPhotoService> PhotoService;
        protected ApiApp App;
        protected CryptoService CryptoService;
        protected string Password;
        protected User User;
        protected IContainer IocContainer;
        protected string ControllerAddress;
        private const string AUTH_SCHEME = "ZazzApi";


        [SetUp]
        public virtual void Init()
        {
            //Global api config

            App = Mother.GetApiApp();
            _mockRepo = new MockRepository(MockBehavior.Strict);
            UserService = _mockRepo.Create<IUserService>();
            AppRepo = _mockRepo.Create<IApiAppRepository>();
            PhotoService = _mockRepo.Create<IPhotoService>();
            CryptoService = new CryptoService();

            Password = "password";
            string iv;
            var passwordCipher = CryptoService.EncryptPassword(Password, out iv);

            User = new User
            {
                Id = 12,
                Password = passwordCipher,
                PasswordIV = Convert.FromBase64String(iv)
            };

            const string BASE_ADDRESS = "http://localhost:8080";
            var config = new HttpSelfHostConfiguration(BASE_ADDRESS);
            JsonConfig.Configure(config);
            WebApiConfig.Register(config);
            IocContainer = BuildIoC();
            config.DependencyResolver = new StructureMapDependencyResolver(IocContainer);

            _server = new HttpSelfHostServer(config);
            _client = new HttpClient(_server);
            _client.BaseAddress = new Uri(BASE_ADDRESS);
        }

        private IContainer BuildIoC()
        {
            return new Container(x =>
            {
                x.For<IFilterProvider>().Use<StructureMapFilterProvider>();
                x.For<ICryptoService>().Use<CryptoService>();
                x.For<IUserService>().Use(UserService.Object);
                x.For<IApiAppRepository>().Use(AppRepo.Object);
                x.For<IPhotoService>().Use(PhotoService.Object);
            });
        }

        private string GetPasswordSignature()
        {
            var passwordBuffer = Encoding.UTF8.GetBytes(Password);
            return CryptoService.GenerateHMACSHA512Hash(passwordBuffer, App.PasswordSigningKey);
        }

        private string GetRequestSignature(string method, string date, string path, string body = null)
        {
            var request = method + "\n" +
                          date + "\n" +
                          path + "\n" +
                          body;

            return CryptoService.GenerateHMACSHA512Hash(Encoding.UTF8.GetBytes(request),App.RequestSigningKey);
        }

        protected void AddValidHMACHeaders(string method, string path = null, string body = null)
        {
            if (path == null)
                path = ControllerAddress;

            var date = DateTime.UtcNow;
            var requestSignature = GetRequestSignature(method, date.ToString("r"), path, body);
            var passSignature = GetPasswordSignature();

            var authHeader = String.Format("{0}:{1}:{2}:{3}",
                                           App.Id, requestSignature, User.Id, passSignature);

            _client.DefaultRequestHeaders.Date = date;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AUTH_SCHEME, authHeader);
        }

        /******************************************************************
         * The following tests are testing HMACAuthorize on the controller.
         ******************************************************************/

        [Test]
        public async Task Return403IfDateHeaderIsNotProvided()
        {
            //Arrange
            _client.DefaultRequestHeaders.Date = null;

            //Act
            var result = await _client.GetAsync(ControllerAddress);

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
            var result = await _client.GetAsync(ControllerAddress);

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
            var result = await _client.GetAsync(ControllerAddress);

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
            var result = await _client.GetAsync(ControllerAddress);

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
            var result = await _client.GetAsync(ControllerAddress);

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
            var result = await _client.GetAsync(ControllerAddress);

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
            var result = await _client.GetAsync(ControllerAddress);

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
            var result = await _client.GetAsync(ControllerAddress);

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
            var result = await _client.GetAsync(ControllerAddress);

            //Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, result.StatusCode);
            _mockRepo.VerifyAll();
        }

        [Test]
        public async Task Return403IfAppNotExists()
        {
            //Arrange
            AddValidHMACHeaders("GET");

            AppRepo.Setup(x => x.GetById(App.Id))
                    .Returns(() => null);

            //Act
            var result = await _client.GetAsync(ControllerAddress);

            //Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, result.StatusCode);
            _mockRepo.VerifyAll();
        }

        [Test]
        public async Task Return403IfRequestSignatureIsInvalid()
        {
            //Arrange
            _client.DefaultRequestHeaders.Date = DateTimeOffset.UtcNow;

            var authHeader = String.Format("{0}:{1}:{2}:{3}",
                App.Id,
                "Invalid Signature",
                User.Id,
                GetPasswordSignature());
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AUTH_SCHEME, authHeader);

            AppRepo.Setup(x => x.GetById(App.Id))
                   .Returns(App);

            //Act
            var result = await _client.GetAsync(ControllerAddress);

            //Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, result.StatusCode);
            _mockRepo.VerifyAll();
        }

        [Test]
        public async Task Return403IfTheUserDoesNotExists()
        {
            //Arrange
            AddValidHMACHeaders("GET");

            AppRepo.Setup(x => x.GetById(App.Id))
                   .Returns(App);
            UserService.Setup(x => x.GetUserPassword(User.Id))
                       .Returns(() => null);

            //Act
            var result = await _client.GetAsync(ControllerAddress);

            //Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, result.StatusCode);
            _mockRepo.VerifyAll();
        }

        [Test]
        public async Task Return403IfThePasswordSignatureIsInvalid()
        {
            //Arrange
            _client.DefaultRequestHeaders.Date = DateTimeOffset.UtcNow;

            var authHeader = String.Format("{0}:{1}:{2}:{3}",
                App.Id,
                GetRequestSignature("GET", _client.DefaultRequestHeaders.Date.Value.ToString("r"), ControllerAddress),
                User.Id,
                "InvalidPassSignature");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AUTH_SCHEME, authHeader);

            AppRepo.Setup(x => x.GetById(App.Id))
                   .Returns(App);
            UserService.Setup(x => x.GetUserPassword(User.Id))
                       .Returns(() => Encoding.UTF8.GetBytes(Password));

            //Act
            var result = await _client.GetAsync(ControllerAddress);

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
}