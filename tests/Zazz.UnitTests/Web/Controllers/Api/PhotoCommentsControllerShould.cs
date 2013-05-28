using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Zazz.Core.Interfaces;
using Zazz.Web.Interfaces;
using Zazz.Web.Models.Api;

namespace Zazz.UnitTests.Web.Controllers.Api
{
    [TestFixture]
    public class PhotoCommentsControllerShould : BaseHMACTests
    {
        private Mock<ICommentService> _commentService;
        private ApiComment _comment;
        private int _commentId;
        private Mock<IFeedHelper> _feedHelper;
        private int _photoId;

        public override void Init()
        {
            base.Init();

            _commentId = 99;
            _photoId = 444;
            ControllerAddress = "/api/v1/photocomments/" + _photoId;

            _comment = new ApiComment
                       {
                           CommentId =  _commentId,
                           CommentText = "message"
                       };

            _commentService = MockRepo.Create<ICommentService>();
            _feedHelper = MockRepo.Create<IFeedHelper>();
            IocContainer.Configure(x =>
                                   {
                                       x.For<ICommentService>().Use(_commentService.Object);
                                       x.For<IFeedHelper>().Use(_feedHelper.Object);
                                   });
        }

        [Test]
        public async Task Return400IfIdIs0_OnGet()
        {
            //Arrange
            ControllerAddress = "/api/v1/photocomments/" + 0;

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