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
using Zazz.Infrastructure;
using Zazz.Infrastructure.Services;
using Zazz.Web;
using Zazz.Web.DependencyResolution;
using Zazz.Web.Interfaces;

namespace Zazz.UnitTests.Web.Controllers.Api
{
    public abstract class BaseHMACTests
    {
        protected HttpSelfHostServer Server;
        protected HttpClient Client;
        protected MockRepository MockRepo;
        protected Mock<IUserService> UserService;
        protected Mock<IOAuthClientRepository> AppRepo;
        protected Mock<IPhotoService> PhotoService;
        protected OAuthClient App;
        protected CryptoService CryptoService;
        protected string Password;
        protected User User;
        protected IContainer IocContainer;
        protected string ControllerAddress;
        private const string AUTH_SCHEME = "ZazzApi";

        protected HttpMethod DefaultHttpMethod = HttpMethod.Get;

        [SetUp]
        public virtual void Init()
        {
            //Global api config
            
            App = Mother.GetApiApp();
            MockRepo = new MockRepository(MockBehavior.Strict);
            UserService = MockRepo.Create<IUserService>();
            AppRepo = MockRepo.Create<IOAuthClientRepository>();
            PhotoService = MockRepo.Create<IPhotoService>();
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

            Server = new HttpSelfHostServer(config);
            Client = new HttpClient(Server);
            Client.BaseAddress = new Uri(BASE_ADDRESS);
        }

