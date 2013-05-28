using System;
using System.Net.Http;
using System.Threading.Tasks;
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
    public class PostControllerShould : BaseApiControllerTests
    {
        private Mock<IPostService> _postService;

        public override void Init()
        {
            base.Init();

            _postService = MockRepo.Create<IPostService>();
            IocContainer.Configure(x =>
                                   {
                                       x.For<IPostService>().Use(_postService.Object);
                                   });
        }
    }
}