using System;
using System.Net.Http;
using System.Web.Http.Filters;
using System.Web.Http.SelfHost;
using Moq;
using NUnit.Framework;
using StructureMap;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Infrastructure.Services;
using Zazz.Web;
using Zazz.Web.DependencyResolution;

namespace Zazz.UnitTests.Web.Controllers.Api
{
    [TestFixture]
    public class PostControllerShould
    {
        private HttpSelfHostServer _handler;
        private HttpClient _client;
        private MockRepository _mockRepo;
        private Mock<IUserService> _userService;
        private Mock<IApiAppRepository> _appRepo;
        private Mock<IPhotoService> _photoService;
        private ApiApp _app;
        private CryptoService _cryptoService;
        private string _password;
        private User _user;
        private Mock<IPostService> _postService;

        [SetUp]
        public void Init()
        {
            //Global api config

            _app = Mother.GetApiApp();
            _mockRepo = new MockRepository(MockBehavior.Strict);
            _userService = _mockRepo.Create<IUserService>();
            _appRepo = _mockRepo.Create<IApiAppRepository>();
            _photoService = _mockRepo.Create<IPhotoService>();
            _cryptoService = new CryptoService();

            _password = "password";
            string iv;
            var passwordCipher = _cryptoService.EncryptPassword(_password, out iv);

            _user = new User
                    {
                        Id = 12,
                        Password = passwordCipher,
                        PasswordIV = Convert.FromBase64String(iv)
                    };


            const string BASE_ADDRESS = "http://localhost:8080";
            var config = new HttpSelfHostConfiguration(BASE_ADDRESS);
            JsonConfig.Configure(config);
            WebApiConfig.Register(config);
            var iocContainer = BuildIoC();
            config.DependencyResolver = new StructureMapDependencyResolver(iocContainer);

            _handler = new HttpSelfHostServer(config);
            _client = new HttpClient(_handler);

            //end of global config

            _postService = _mockRepo.Create<IPostService>();
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
                                     x.For<IPostService>().Use(_postService.Object);
                                 });
        }
    }
}