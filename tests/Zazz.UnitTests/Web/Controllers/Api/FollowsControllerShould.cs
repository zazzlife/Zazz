//using System.Linq;
//using System.Net;
//using System.Net.Http;
//using System.Text;
//using System.Threading.Tasks;
//using Moq;
//using NUnit.Framework;
//using Newtonsoft.Json;
//using Zazz.Core.Exceptions;
//using Zazz.Core.Interfaces;
//using Zazz.Core.Models;
//using Zazz.Core.Models.Data;
//using Zazz.Web.Models.Api;

//namespace Zazz.UnitTests.Web.Controllers.Api
//{
//    [TestFixture]
//    public class FollowsControllerShould : BaseHMACTests
//    {
//        private Mock<IFollowService> _followService;
//        private int _userId;

//        public override void Init()
//        {
//            base.Init();

//            _userId = 83;
//            ControllerAddress = "/api/v1/follows";
//            _followService = MockRepo.Create<IFollowService>();

//            IocContainer.Configure(x =>
//                                   {
//                                       x.For<IFollowService>().Use(_followService.Object);
//                                   });
//        }

//        [Test]
//        public async Task GetFollowers_OnGet()
//        {
//            //Arrange
//            _followService.Setup(x => x.GetFollowers(User.Id))
//                          .Returns(new EnumerableQuery<Follow>(Enumerable.Empty<Follow>()));

//            AddValidHMACHeaders("GET");
//            SetupMocksForHMACAuth();

//            //Act
//            var result = await Client.GetAsync(ControllerAddress);

//            //Assert
//            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
//            MockRepo.VerifyAll();
//        }

//        [Test]
//        public async Task Return400IfUserIdIs0_OnPost()
//        {
//            //Arrange
//            var json = JsonConvert.SerializeObject(0);
//            var content = new StringContent(json, Encoding.UTF8, "application/json");

//            AddValidHMACHeaders("POST", ControllerAddress, json);
//            SetupMocksForHMACAuth();

//            //Act
//            var response = await Client.PostAsync(ControllerAddress, content);

//            //Assert
//            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
//            MockRepo.VerifyAll();
//        }

//        [Test]
//        public async Task CreateFollow_OnPost()
//        {
//            //Arrange
//            var json = JsonConvert.SerializeObject(_userId);
//            var content = new StringContent(json, Encoding.UTF8, "application/json");

//            _followService.Setup(x => x.Follow(User.Id, _userId));

//            AddValidHMACHeaders("POST", ControllerAddress, json);
//            SetupMocksForHMACAuth();

//            //Act
//            var response = await Client.PostAsync(ControllerAddress, content);

//            //Assert
//            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
//            MockRepo.VerifyAll();
//        }

//        [Test]
//        public async Task Return400IfUserIdIs0_OnQRCodePost()
//        {
//            //Arrange
//            ControllerAddress = "/api/v1/follows/qrcode";

//            var qrModel = new QRCodeModel
//                          {
//                              Id = 0,
//                              Token = "token"
//                          };

//            var json = JsonConvert.SerializeObject(qrModel);
//            var content = new StringContent(json, Encoding.UTF8, "application/json");

//            AddValidHMACHeaders("POST", ControllerAddress, json);
//            SetupMocksForHMACAuth();

//            //Act
//            var response = await Client.PostAsync(ControllerAddress, content);

//            //Assert
//            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
//            MockRepo.VerifyAll();
//        }

//        [TestCase(null)]
//        [TestCase("")]
//        [TestCase(" ")]
//        public async Task Return400IfTokenIsInvalid_OnQRCodePost(string token)
//        {
//            //Arrange
//            ControllerAddress = "/api/v1/follows/qrcode";

//            var qrModel = new QRCodeModel
//            {
//                Id = 14,
//                Token = token
//            };

//            var json = JsonConvert.SerializeObject(qrModel);
//            var content = new StringContent(json, Encoding.UTF8, "application/json");

