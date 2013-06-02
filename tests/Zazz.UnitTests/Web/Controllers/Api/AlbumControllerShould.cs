using System;
using System.Net;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.UnitTests.Web.Controllers.Api
{
    [TestFixture]
    public class AlbumControllerShould : BaseHMACTests
    {
        private Mock<IAlbumService> _albumService;
        private int _albumId;

        public override void Init()
        {
            base.Init();

            _albumId = 444;
            ControllerAddress = "/api/v1/albums/" + _albumId;

            _albumService = MockRepo.Create<IAlbumService>();

            IocContainer.Configure(x =>
            {
                x.For<IAlbumService>().Use(_albumService.Object);
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


    }
}