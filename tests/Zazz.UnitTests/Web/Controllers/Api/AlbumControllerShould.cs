using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Web.Interfaces;

namespace Zazz.UnitTests.Web.Controllers.Api
{
    [TestFixture]
    public class AlbumControllerShould : BaseHMACTests
    {
        private Mock<IAlbumService> _albumService;
        private int _albumId;
        private Mock<IObjectMapper> _objectMapper;

        public override void Init()
        {
            base.Init();

            _albumId = 444;
            ControllerAddress = "/api/v1/albums/" + _albumId;

            _albumService = MockRepo.Create<IAlbumService>();
            _objectMapper = MockRepo.Create<IObjectMapper>();

            IocContainer.Configure(x =>
            {
                x.For<IAlbumService>().Use(_albumService.Object);
                x.For<IObjectMapper>().Use(_objectMapper.Object);
            });
        }

        [TestCase(0)]
        [TestCase(-1)]
        public async Task Return400IfUserIdIsInvalid_OnGetUserAlbums(int userId)
        {
            //Arrange
            ControllerAddress = String.Format("/api/v1/albums?userId={0}", userId);

            AddValidHMACHeaders("GET");
            SetupMocksForHMACAuth();

            //Act
            var response = await Client.GetAsync(ControllerAddress);

            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task ReturnAlbums_OnGetUserAlbums()
        {
            //Arrange
            ControllerAddress = String.Format("/api/v1/albums?userId={0}", User.Id);

            AddValidHMACHeaders("GET");
            SetupMocksForHMACAuth();

            _albumService.Setup(x => x.GetUserAlbums(User.Id, true))
                         .Returns(new EnumerableQuery<Album>(Enumerable.Empty<Album>()));

            //Act
            var response = await Client.GetAsync(ControllerAddress);

            //Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            MockRepo.VerifyAll();
        }


    }
}