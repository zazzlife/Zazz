using System;
using System.Collections.Generic;
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
using Zazz.Core.Exceptions;
using Zazz.Core.Interfaces;
using Zazz.Core.Models;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;
using Zazz.Data;
using Zazz.Infrastructure.Services;
using Zazz.Web;
using Zazz.Web.Controllers.Api;
using Zazz.Web.DependencyResolution;
using Zazz.Web.Models.Api;
using Zazz.Web.OAuthAuthorizationServer;

namespace Zazz.UnitTests.Web.Controllers.Api
{
    [TestFixture]
    public class RegisterControllerShould
    {
        private HttpSelfHostServer _server;
        private HttpClient _client;
        private string _registerUrl;
        private MockRepository _mockRepo;
        private Mock<IAuthService> _authService;
        private ApiRegister _validUser;
        private ApiRegister _validClub;
        private Mock<IOAuthClientRepository> _oauthClientRepo;
        private string _clientId;
        private Mock<IOAuthService> _oauthService;
        private Mock<IUserService> _userService;
        private Mock<IPhotoService> _photoService;
        private const string BASE_ADDRESS = "http://localhost:8080";

        [SetUp]
        public void Init()
        {

            _mockRepo = new MockRepository(MockBehavior.Strict);
            _authService = _mockRepo.Create<IAuthService>();
            _oauthClientRepo = _mockRepo.Create<IOAuthClientRepository>();
            _oauthService = _mockRepo.Create<IOAuthService>();
            _userService = _mockRepo.Create<IUserService>();
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

            _registerUrl = "/api/v1/register";

            _validUser = new ApiRegister
                         {
                             AccountType = AccountType.User,
                             Email = "abc@abc.com",
                             Password = "1234",
                             Username = "Soroush",
                             Gender = Gender.Male
                         };

            _validClub = new ApiRegister
                         {
                             AccountType = AccountType.Club,
                             ClubName = "Club name",
                             ClubType = ClubType.Bar,
                             Email = "abc@abc.com",
                             Username = "Soroush",
                             Password = "1234"
                         };

            _clientId = "clientId";
        }

        private IContainer BuildIoC()
        {
            return new Container(x =>
            {
                x.For<IFilterProvider>().Use<StructureMapFilterProvider>();
                x.For<IAuthService>().Use(_authService.Object);
                x.For<IOAuthClientRepository>().Use(_oauthClientRepo.Object);
                x.For<IStaticDataRepository>().Use<StaticDataRepository>();
                x.For<IOAuthService>().Use(_oauthService.Object);
                x.For<IUserService>().Use(_userService.Object);
                x.For<IPhotoService>().Use(_photoService.Object);
            });
        }

        [Test]
        public async Task ReturnInvalidRequestIfRequestParametersAreMissing_OnPost()
        {
            //Arrange
            //Act
            var response = await _client.PostAsync(_registerUrl, null);
            var error = JsonConvert.DeserializeObject<OAuthErrorModel>(await response.Content.ReadAsStringAsync());

            //Assert
            Assert.AreEqual(OAuthError.InvalidRequest, error.Error);
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            _mockRepo.VerifyAll();
        }

        [Test]
        public async Task ReturnInvalidClientIfClientAuthorizationIsMissing_OnPost()
        {
            //Arrange
            var validUser = JsonConvert.SerializeObject(_validUser);
            var content = new StringContent(validUser, Encoding.UTF8, "application/json");

            //Act
            var response = await _client.PostAsync(_registerUrl, content);
            var error = JsonConvert.DeserializeObject<OAuthErrorModel>(await response.Content.ReadAsStringAsync());

            //Assert
            Assert.AreEqual(OAuthError.InvalidClient, error.Error);
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            _mockRepo.VerifyAll();
        }

        [Test]
        public async Task ReturnInvalidClientIfClientIsInvalid_OnPost()
        {
            //Arrange
            var validUser = JsonConvert.SerializeObject(_validUser);
            var content = new StringContent(validUser, Encoding.UTF8, "application/json");

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("basic", _clientId);

            _oauthClientRepo.Setup(x => x.GetById(_clientId))
                            .Returns(() => null);

            //Act
            var response = await _client.PostAsync(_registerUrl, content);
            var error = JsonConvert.DeserializeObject<OAuthErrorModel>(await response.Content.ReadAsStringAsync());

            //Assert
            Assert.AreEqual(OAuthError.InvalidClient, error.Error);
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            _mockRepo.VerifyAll();
        }

