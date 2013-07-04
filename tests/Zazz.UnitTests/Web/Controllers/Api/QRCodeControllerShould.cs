//using System;
//using System.IO;
//using System.Net;
//using System.Text;
//using System.Threading.Tasks;
//using Moq;
//using NUnit.Framework;
//using Newtonsoft.Json;
//using Zazz.Core.Interfaces;
//using Zazz.Core.Models;
//using Zazz.Core.Models.Data;

//namespace Zazz.UnitTests.Web.Controllers.Api
//{
//    [TestFixture]
//    public class QRCodeControllerShould : BaseHMACTests
//    {
//        private Mock<IQRCodeService> _qrCodeService;

//        public override void Init()
//        {
//            base.Init();

//            ControllerAddress = "/api/v1/qrcode";

//            _qrCodeService = MockRepo.Create<IQRCodeService>();

//            IocContainer.Configure(x =>
//                                   {
//                                       x.For<IQRCodeService>().Use(_qrCodeService.Object);
//                                   });
//        }

//        [Test]
//        public async Task ReturnQRCode_OnGet()
//        {
//            //Arrange
//            var password = Encoding.UTF8.GetBytes("password");
//            var displayName = "name";
//            var displayPhoto = new PhotoLinks("photo url");

//            UserService.Setup(x => x.GetUserPassword(User.Id))
//                       .Returns(password);

//            UserService.Setup(x => x.GetUserDisplayName(User.Id))
//                       .Returns(displayName);

//            PhotoService.Setup(x => x.GetUserImageUrl(User.Id))
//                        .Returns(displayPhoto);

//            var model = new QRCodeModel
//                        {
//                            Id = User.Id,
//                            Name = displayName,
//                            Photo = displayPhoto.MediumLink,
//                            Type = QRCodeType.User,
//                            Token = CryptoService.GenerateQRCodeToken(password)
//                        };

//            var json = JsonConvert.SerializeObject(model);
//            var ms = new MemoryStream();

//            _qrCodeService.Setup(x => x.GenerateBlackAndWhite(json, 200, 96))
//                          .Returns(ms);


//            AddValidHMACHeaders("GET");
//            SetupMocksForHMACAuth();

//            //Act
//            var response = await Client.GetAsync(ControllerAddress);

//            //Assert
//            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
//        }


//    }
//}