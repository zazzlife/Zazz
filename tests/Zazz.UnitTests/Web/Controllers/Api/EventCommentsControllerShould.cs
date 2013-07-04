//using System.Collections.Generic;
//using System.Net;
//using System.Net.Http;
//using System.Text;
//using System.Threading.Tasks;
//using Moq;
//using NUnit.Framework;
//using Newtonsoft.Json;
//using Zazz.Core.Interfaces;
//using Zazz.Core.Models.Data;
//using Zazz.Core.Models.Data.Enums;
//using Zazz.Web.Interfaces;
//using Zazz.Web.Models;
//using Zazz.Web.Models.Api;

//namespace Zazz.UnitTests.Web.Controllers.Api
//{
//    [TestFixture]
//    public class EventCommentsControllerShould : BaseHMACTests
//    {
//        private Mock<ICommentService> _commentService;
//        private ApiComment _comment;
//        private int _commentId;
//        private Mock<IFeedHelper> _feedHelper;
//        private int _eventId;

//        public override void Init()
//        {
//            base.Init();

//            _commentId = 99;
//            _eventId = 444;
//            ControllerAddress = "/api/v1/comments/events/" + _eventId;

//            _comment = new ApiComment
//                       {
//                           CommentId = _commentId,
//                           CommentText = "message"
//                       };

//            _commentService = MockRepo.Create<ICommentService>();
//            _feedHelper = MockRepo.Create<IFeedHelper>();
//            IocContainer.Configure(x =>
//                                   {
//                                       x.For<ICommentService>().Use(_commentService.Object);
//                                       x.For<IFeedHelper>().Use(_feedHelper.Object);
//                                   });
//        }

//        [Test]
//        public async Task Return400IfIdIs0_OnGet()
//        {
//            //Arrange
//            ControllerAddress = "/api/v1/comments/events/" + 0;

//            AddValidHMACHeaders("GET");
//            SetupMocksForHMACAuth();

//            //Act
//            var response = await Client.GetAsync(ControllerAddress);

//            //Assert
//            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
//            MockRepo.VerifyAll();
//        }

//        [Test]
//        public async Task GetCommentsUsingFeedHelper_OnGet()
//        {
//            //Arrange
//            AddValidHMACHeaders("GET");
//            SetupMocksForHMACAuth();

//            _feedHelper.Setup(x => x.GetComments(_eventId, CommentType.Event, User.Id, 0, 10))
//                       .Returns(new List<CommentViewModel> { new CommentViewModel() });

//            //Act
//            var response = await Client.GetAsync(ControllerAddress);

//            //Assert
//            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
//            MockRepo.VerifyAll();
//        }

//        [Test]
//        public async Task Return400IfIdIs0_OnPost()
//        {
//            //Arrange
//            ControllerAddress = "/api/v1/comments/events/" + 0;
//            var message = "new message";

//            var json = JsonConvert.SerializeObject(message);
//            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

//            AddValidHMACHeaders("POST", ControllerAddress, json);
//            SetupMocksForHMACAuth();

//            //Act
//            var response = await Client.PostAsync(ControllerAddress, httpContent);

//            //Assert
//            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
//            MockRepo.VerifyAll();
//        }

//        [TestCase(null)]
//        [TestCase("")]
//        [TestCase(" ")]
//        public async Task Return400IfMessageIsMissing_OnPost(string message)
//        {
//            //Arrange
//            var json = JsonConvert.SerializeObject(message);
//            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

//            AddValidHMACHeaders("POST", ControllerAddress, json);
//            SetupMocksForHMACAuth();

//            //Act
//            var response = await Client.PostAsync(ControllerAddress, httpContent);

//            //Assert
//            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
//            MockRepo.VerifyAll();
//        }

//        [Test]
//        public async Task SaveNewComment_OnPost()
//        {
//            //Arrange
//            var message = "new message";
//            var json = JsonConvert.SerializeObject(message);
//            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

//            AddValidHMACHeaders("POST", ControllerAddress, json);
//            SetupMocksForHMACAuth();

//            _commentService.Setup(x => x.CreateComment(It.Is<Comment>(c => c.Message == message &&
//                                                                           c.EventComment.EventId == _eventId &&
//                                                                           c.PostComment == null &&
//                                                                           c.PhotoComment == null)
//                                                       , CommentType.Event))
//                           .Returns(_commentId);

//            //Act
//            var response = await Client.PostAsync(ControllerAddress, httpContent);

//            //Assert
//            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
//            MockRepo.VerifyAll();
//        }
//    }
//}