        [Test]
        public async Task ReturnInvalidRequestIfUsernameIsMissing_OnPost()
        {
            //Arrange
            _validUser.Username = "";
            var validUser = JsonConvert.SerializeObject(_validUser);
            var content = new StringContent(validUser, Encoding.UTF8, "application/json");

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("basic", _clientId);

            //Act
            var response = await _client.PostAsync(_registerUrl, content);
            var error = JsonConvert.DeserializeObject<OAuthErrorModel>(await response.Content.ReadAsStringAsync());

            //Assert
            Assert.AreEqual(OAuthError.InvalidRequest, error.Error);
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            _mockRepo.VerifyAll();
        }

        [Test]
        public async Task ReturnInvalidRequestIfPasswordIsMissing_OnPost()
        {
            //Arrange
            _validUser.Password = "";
            var validUser = JsonConvert.SerializeObject(_validUser);
            var content = new StringContent(validUser, Encoding.UTF8, "application/json");

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("basic", _clientId);

            //Act
            var response = await _client.PostAsync(_registerUrl, content);
            var error = JsonConvert.DeserializeObject<OAuthErrorModel>(await response.Content.ReadAsStringAsync());

            //Assert
            Assert.AreEqual(OAuthError.InvalidRequest, error.Error);
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            _mockRepo.VerifyAll();
        }

        [Test]
        public async Task ReturnInvalidRequestIfEmailIsMissing_OnPost()
        {
            //Arrange
            _validUser.Email = "";
            var validUser = JsonConvert.SerializeObject(_validUser);
            var content = new StringContent(validUser, Encoding.UTF8, "application/json");

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("basic", _clientId);

            //Act
            var response = await _client.PostAsync(_registerUrl, content);
            var error = JsonConvert.DeserializeObject<OAuthErrorModel>(await response.Content.ReadAsStringAsync());

            //Assert
            Assert.AreEqual(OAuthError.InvalidRequest, error.Error);
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            _mockRepo.VerifyAll();
        }

        [Test]
        public async Task ReturnInvalidRequestIfClubNameMissingAndAccountTypeIsClub_OnPost()
        {
            //Arrange
            _validClub.ClubName = "";

            var validClub = JsonConvert.SerializeObject(_validClub);
            var content = new StringContent(validClub, Encoding.UTF8, "application/json");

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("basic", _clientId);

            //Act
            var response = await _client.PostAsync(_registerUrl, content);
            var error = JsonConvert.DeserializeObject<OAuthErrorModel>(await response.Content.ReadAsStringAsync());

            //Assert
            Assert.AreEqual(OAuthError.InvalidRequest, error.Error);
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            _mockRepo.VerifyAll();
        }

        [Test]
        public async Task ReturnInvalidRequestOnInvalidEmailException_OnPost()
        {
            //Arrange
            var validUser = JsonConvert.SerializeObject(_validUser);
            var content = new StringContent(validUser, Encoding.UTF8, "application/json");

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("basic", _clientId);

            _oauthClientRepo.Setup(x => x.GetById(_clientId))
                            .Returns(new OAuthClient());

            _authService.Setup(x => x.Register(
                It.Is<User>(
                    u => u.Username.Equals(_validUser.Username) &&
                         u.IsConfirmed == false &&
                         u.AccountType == AccountType.User &&
                         u.Email.Equals(_validUser.Email) &&
                         u.Username.Equals(_validUser.Username)), _validUser.Password, true))
                        .Throws<InvalidEmailException>();
            
            //Act
            var response = await _client.PostAsync(_registerUrl, content);
            var error = JsonConvert.DeserializeObject<OAuthErrorModel>(await response.Content.ReadAsStringAsync());

            //Assert
            Assert.AreEqual(OAuthError.InvalidRequest, error.Error);
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            _mockRepo.VerifyAll();
        }

        [Test]
        public async Task ReturnInvalidRequestOnPasswordTooLongException_OnPost()
        {
            //Arrange
            var validUser = JsonConvert.SerializeObject(_validUser);
            var content = new StringContent(validUser, Encoding.UTF8, "application/json");

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("basic", _clientId);

            _oauthClientRepo.Setup(x => x.GetById(_clientId))
                            .Returns(new OAuthClient());

            _authService.Setup(x => x.Register(
                It.Is<User>(
                    u => u.Username.Equals(_validUser.Username) &&
                         u.IsConfirmed == false &&
                         u.AccountType == AccountType.User &&
                         u.Email.Equals(_validUser.Email) &&
                         u.Username.Equals(_validUser.Username)), _validUser.Password, true))
                        .Throws<PasswordTooLongException>();

            //Act
            var response = await _client.PostAsync(_registerUrl, content);
            var error = JsonConvert.DeserializeObject<OAuthErrorModel>(await response.Content.ReadAsStringAsync());

            //Assert
            Assert.AreEqual(OAuthError.InvalidRequest, error.Error);
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            _mockRepo.VerifyAll();
        }

