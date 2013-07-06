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
using Newtonsoft.Json.Linq;
using StructureMap;
using Zazz.Core.Interfaces;
using Zazz.Core.Models;
using Zazz.Core.Models.Data;
using Zazz.Data;
using Zazz.Web;
using Zazz.Web.Controllers.Api;
using Zazz.Web.DependencyResolution;
using System.Net;
using Zazz.Web.Models.Api;
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
        private Mock<ICryptoService> _cryptoService;

        [SetUp]
        public void Init()
        {
            _mockRepo = new MockRepository(MockBehavior.Strict);
            _oauthService = _mockRepo.Create<IOAuthService>();
            _userService = _mockRepo.Create<IUserService>();
            _photoService = _mockRepo.Create<IPhotoService>();
            _oauthClientRepo = _mockRepo.Create<IOAuthClientRepository>();
            _cryptoService = _mockRepo.Create<ICryptoService>();

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
                                                 x.For<ICryptoService>().Use(_cryptoService.Object);
                                                 x.For<IStaticDataRepository>().Use<StaticDataRepository>();
                                             });

            _configuration.DependencyResolver = new StructureMapDependencyResolver(iocContainer);

            var server = new HttpSelfHostServer(_configuration);
            _client = new HttpClient(server)
                      {
                          BaseAddress = new Uri(BASE_ADDRESS)
                      };
        }

        [Test]
        public async Task ReturnInvalidRequestIfRequestParametersAreMissing_OnPost()
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
        public async Task ReturnInvalidRequestIfPasswordIsMissingAndGrantTypeIsPassword_OnPost()
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
        public async Task ReturnInvalidRequestIfUsernameIsMissingAndGrantTypeIsPassword_OnPost()
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
        public async Task ReturnInvalidRequestIfScopeIsMissingAndGrantTypeIsPassword_OnPost()
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
        public async Task ReturnInvalidClientIfClientAuthorizationIsMissingAndGrantTypeIsPassword_OnPost()
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
        public async Task ReturnInvalidClientIfClientNotExists_OnPost()
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

        [Test]
        public async Task ReturnUnauthorizedClientIfClientIsNotAllowedToRequestForPasswordGrant_OnPost()
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
                                  IsAllowedToRequestPasswordGrantType = false,
                                  Secret = "secret"
                              };

            _oauthClientRepo.Setup(x => x.GetById(oauthClient.ClientId))
                            .Returns(oauthClient);

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("basic", oauthClient.ClientId);

            //Act
            var response = await _client.PostAsync(path, new FormUrlEncodedContent(values));
            var error = JsonConvert.DeserializeObject<OAuthErrorModel>(await response.Content.ReadAsStringAsync());

            //Assert
            Assert.AreEqual(OAuthError.UnauthorizedClient, error.Error);
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            _mockRepo.VerifyAll();
        }

        [Test]
        public async Task ReturnInvalidScopeIfClientIsNotAllowedToRequestForFullScope_OnPost()
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
                                  IsAllowedToRequestFullScope = false,
                                  IsAllowedToRequestPasswordGrantType = true,
                                  Secret = "secret"
                              };

            _oauthClientRepo.Setup(x => x.GetById(oauthClient.ClientId))
                            .Returns(oauthClient);

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("basic", oauthClient.ClientId);

            //Act
            var response = await _client.PostAsync(path, new FormUrlEncodedContent(values));
            var error = JsonConvert.DeserializeObject<OAuthErrorModel>(await response.Content.ReadAsStringAsync());

            //Assert
            Assert.AreEqual(OAuthError.InvalidScope, error.Error);
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            _mockRepo.VerifyAll();
        }

        [Test]
        public async Task ReturnInvalidGrantIfUserNotExists_OnPost()
        {
            //Arrange
            var path = "/api/v1/token";
            var username = "usern";

            var values = new List<KeyValuePair<string, string>>
                         {
                             new KeyValuePair<string, string>("grant_type", "password"),
                             new KeyValuePair<string, string>("password", "pass"),
                             new KeyValuePair<string, string>("username", username),
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
                            .Returns(oauthClient);

            _userService.Setup(x => x.GetUser(username, false, false, false, false))
                .Returns(() => null);


            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("basic", oauthClient.ClientId);

            //Act
            var response = await _client.PostAsync(path, new FormUrlEncodedContent(values));
            var error = JsonConvert.DeserializeObject<OAuthErrorModel>(await response.Content.ReadAsStringAsync());

            //Assert
            Assert.AreEqual(OAuthError.InvalidGrant, error.Error);
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            _mockRepo.VerifyAll();
        }

        [Test]
        public async Task ReturnInvalidGrantIfPasswordIsInvalid_OnPost()
        {
            //Arrange
            var path = "/api/v1/token";
            var username = "usern";
            var password = "pass";

            var values = new List<KeyValuePair<string, string>>
                         {
                             new KeyValuePair<string, string>("grant_type", "password"),
                             new KeyValuePair<string, string>("password", "some random password"),
                             new KeyValuePair<string, string>("username", username),
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

            var user = new User
                       {
                           Password = new byte[] { 1, 2, 3 },
                           PasswordIV = new byte[] { 4, 5 }
                       };

            _oauthClientRepo.Setup(x => x.GetById(oauthClient.ClientId))
                            .Returns(oauthClient);

            _userService.Setup(x => x.GetUser(username, false, false, false, false))
                .Returns(user);

            _cryptoService.Setup(x => x.DecryptPassword(user.Password, user.PasswordIV))
                          .Returns(password);

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("basic", oauthClient.ClientId);

            //Act
            var response = await _client.PostAsync(path, new FormUrlEncodedContent(values));
            var error = JsonConvert.DeserializeObject<OAuthErrorModel>(await response.Content.ReadAsStringAsync());

            //Assert
            Assert.AreEqual(OAuthError.InvalidGrant, error.Error);
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            _mockRepo.VerifyAll();
        }

        [Test]
        public async Task ReturnTokensIfEverythingIsValid_OnPost()
        {
            //Arrange
            var path = "/api/v1/token";
            var username = "usern";
            var password = "pass";
            var displayName = "displayname";

            var values = new List<KeyValuePair<string, string>>
                         {
                             new KeyValuePair<string, string>("grant_type", "password"),
                             new KeyValuePair<string, string>("password", password),
                             new KeyValuePair<string, string>("username", username),
                             new KeyValuePair<string, string>("scope", "full")
                         };

            var oauthClient = new OAuthClient
            {
                ClientId = "adsdsadas",
                Id = 5454,
                IsAllowedToRequestFullScope = true,
                IsAllowedToRequestPasswordGrantType = true,
                Secret = "secret"
            };

            var user = new User
            {
                Id = 3232,
                Password = new byte[] { 1, 2, 3 },
                PasswordIV = new byte[] { 4, 5 }
            };

            var oauthCred = new OAuthCredentials
                        {
                            AccessToken = new JWT
                                          {
                                              IssuedDate = DateTime.UtcNow,
                                              ExpirationDate = DateTime.UtcNow.AddHours(1),
                                              Scopes = new List<string> { "full" },
                                              ClientId = oauthClient.Id,
                                              UserId = user.Id,
                                              TokenType = JWT.ACCESS_TOKEN_TYPE
                                          },
                            RefreshToken = new JWT
                                           {
                                               IssuedDate = DateTime.UtcNow,
                                               TokenType = JWT.REFRESH_TOKEN_TYPE,
                                               TokenId = 23,
                                               VerificationCode = "verification",
                                               ClientId = oauthClient.Id,
                                               UserId = user.Id,
                                               Scopes = new List<string> { "full" }
                                           }
                        };

            _oauthClientRepo.Setup(x => x.GetById(oauthClient.ClientId))
                            .Returns(oauthClient);

            _userService.Setup(x => x.GetUser(username, false, false, false, false))
                .Returns(user);

            _cryptoService.Setup(x => x.DecryptPassword(user.Password, user.PasswordIV))
                          .Returns(password);

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("basic", oauthClient.ClientId);

            _oauthService.Setup(x => x.CreateOAuthCredentials(user, oauthClient, It.IsAny<List<OAuthScope>>()))
                         .Returns(oauthCred);

            _userService.Setup(x => x.GetUserDisplayName(user.Id))
                        .Returns(displayName);
            _photoService.Setup(x => x.GetUserImageUrl(user.Id))
                         .Returns(new PhotoLinks("photo url"));

            //Act
            var response = await _client.PostAsync(path, new FormUrlEncodedContent(values));
            var content = await response.Content.ReadAsStringAsync();
            dynamic json = JObject.Parse(content);

            //Assert
            Assert.AreEqual(oauthCred.AccessToken.ToJWTString(), (string)json.access_token);
            Assert.AreEqual(oauthCred.RefreshToken.ToJWTString(), (string)json.refresh_token);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            _mockRepo.VerifyAll();
        }

        #endregion

    }
}
