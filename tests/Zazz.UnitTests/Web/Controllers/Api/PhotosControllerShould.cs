using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Web.Models.Api;

namespace Zazz.UnitTests.Web.Controllers.Api
{
    [TestFixture]
    public class PhotosControllerShould : BaseHMACTests
    {
        private int _photoId;
        private Mock<IPhotoService> _photoService;
        private ApiPhoto _apiPhoto;

        public override void Init()
        {
            base.Init();

            _photoId = 99;
            ControllerAddress = "/api/v1/photos/" + _photoId;

            _apiPhoto = new ApiPhoto
                        {
                            Description = "desc",
                        };

            _photoService = MockRepo.Create<IPhotoService>();

            IocContainer.Configure(x =>
            {
                x.For<IPhotoService>().Use(_photoService.Object);
            });
        }

        [TestCase(0)]
        [TestCase(-1)]
        public async Task Return400IfUserIdIsLessThan1_OnGet(int userId)
        {
            //Arrange
            ControllerAddress = String.Format("/api/v1/photos?userId={0}", userId);

            AddValidHMACHeaders("GET");
            SetupMocksForHMACAuth();

            //Act
            var result = await Client.GetAsync(ControllerAddress);

            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task GetUserPhotos_OnGet()
        {
            //Arrange
            var userId = 12;
            var defaultPageSize = 25;
            ControllerAddress = String.Format("/api/v1/photos?userId={0}", userId);

            _photoService.Setup(x => x.GetUserPhotos(userId, defaultPageSize, null))
                         .Returns(new EnumerableQuery<Photo>(Enumerable.Empty<Photo>()));

            AddValidHMACHeaders("GET");
            SetupMocksForHMACAuth();

            //Act
            var result = await Client.GetAsync(ControllerAddress);

            //Assert
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task GetUserPhotosAndPassLastPhotoId_OnGet()
        {
            //Arrange
            var userId = 12;
            var defaultPageSize = 25;
            var lastPhoto = 44;
            ControllerAddress = String.Format("/api/v1/photos?userId={0}&lastPhoto={1}", userId, lastPhoto);

            _photoService.Setup(x => x.GetUserPhotos(userId, defaultPageSize, lastPhoto))
                         .Returns(new EnumerableQuery<Photo>(Enumerable.Empty<Photo>()));

            AddValidHMACHeaders("GET");
            SetupMocksForHMACAuth();

            //Act
            var result = await Client.GetAsync(ControllerAddress);

            //Assert
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task Return400IfPhotoIdIs0_OnGet()
        {
            //Arrange
            ControllerAddress = "/api/v1/photos/0";

            AddValidHMACHeaders("GET");
            SetupMocksForHMACAuth();

            //Act
            var result = await Client.GetAsync(ControllerAddress);

            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
            MockRepo.VerifyAll();
        }
    }
}