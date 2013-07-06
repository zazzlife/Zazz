using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.SelfHost;
using Moq;
using NUnit.Framework;
using Newtonsoft.Json;
using StructureMap;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Web;
using Zazz.Web.Controllers.Api;
using Zazz.Web.DependencyResolution;
using System.Net;
using Zazz.Web.OAuthAuthorizationServer;

namespace Zazz.UnitTests.Web.Controllers.Api
{
    [TestFixture]
    public class TokenControllerShould
    {
        private HttpSelfHostConfiguration _configuration;
        private HttpClient _client;
        private MockRepository _mockRepo;
        private Mock<IOAuthService> _oauthService;
        private Mock<IPhotoService> _photoService;
        private Mock<IUserService> _userService;
        private Mock<IOAuthClientRepository> _oauthClientRepo;

        [SetUp]
        public void Init()
        {
            _mockRepo = new MockRepository(MockBehavior.Strict);
            _oauthService = _mockRepo.Create<IOAuthService>();
            _userService = _mockRepo.Create<IUserService>();
            _photoService = _mockRepo.Create<IPhotoService>();
            _oauthClientRepo = _mockRepo.Create<IOAuthClientRepository>();

            const string BASE_ADDRESS = "http://localhost:8080";
            _configuration = new HttpSelfHostConfiguration(BASE_ADDRESS);

            JsonConfig.Configure(_configuration);
            WebApiConfig.Register(_configuration);

            var iocContainer = new Container(x =>
                                             {
                                                 x.For<IOAuthService>().Use(_oauthService.Object);
                                                 x.For<IUserService>().Use(_userService.Object);
                                                 x.For<IPhotoService>().Use(_photoService.Object);
                                                 x.For<IOAuthClientRepository>().Use(_oauthClientRepo.Object);
                                             });

            _configuration.DependencyResolver = new StructureMapDependencyResolver(iocContainer);

            var server = new HttpSelfHostServer(_configuration);
            _client = new HttpClient(server)
                      {
                          BaseAddress = new Uri(BASE_ADDRESS)
                      };
        }

        [Test]
        public async Task Return400IfRequestParametersAreMissing_OnPost()
        {
            //Arrange
            var path = "/api/v1/token";

            //Act
            var response = await _client.PostAsync(path, null);
            var error = JsonConvert.DeserializeObject<OAuthErrorModel>(await response.Content.ReadAsStringAsync());

            //Assert
            Assert.AreEqual(OAuthError.InvalidRequest, error.Error);
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            _mockRepo.VerifyAll();
        }

        #region greant_type=Password

        [Test]
        public async Task Return400IfPasswordIsMissingAndGrantTypeIsPassword_OnPost()
        {
            //Arrange
            var path = "/api/v1/token";

            var values = new List<KeyValuePair<string, string>>
                         {
                             new KeyValuePair<string, string>("grant_type", "password"),
                             new KeyValuePair<string, string>("username", "user"),
                             new KeyValuePair<string, string>("scope", "full"),
                         };


            //Act
            var response = await _client.PostAsync(path, new FormUrlEncodedContent(values));
            var error = JsonConvert.DeserializeObject<OAuthErrorModel>(await response.Content.ReadAsStringAsync());

            //Assert
            Assert.AreEqual(OAuthError.InvalidRequest, error.Error);
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            _mockRepo.VerifyAll();
        }

        [Test]
        public async Task Return400IfUsernameIsMissingAndGrantTypeIsPassword_OnPost()
        {
            //Arrange
            var path = "/api/v1/token";

            var values = new List<KeyValuePair<string, string>>
                         {
                             new KeyValuePair<string, string>("grant_type", "password"),
                             new KeyValuePair<string, string>("password", "pass"),
                             new KeyValuePair<string, string>("scope", "full"),
                         };


            //Act
            var response = await _client.PostAsync(path, new FormUrlEncodedContent(values));
            var error = JsonConvert.DeserializeObject<OAuthErrorModel>(await response.Content.ReadAsStringAsync());

            //Assert
            Assert.AreEqual(OAuthError.InvalidRequest, error.Error);
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            _mockRepo.VerifyAll();
        }

        [Test]
        public async Task Return400IfScopeIsMissingAndGrantTypeIsPassword_OnPost()
        {
            //Arrange
            var path = "/api/v1/token";

            var values = new List<KeyValuePair<string, string>>
                         {
                             new KeyValuePair<string, string>("grant_type", "password"),
                             new KeyValuePair<string, string>("password", "pass"),
                             new KeyValuePair<string, string>("username", "user")
                         };


            //Act
            var response = await _client.PostAsync(path, new FormUrlEncodedContent(values));
            var error = JsonConvert.DeserializeObject<OAuthErrorModel>(await response.Content.ReadAsStringAsync());

            //Assert
            Assert.AreEqual(OAuthError.InvalidRequest, error.Error);
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            _mockRepo.VerifyAll();
        }

        [Test]
        public async Task Return400IfClientAuthorizationIsMissingAndGrantTypeIsPassword_OnPost()
        {
            //Arrange
            var path = "/api/v1/token";

            var values = new List<KeyValuePair<string, string>>
                         {
                             new KeyValuePair<string, string>("grant_type", "password"),
                             new KeyValuePair<string, string>("password", "pass"),
                             new KeyValuePair<string, string>("username", "user"),
                             new KeyValuePair<string, string>("scope", "full")
                         };


            //Act
            var response = await _client.PostAsync(path, new FormUrlEncodedContent(values));
            var error = JsonConvert.DeserializeObject<OAuthErrorModel>(await response.Content.ReadAsStringAsync());

            //Assert
            Assert.AreEqual(OAuthError.InvalidClient, error.Error);
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            _mockRepo.VerifyAll();
        }

        [Test]
        public async Task Return400IfClientNotExists_OnPost()
        {
            //Arrange
            var path = "/api/v1/token";

            var values = new List<KeyValuePair<string, string>>
                         {
                             new KeyValuePair<string, string>("grant_type", "password"),
                             new KeyValuePair<string, string>("password", "pass"),
                             new KeyValuePair<string, string>("username", "user"),
                             new KeyValuePair<string, string>("scope", "full")
                         };

            var oauthClient = new OAuthClient
                              {
                                  ClientId = "adsdsadas",
                                  Id = 1,
                                  IsAllowedToRequestFullScope = true,
                                  IsAllowedToRequestPasswordGrantType = true,
                                  Secret = "secret"
                              };

            _oauthClientRepo.Setup(x => x.GetById(oauthClient.ClientId))
                            .Returns(() => null);

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("basic", oauthClient.ClientId);

            //Act
            var response = await _client.PostAsync(path, new FormUrlEncodedContent(values));
            var error = JsonConvert.DeserializeObject<OAuthErrorModel>(await response.Content.ReadAsStringAsync());

            //Assert
            Assert.AreEqual(OAuthError.InvalidClient, error.Error);
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            _mockRepo.VerifyAll();
        }

        #endregion

    }
}
