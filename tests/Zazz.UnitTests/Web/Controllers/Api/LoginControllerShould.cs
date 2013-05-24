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
    public class LoginControllerShould
    {
        private HttpSelfHostServer _server;
        private HttpClient _client;
        private string _username;
        private string _password;
        private string _loginUrl;
        private MockRepository _mockRepo;
        private Mock<IUserService> _userService;
        private Mock<IApiAppRepository> _appRepo;
        private ApiApp _testApp;
        private CryptoService _cryptoService;
        private LoginApiRequest _loginRequest;
        private Mock<IPhotoService> _photoService;
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
            _photoService = _mockRepo.Create<IPhotoService>();

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

            _username = "Soroush";
            _password = "123";
            _loginUrl = "/api/v1/login";

            _loginRequest = new LoginApiRequest
                            {
                                AppId = _testApp.Id,
                                Password = _password,
                                Username = _username
                            };
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
                                 });
        }

        private string CreateRequestSignature(string body)
        {
            var stringToSign = "POST" + "\n" +
                               _client.DefaultRequestHeaders.Date.Value.ToString("r") + "\n" +
                               _loginUrl + "\n" +
                               body;

            var utf8Buffer = Encoding.UTF8.GetBytes(stringToSign);
            var signature = _cryptoService.GenerateHMACSHA512Hash(utf8Buffer, _testApp.RequestSigningKey);
            return String.Format("{0}:{1}", _testApp.Id, signature);
        }

        [Test]
        public async Task Return403IfAuthorizationHeaderIsMissing()
        {
            //Arrange
            var postContent = new StringContent(JsonConvert.SerializeObject(_loginRequest));

            //Act
            var response = await _client.PostAsync(_loginUrl, postContent);

            //Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
            _mockRepo.VerifyAll();
        }

        [Test]
        public async Task Return404IfRequestSignatureIsOkButUserDoesNotExists()
        {
            //Arrange
            var bodyContent = JsonConvert.SerializeObject(_loginRequest);

            var authSignature = CreateRequestSignature(bodyContent);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AUTH_SCHEME, authSignature);

            _appRepo.Setup(x => x.GetById(_testApp.Id))
                    .Returns(_testApp);
            _userService.Setup(x => x.GetUser(_username, true, true, false, false))
                        .Returns(() => null);

            //Act
            var response = await _client.PostAsync(_loginUrl, new StringContent(bodyContent, Encoding.UTF8, "application/json"));

            //Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            _mockRepo.VerifyAll();
        }

        [Test]
        public async Task Return401IfPasswordIsIncorrect()
        {
            //Arrange
            string iv;
            var user = new User
                       {
                           Password = _cryptoService.EncryptPassword("some other pass", out iv),
                           PasswordIV = Convert.FromBase64String(iv)
                       };

            var bodyContent = JsonConvert.SerializeObject(_loginRequest);
            
            var authSignature = CreateRequestSignature(bodyContent);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AUTH_SCHEME, authSignature);

            _appRepo.Setup(x => x.GetById(_testApp.Id))
                    .Returns(_testApp);
            _userService.Setup(x => x.GetUser(_username, true, true, false, false))
                        .Returns(() => user);

            //Act
            var response = await _client.PostAsync(_loginUrl, new StringContent(bodyContent, Encoding.UTF8, "application/json"));

            //Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            _mockRepo.VerifyAll();
        }

        [Test]
        public async Task Return200WhenEverythingIsOk()
        {
            //Arrange
            string iv;
            var user = new User
            {
                Id = 25,
                AccountType = AccountType.User,
                Username = _username,
                Password = _cryptoService.EncryptPassword(_password, out iv),
                PasswordIV = Convert.FromBase64String(iv),
                IsConfirmed = true
            };

            var passwordHash =
                _cryptoService.GenerateHMACSHA512Hash(Encoding.UTF8.GetBytes(_password),
                                                      _testApp.PasswordSigningKey);

            _loginRequest.Password = passwordHash;
            var bodyContent = JsonConvert.SerializeObject(_loginRequest);
            var authSignature = CreateRequestSignature(bodyContent);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AUTH_SCHEME, authSignature);

            _appRepo.Setup(x => x.GetById(_testApp.Id))
                    .Returns(_testApp);
            _userService.Setup(x => x.GetUser(_username, true, true, false, false))
                        .Returns(() => user);

            _userService.Setup(x => x.GetUserDisplayName(user.Id))
                        .Returns(_username);

            _photoService.Setup(x => x.GetUserImageUrl(user.Id))
                         .Returns(new PhotoLinks("testUrl"));
            
            //Act
            var response = await _client.PostAsync(_loginUrl, new StringContent(bodyContent, Encoding.UTF8, "application/json"));

            //Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
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