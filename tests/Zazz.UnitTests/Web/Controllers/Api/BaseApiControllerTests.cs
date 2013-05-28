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
    public abstract class BaseApiControllerTests
    {
        protected HttpSelfHostServer Server;
        protected HttpClient Client;
        protected MockRepository MockRepo;
        protected Mock<IUserService> UserService;
        protected Mock<IApiAppRepository> AppRepo;
        protected Mock<IPhotoService> PhotoService;
        protected ApiApp App;
        protected CryptoService CryptoService;
        protected string Password;
        protected User User;
        protected IContainer IocContainer;

        [SetUp]
        public virtual void Init()
        {
            //Global api config

            App = Mother.GetApiApp();
            MockRepo = new MockRepository(MockBehavior.Strict);
            UserService = MockRepo.Create<IUserService>();
            AppRepo = MockRepo.Create<IApiAppRepository>();
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
        }

        private IContainer BuildIoC()
        {
            return new Container(x =>
            {
                x.For<IFilterProvider>().Use<StructureMapFilterProvider>();
                x.For<ICryptoService>().Use<CryptoService>();
                x.For<IUserService>().Use(UserService.Object);
                x.For<IApiAppRepository>().Use(AppRepo.Object);
                x.For<IPhotoService>().Use(PhotoService.Object);
            });
        }

        [TearDown]
        public void Cleanup()
        {
            Server.Dispose();
            Client.Dispose();
        }

    }
}