using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
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
using Zazz.Web.Filters;
using Zazz.Web.OAuthAuthorizationServer;

namespace Zazz.UnitTests.Web.Filters
{
    [TestFixture]
    public class OAuth2AuthorizeAttributeShould
    {
        private HttpSelfHostConfiguration _config;
        private HttpSelfHostServer _server;
        private HttpClient _client;
        private int _usreId;
        private const string BASE_ADDRESS = "http://localhost:8080";

        [SetUp]
        public void Init()
        {
            var container = SetupIoC();
            _config = new HttpSelfHostConfiguration(BASE_ADDRESS);

            JsonConfig.Configure(_config);

            _config.Routes.MapHttpRoute(
                name: "Dummy Route",
                routeTemplate: "{controller}",
                defaults: new
                {
                    controller = "Test"
                });

            _config.DependencyResolver = new StructureMapDependencyResolver(container);

            _server = new HttpSelfHostServer(_config);
            _client = new HttpClient(_server) { BaseAddress = new Uri(BASE_ADDRESS) };

            _usreId = 1000;
        }

        private IContainer SetupIoC()
        {
            return new Container(x =>
                                    {
                                        x.Scan(s =>
                                               {
                                                   s.TheCallingAssembly();
                                                   s.WithDefaultConventions();
                                               });

                                        x.For<IFilterProvider>().Use<StructureMapFilterProvider>();
                                    });
        }

        [Test]
        public async Task ReturnInvalidGrantIfAccessTokenIsMissing()
        {
            //Arrange

            _client.DefaultRequestHeaders.Authorization = null;

            //Act
            var response = await _client.GetAsync("/");
            var content = await response.Content.ReadAsStringAsync();
            var error = JsonConvert.DeserializeObject<OAuthErrorModel>(content);

            //Assert
            Assert.AreEqual(OAuthError.InvalidGrant, error.Error);
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }
        
    }

    [OAuth2Authorize]
    public class TestController : ApiController
    {
        public int Get()
        {
            return 1;
        }
    }
}