        private IContainer BuildIoC()
        {
            return new Container(x =>
            {
                x.For<IFilterProvider>().Use<StructureMapFilterProvider>();
                x.For<ICryptoService>().Use<CryptoService>();
                x.For<IUserService>().Use(UserService.Object);
                x.For<IOAuthClientRepository>().Use(AppRepo.Object);
                x.For<IPhotoService>().Use(PhotoService.Object);
                x.For<IObjectMapper>().Use<ObjectMapper>();
                x.For<IDefaultImageHelper>().Use<DefaultImageHelper>()
                    .Ctor<string>("baseAddress").Is("test.zazzlife.com");
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

            Client.DefaultRequestHeaders.Date = date;
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AUTH_SCHEME, authHeader);
        }

        protected void SetupMocksForHMACAuth()
        {
            AppRepo.Setup(x => x.GetById(App.Id))
                   .Returns(App);
            UserService.Setup(x => x.GetUserPassword(User.Id))
                       .Returns(Encoding.UTF8.GetBytes(Password));

        }

        /******************************************************************
         * The following tests are testing HMACAuthorize on the controller.
         ******************************************************************/

        [Test]
        public async Task Return403IfDateHeaderIsNotProvided()
        {
            //Arrange
            Client.DefaultRequestHeaders.Date = null;

            //Act
            HttpResponseMessage result;
            if (DefaultHttpMethod == HttpMethod.Get)
            {
                result = await Client.GetAsync(ControllerAddress);
            }
            else
            {
                result = await Client.DeleteAsync(ControllerAddress);
            }

            //Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, result.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task Return403IfDateIsLessThan30MinutesAgo()
        {
            //Arrange
            Client.DefaultRequestHeaders.Date = DateTimeOffset.UtcNow.AddMinutes(-31);

            //Act
            HttpResponseMessage result;
            if (DefaultHttpMethod == HttpMethod.Get)
            {
                result = await Client.GetAsync(ControllerAddress);
            }
            else
            {
                result = await Client.DeleteAsync(ControllerAddress);
            }

            //Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, result.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task Return403IfDateIs30MinutesGreaterThanUTCNow()
        {
            //Arrange
            Client.DefaultRequestHeaders.Date = DateTimeOffset.UtcNow.AddSeconds(31);

            //Act
            HttpResponseMessage result;
            if (DefaultHttpMethod == HttpMethod.Get)
            {
                result = await Client.GetAsync(ControllerAddress);
            }
            else
            {
                result = await Client.DeleteAsync(ControllerAddress);
            }

            //Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, result.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task Return403IfAuthorizationHeaderIsMissing()
        {
            //Arrange
            Client.DefaultRequestHeaders.Date = DateTimeOffset.UtcNow;
            Client.DefaultRequestHeaders.Authorization = null;

            //Act
            HttpResponseMessage result;
            if (DefaultHttpMethod == HttpMethod.Get)
            {
                result = await Client.GetAsync(ControllerAddress);
            }
            else
            {
                result = await Client.DeleteAsync(ControllerAddress);
            }

            //Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, result.StatusCode);
            MockRepo.VerifyAll();
        }

        [TestCase("basic")]
        [TestCase("digest")]
        [TestCase("somethingelse")]
        public async Task Return403IfAuthorizationHeaderIsNotZazzApi(string authorizationScheme)
        {
            //Arrange
            Client.DefaultRequestHeaders.Date = DateTimeOffset.UtcNow;
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(authorizationScheme);

            //Act
            HttpResponseMessage result;
            if (DefaultHttpMethod == HttpMethod.Get)
            {
                result = await Client.GetAsync(ControllerAddress);
            }
            else
            {
                result = await Client.DeleteAsync(ControllerAddress);
            }

            //Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, result.StatusCode);
            MockRepo.VerifyAll();
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public async Task Return403IfAuthorizationHeaderValueIsMissing(string authorizationParam)
        {
            //Arrange
            Client.DefaultRequestHeaders.Date = DateTimeOffset.UtcNow;
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AUTH_SCHEME, authorizationParam);

            //Act
            HttpResponseMessage result;
            if (DefaultHttpMethod == HttpMethod.Get)
            {
                result = await Client.GetAsync(ControllerAddress);
            }
            else
            {
                result = await Client.DeleteAsync(ControllerAddress);
            }

            //Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, result.StatusCode);
            MockRepo.VerifyAll();
        }

        [TestCase("a")]
        [TestCase("a:b")]
        [TestCase("a:b:C")]
        public async Task Return403IfAuthorizationHeaderValueIsMissingAnyOfTheRequiredParameters(string auth)
        {
            //Arrange
            Client.DefaultRequestHeaders.Date = DateTimeOffset.UtcNow;
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AUTH_SCHEME, auth);

            //Act
            HttpResponseMessage result;
            if (DefaultHttpMethod == HttpMethod.Get)
            {
                result = await Client.GetAsync(ControllerAddress);
            }
            else
            {
                result = await Client.DeleteAsync(ControllerAddress);
            }

            //Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, result.StatusCode);
            MockRepo.VerifyAll();
        }

        [TestCase("text:sign:1:sign")]
        [TestCase("0:sign:1:sign")]
        [TestCase("-1:sign:1:sign")]
        public async Task Return403IfAppIdIsNotValidInteger(string auth)
        {
            //Arrange
            Client.DefaultRequestHeaders.Date = DateTimeOffset.UtcNow;
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AUTH_SCHEME, auth);

            //Act
            HttpResponseMessage result;
            if (DefaultHttpMethod == HttpMethod.Get)
            {
                result = await Client.GetAsync(ControllerAddress);
            }
            else
            {
                result = await Client.DeleteAsync(ControllerAddress);
            }

            //Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, result.StatusCode);
            MockRepo.VerifyAll();
        }

        [TestCase("1:sign:0:sign")]
        [TestCase("1:sign:text:sign")]
        [TestCase("1:sign:-1:sign")]
        public async Task Return403IfUserIdIsNotValidInteger(string auth)
        {
            //Arrange
            Client.DefaultRequestHeaders.Date = DateTimeOffset.UtcNow;
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AUTH_SCHEME, auth);

            //Act
            HttpResponseMessage result;
            if (DefaultHttpMethod == HttpMethod.Get)
            {
                result = await Client.GetAsync(ControllerAddress);
            }
            else
            {
                result = await Client.DeleteAsync(ControllerAddress);
            }

            //Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, result.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task Return403IfAppNotExists()
        {
            //Arrange

            AppRepo.Setup(x => x.GetById(App.Id))
                    .Returns(() => null);

            AddValidHMACHeaders(DefaultHttpMethod.Method);

            //Act
            HttpResponseMessage result;
            if (DefaultHttpMethod == HttpMethod.Get)
            {
                result = await Client.GetAsync(ControllerAddress);
            }
            else
            {
                result = await Client.DeleteAsync(ControllerAddress);
            }

            //Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, result.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task Return403IfRequestSignatureIsInvalid()
        {
            //Arrange
            Client.DefaultRequestHeaders.Date = DateTimeOffset.UtcNow;

            var authHeader = String.Format("{0}:{1}:{2}:{3}",
                App.Id,
                "Invalid Signature",
                User.Id,
                GetPasswordSignature());
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AUTH_SCHEME, authHeader);

            AppRepo.Setup(x => x.GetById(App.Id))
                   .Returns(App);

            //Act
            HttpResponseMessage result;
            if (DefaultHttpMethod == HttpMethod.Get)
            {
                result = await Client.GetAsync(ControllerAddress);
            }
            else
            {
                result = await Client.DeleteAsync(ControllerAddress);
            }

            //Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, result.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task Return403IfTheUserDoesNotExists()
        {
            //Arrange

            AppRepo.Setup(x => x.GetById(App.Id))
                   .Returns(App);
            UserService.Setup(x => x.GetUserPassword(User.Id))
                       .Returns(() => null);

            AddValidHMACHeaders(DefaultHttpMethod.Method);

            //Act
            HttpResponseMessage result;
            if (DefaultHttpMethod == HttpMethod.Get)
            {
                result = await Client.GetAsync(ControllerAddress);
            }
            else
            {
                result = await Client.DeleteAsync(ControllerAddress);
            }

            //Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, result.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task Return403IfThePasswordSignatureIsInvalid()
        {
            //Arrange
            Client.DefaultRequestHeaders.Date = DateTimeOffset.UtcNow;

            var authHeader = String.Format("{0}:{1}:{2}:{3}",
                App.Id,
                GetRequestSignature(DefaultHttpMethod.Method, Client.DefaultRequestHeaders.Date.Value.ToString("r"), ControllerAddress),
                User.Id,
                "InvalidPassSignature");
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AUTH_SCHEME, authHeader);

            AppRepo.Setup(x => x.GetById(App.Id))
                   .Returns(App);
            UserService.Setup(x => x.GetUserPassword(User.Id))
                       .Returns(() => Encoding.UTF8.GetBytes(Password));

            //Act
            HttpResponseMessage result;
            if (DefaultHttpMethod == HttpMethod.Get)
            {
                result = await Client.GetAsync(ControllerAddress);
            }
            else
            {
                result = await Client.DeleteAsync(ControllerAddress);
            }

            //Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, result.StatusCode);
            MockRepo.VerifyAll();
        }

        [TearDown]
        public void Cleanup()
        {
            Server.Dispose();
            Client.Dispose();
        }

    }
}