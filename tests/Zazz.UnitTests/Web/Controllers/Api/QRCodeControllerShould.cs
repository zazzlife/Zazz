using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Newtonsoft.Json;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Services;
using Zazz.Core.Models;
using Zazz.Core.Models.Data;
using Zazz.Web.OAuthAuthorizationServer;

namespace Zazz.UnitTests.Web.Controllers.Api
{
    [TestFixture]
    public class QRCodeControllerShould : BaseOAuthTests
    {
        private Mock<IQRCodeService> _qrCodeService;

        public override void Init()
        {
            base.Init();

            ControllerAddress = "/api/v1/qrcode";

            _qrCodeService = MockRepo.Create<IQRCodeService>();

            IocContainer.Configure(x =>
                                   {
                                       x.For<IQRCodeService>().Use(_qrCodeService.Object);
                                   });
        }

        [Test]
        public async Task ReturnQRCode_OnGet()
        {
            //Arrange
            var password = Encoding.UTF8.GetBytes("password");
            var displayName = "name";
            var displayPhoto = new PhotoLinks("photo url");

            

            UserService.Setup(x => x.GetUserDisplayName(User.Id))
                       .Returns(displayName);

            PhotoService.Setup(x => x.GetUserImageUrl(User.Id))
                        .Returns(displayPhoto);

            var ms = new MemoryStream();

            _qrCodeService.Setup(x => x.GenerateBlackAndWhite(It.IsAny<string>(), 200, 96))
                          .Returns(ms);

            ms.Dispose();

            CreateValidAccessToken();

            //Act
            var response = await Client.GetAsync(ControllerAddress);

            //Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }


    }
}