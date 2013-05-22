using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.SelfHost;
using NUnit.Framework;
using Zazz.Web;

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
        private const string BASE_ADDRESS = "http://localhost:8080";

        [SetUp]
        public void Init()
        {
            var config = new HttpSelfHostConfiguration(BASE_ADDRESS);
            JsonConfig.Configure(config);
            WebApiConfig.Register(config);

            _server = new HttpSelfHostServer(config);
            _client = new HttpClient(_server)
                      {
                          BaseAddress = new Uri(BASE_ADDRESS)
                      };

            _username = "Soroush";
            _password = "123";
            _loginUrl = "/api/v1/login";
        }

        private void AddUserNameAndPassToUrl(string username, string password)
        {
            _loginUrl += String.Format("?username={0}&password={1}", username, password);
        }

        [Test]
        public async Task Return403IfAuthorizationHeaderIsMissing()
        {
            //Arrange
            _client.DefaultRequestHeaders.Date = DateTimeOffset.UtcNow;
            AddUserNameAndPassToUrl(_username, _password);

            //Act
            var response = await _client.GetAsync(_loginUrl);

            //Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [TearDown]
        public void Cleanup()
        {
            _server.Dispose();
            _client.Dispose();
        }
    }
}