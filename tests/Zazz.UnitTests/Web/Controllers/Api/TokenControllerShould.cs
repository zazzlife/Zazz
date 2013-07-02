using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.SelfHost;
using Moq;
using NUnit.Framework;
using StructureMap;
using Zazz.Web;
using Zazz.Web.Controllers.Api;
using Zazz.Web.DependencyResolution;
using System.Net;

namespace Zazz.UnitTests.Web.Controllers.Api
{
    [TestFixture]
    public class TokenControllerShould
    {
        private HttpSelfHostConfiguration _configuration;
        private HttpClient _client;
        private MockRepository _mockRepo;

        [SetUp]
        public void Init()
        {
            _mockRepo = new MockRepository(MockBehavior.Strict);

            const string BASE_ADDRESS = "http://localhost:8080";
            _configuration = new HttpSelfHostConfiguration(BASE_ADDRESS);

            JsonConfig.Configure(_configuration);
            WebApiConfig.Register(_configuration);

            var iocContainer = new Container(x =>
                                             {
                                                 
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

            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            _mockRepo.VerifyAll();
        }

        [Test]
        public async Task Return400IfRequestTypeIsNotPassword_OnPost()
        {
            //Arrange
            var path = "/api/v1/token";

            var values = new List<KeyValuePair<string, string>>
                         {
                             new KeyValuePair<string, string>("grant_type", "undefined"),
                             new KeyValuePair<string, string>("password", "pass"),
                             new KeyValuePair<string, string>("username", "user"),
                             new KeyValuePair<string, string>("scope", "full"),
                         };
            

            //Act
            var response = await _client.PostAsync(path, new FormUrlEncodedContent(values));

            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            _mockRepo.VerifyAll();
        }

        [Test]
        public async Task Return400IfPasswordIsMissing_OnPost()
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

            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            _mockRepo.VerifyAll();
        }

        [Test]
        public async Task Return400IfUsernameIsMissing_OnPost()
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

            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            _mockRepo.VerifyAll();
        }

        [Test]
        public async Task Return400IfScopeIsMissing_OnPost()
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

            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            _mockRepo.VerifyAll();
        }
    }
}