//            AddValidHMACHeaders("POST", ControllerAddress, json);
//            SetupMocksForHMACAuth();

//            //Act
//            var response = await Client.PostAsync(ControllerAddress, content);

//            //Assert
//            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
//            MockRepo.VerifyAll();
//        }

//        [Test]
//        public async Task Return404IfUserDoesntExists_OnQRCodePost()
//        {
//            //Arrange
//            ControllerAddress = "/api/v1/follows/qrcode";

//            var qrModel = new QRCodeModel
//            {
//                Id = 500,
//                Token = "token"
//            };

//            var json = JsonConvert.SerializeObject(qrModel);
//            var content = new StringContent(json, Encoding.UTF8, "application/json");

//            UserService.Setup(x => x.GetUserPassword(qrModel.Id))
//                       .Throws<NotFoundException>();

//            AddValidHMACHeaders("POST", ControllerAddress, json);
//            SetupMocksForHMACAuth();

//            //Act
//            var response = await Client.PostAsync(ControllerAddress, content);

//            //Assert
//            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
//            MockRepo.VerifyAll();
//        }

//        [Test]
//        public async Task Return403IfTokenIsInvalid_OnQRCodePost()
//        {
//            //Arrange
//            ControllerAddress = "/api/v1/follows/qrcode";

//            var password = Encoding.UTF8.GetBytes("password");

//            var qrModel = new QRCodeModel
//            {
//                Id = 500,
//                Token = "invalid token"
//            };

//            var json = JsonConvert.SerializeObject(qrModel);
//            var content = new StringContent(json, Encoding.UTF8, "application/json");

//            UserService.Setup(x => x.GetUserPassword(qrModel.Id))
//                       .Returns(password);

//            AddValidHMACHeaders("POST", ControllerAddress, json);
//            SetupMocksForHMACAuth();

//            //Act
//            var response = await Client.PostAsync(ControllerAddress, content);

//            //Assert
//            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
//            MockRepo.VerifyAll();
//        }

//        [Test]
//        public async Task Return204AndCreateFollowOnSuccess_OnQRCodePost()
//        {
//            //Arrange
//            ControllerAddress = "/api/v1/follows/qrcode";

//            var password = Encoding.UTF8.GetBytes("password");

//            var qrModel = new QRCodeModel
//            {
//                Id = 500,
//                Token = CryptoService.GenerateQRCodeToken(password)
//            };

//            var json = JsonConvert.SerializeObject(qrModel);
//            var content = new StringContent(json, Encoding.UTF8, "application/json");

//            UserService.Setup(x => x.GetUserPassword(qrModel.Id))
//                       .Returns(password);

//            _followService.Setup(x => x.Follow(qrModel.Id, User.Id));

//            AddValidHMACHeaders("POST", ControllerAddress, json);
//            SetupMocksForHMACAuth();

//            //Act
//            var response = await Client.PostAsync(ControllerAddress, content);

//            //Assert
//            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
//            MockRepo.VerifyAll();
//        }

//        [Test]
//        public async Task Throw400IfIdIs0_OnDelete()
//        {
//            //Arrange
//            ControllerAddress = "/api/v1/follows/0";

//            AddValidHMACHeaders("DELETE");
//            SetupMocksForHMACAuth();
            
//            //Act
//            var response = await Client.DeleteAsync(ControllerAddress);

//            //Assert
//            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
//            MockRepo.VerifyAll();
//        }

//        [Test]
//        public async Task RemoveFollow_OnDelete()
//        {
//            //Arrange
//            ControllerAddress = "/api/v1/follows/" + _userId;

//            AddValidHMACHeaders("DELETE");
//            SetupMocksForHMACAuth();

//            _followService.Setup(x => x.RemoveFollow(User.Id, _userId));

//            //Act
//            var response = await Client.DeleteAsync(ControllerAddress);

//            //Assert
//            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
//            MockRepo.VerifyAll();
//        }
//    }
//}