        [Test]
        public async Task ReturnInvalidRequestOnUserNameExistsException_OnPost()
        {
            //Arrange
            var validUser = JsonConvert.SerializeObject(_validUser);
            var content = new StringContent(validUser, Encoding.UTF8, "application/json");

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("basic", _clientId);

            _oauthClientRepo.Setup(x => x.GetById(_clientId))
                            .Returns(new OAuthClient());

            _authService.Setup(x => x.Register(
                It.Is<User>(
                    u => u.Username.Equals(_validUser.Username) &&
                         u.IsConfirmed == false &&
                         u.AccountType == AccountType.User &&
                         u.Email.Equals(_validUser.Email) &&
                         u.Username.Equals(_validUser.Username)), _validUser.Password, true))
                        .Throws<UsernameExistsException>();

            //Act
            var response = await _client.PostAsync(_registerUrl, content);
            var error = JsonConvert.DeserializeObject<OAuthErrorModel>(await response.Content.ReadAsStringAsync());

            //Assert
            Assert.AreEqual(OAuthError.InvalidRequest, error.Error);
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            _mockRepo.VerifyAll();
        }

        [Test]
        public async Task ReturnInvalidRequestOnEmailExistsException_OnPost()
        {
            //Arrange
            var validUser = JsonConvert.SerializeObject(_validUser);
            var content = new StringContent(validUser, Encoding.UTF8, "application/json");

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("basic", _clientId);

            _oauthClientRepo.Setup(x => x.GetById(_clientId))
                            .Returns(new OAuthClient());

            _authService.Setup(x => x.Register(
                It.Is<User>(
                    u => u.Username.Equals(_validUser.Username) &&
                         u.IsConfirmed == false &&
                         u.AccountType == AccountType.User &&
                         u.Email.Equals(_validUser.Email) &&
                         u.Username.Equals(_validUser.Username)), _validUser.Password, true))
                        .Throws<EmailExistsException>();

            //Act
            var response = await _client.PostAsync(_registerUrl, content);
            var error = JsonConvert.DeserializeObject<OAuthErrorModel>(await response.Content.ReadAsStringAsync());

            //Assert
            Assert.AreEqual(OAuthError.InvalidRequest, error.Error);
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            _mockRepo.VerifyAll();
        }

        [Test]
        public async Task RegisterAndReturnUser_OnPost()
        {
            //Arrange
            var validUser = JsonConvert.SerializeObject(_validUser);
            var content = new StringContent(validUser, Encoding.UTF8, "application/json");

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("basic", _clientId);

            var client = new OAuthClient();

            _oauthClientRepo.Setup(x => x.GetById(_clientId))
                            .Returns(client);

            var user = new User
                       {
                           AccountType = AccountType.User,
                           Id = 2,
                       };

            _authService.Setup(x => x.Register(
                It.Is<User>(
                    u => u.Username.Equals(_validUser.Username) &&
                         u.IsConfirmed == false &&
                         u.AccountType == AccountType.User &&
                         u.Email.Equals(_validUser.Email) &&
                         u.Username.Equals(_validUser.Username)), _validUser.Password, true))
                        .Returns(user);

            var oauthCreds = new OAuthCredentials
                             {
                                 AccessToken = new JWT(),
                                 RefreshToken = new JWT()
                             };

            _oauthService.Setup(x => x.CreateOAuthCredentials(user, client, It.IsAny<List<OAuthScope>>()))
                         .Returns(oauthCreds);

            _userService.Setup(x => x.GetUserDisplayName(user.Id))
                        .Returns("asdf");

            _photoService.Setup(x => x.GetUserImageUrl(user.Id))
                         .Returns(new PhotoLinks("asdfg"));

            //Act
            var response = await _client.PostAsync(_registerUrl, content);
            var oauthRes = JsonConvert
                .DeserializeObject<OAuthAccessTokenResponse>(await response.Content.ReadAsStringAsync());

            //Assert
            Assert.IsNotNull(oauthRes.User);
            Assert.AreEqual(user.Id, oauthRes.User.UserId);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            _mockRepo.VerifyAll();
        }
    }
}