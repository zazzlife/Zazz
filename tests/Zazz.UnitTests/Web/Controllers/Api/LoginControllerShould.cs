using System;
using System.Net.Http;
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
        private const string BASE_ADDRESS = "http://localhost:8080";

        [SetUp]
        public void Init()
        {
            var config = new HttpSelfHostConfiguration(BASE_ADDRESS);
            JsonConfig.Configure(config);
            WebApiConfig.Register(config);

            _server = new HttpSelfHostServer(config);
            _client = new HttpClient(_server);
            _client.BaseAddress = new Uri(BASE_ADDRESS);
        }

        [TearDown]
        public void Cleanup()
        {
            _server.Dispose();
            _client.Dispose();
        }
    }
}