using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using System.Web.Http.SelfHost;
using Moq;
using NUnit.Framework;
using Newtonsoft.Json;
using StructureMap;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Web;
using Zazz.Web.DependencyResolution;
using Zazz.Web.Interfaces;
using Zazz.Infrastructure;
using Zazz.Web.OAuthAuthorizationServer;

namespace Zazz.UnitTests.Web.Controllers.Api
{
    public abstract class BaseOAuthTests
    {
        protected HttpSelfHostServer Server;
        protected HttpClient Client;
        protected MockRepository MockRepo;
        protected User User;
        protected IContainer IocContainer;
        protected string ControllerAddress;
        protected JWT AccessToken;

        [SetUp]
        public virtual void Init()
        {
            MockRepo = new MockRepository(MockBehavior.Strict);

            User = new User
            {
                Id = 12,
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
                x.For<IObjectMapper>().Use<ObjectMapper>();
                x.For<IDefaultImageHelper>().Use<DefaultImageHelper>()
                    .Ctor<string>("baseAddress").Is("test.zazzlife.com");
            });
        }

        [Test]
        public async Task ReturnInvalidGrantIfAuthHeaderIsMissing()
        {
            //Arrange

            Client.DefaultRequestHeaders.Authorization = null;

            //Act
            var response = await Client.GetAsync(ControllerAddress);
            var content = await response.Content.ReadAsStringAsync();
            var error = JsonConvert.DeserializeObject<OAuthErrorModel>(content);

            //Assert
            Assert.AreEqual(OAuthError.InvalidGrant, error.Error);
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public async Task ReturnInvalidGrantIfAccessTokenIsMissing(string accessToken)
        {
            //Arrange
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", accessToken);

            //Act
            var response = await Client.GetAsync(ControllerAddress);
            var content = await response.Content.ReadAsStringAsync();
            var error = JsonConvert.DeserializeObject<OAuthErrorModel>(content);

            //Assert
            Assert.AreEqual(OAuthError.InvalidGrant, error.Error);
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        public async Task ReturnInvalidGrantIfAccessTokenSignatureIsInvalid()
        {
            //Arrange
            AccessToken = new JWT
            {
                UserId = 1,
                ClientId = 2,
                ExpirationDate = DateTime.UtcNow.AddHours(1),
                Scopes = new List<string> { "full" },
                TokenType = JWT.ACCESS_TOKEN_TYPE,
                IssuedDate = DateTime.UtcNow,
            };

            //chaning the signature
            var accessTokenSegments = AccessToken.ToJWTString().Split('.');
            accessTokenSegments[2] = "invalid signature";

            var token = String.Join(".", accessTokenSegments);

            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);

            //Act
            var response = await Client.GetAsync(ControllerAddress);
            var content = await response.Content.ReadAsStringAsync();
            var error = JsonConvert.DeserializeObject<OAuthErrorModel>(content);

            //Assert
            Assert.AreEqual(OAuthError.InvalidGrant, error.Error);
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        public async Task ReturnInvalidGrantIfAccessTokenIsExpired()
        {
            //Arrange
            var AccessToken = new JWT
            {
                UserId = 1,
                ClientId = 2,
                ExpirationDate = DateTime.UtcNow.AddHours(-1),
                Scopes = new List<string> { "full" },
                TokenType = JWT.ACCESS_TOKEN_TYPE,
                IssuedDate = DateTime.UtcNow,
            };

            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer",
                                                                                        AccessToken.ToJWTString());

            //Act
            var response = await Client.GetAsync(ControllerAddress);
            var content = await response.Content.ReadAsStringAsync();
            var error = JsonConvert.DeserializeObject<OAuthErrorModel>(content);

            //Assert
            Assert.AreEqual(OAuthError.InvalidGrant, error.Error);
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        public async Task ReturnInvalidGrantIfRequiredScopeIsNotGranted()
        {
            //Arrange
            var AccessToken = new JWT
            {
                UserId = 1,
                ClientId = 2,
                ExpirationDate = DateTime.UtcNow.AddHours(1),
                Scopes = new List<string> { "some other scope!" },
                TokenType = JWT.ACCESS_TOKEN_TYPE,
                IssuedDate = DateTime.UtcNow,
            };

            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer",
                                                                                        AccessToken.ToJWTString());

            //Act
            var response = await Client.GetAsync(ControllerAddress);
            var content = await response.Content.ReadAsStringAsync();
            var error = JsonConvert.DeserializeObject<OAuthErrorModel>(content);

            //Assert
            Assert.AreEqual(OAuthError.InvalidScope, error.Error);
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}