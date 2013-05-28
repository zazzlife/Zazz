using System;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Newtonsoft.Json;
using Zazz.Core.Exceptions;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Web.Models.Api;

namespace Zazz.UnitTests.Web.Controllers.Api
{
    [TestFixture]
    public class CommentsControllerShould : BaseHMACTests
    {
        private int _commentId;
        private ApiComment _comment;
        private Mock<ICommentService> _commentService;

        public override void Init()
        {
            DefaultHttpMethod = HttpMethod.Delete;

            base.Init();

            _commentId = 99;
            ControllerAddress = "/api/v1/comments/" + _commentId;

            _comment = new ApiComment
                       {
                           CommentText = "message"
                       };

            _commentService = MockRepo.Create<ICommentService>();
            IocContainer.Configure(x =>
                                   {
                                       x.For<ICommentService>().Use(_commentService.Object);
                                   });
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public async Task Return400IfCommentTextIsMissing_OnPut(string message)
        {
            //Arrange
            _comment.CommentText = message;

            var json = JsonConvert.SerializeObject(_comment);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            AddValidHMACHeaders("PUT", ControllerAddress, json);
            SetupMocksForHMACAuth();

            //Act
            var response = await Client.PutAsync(ControllerAddress, httpContent);

            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task Return404IfCommentsDoesntExists_OnPut()
        {
            //Arrange
            _commentService.Setup(x => x.EditComment(_commentId, User.Id, _comment.CommentText))
                           .Throws<NotFoundException>();

            var json = JsonConvert.SerializeObject(_comment);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            AddValidHMACHeaders("PUT", ControllerAddress, json);
            SetupMocksForHMACAuth();

            //Act
            var response = await Client.PutAsync(ControllerAddress, httpContent);

            //Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task Return403OnSecurityException_OnPut()
        {
            //Arrange
            _commentService.Setup(x => x.EditComment(_commentId, User.Id, _comment.CommentText))
                           .Throws<SecurityException>();

            var json = JsonConvert.SerializeObject(_comment);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            AddValidHMACHeaders("PUT", ControllerAddress, json);
            SetupMocksForHMACAuth();

            //Act
            var response = await Client.PutAsync(ControllerAddress, httpContent);

            //Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task Return204WhenSuccessful_OnPut()
        {
            //Arrange
            _commentService.Setup(x => x.EditComment(_commentId, User.Id, _comment.CommentText));

            var json = JsonConvert.SerializeObject(_comment);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            AddValidHMACHeaders("PUT", ControllerAddress, json);
            SetupMocksForHMACAuth();

            //Act
            var response = await Client.PutAsync(ControllerAddress, httpContent);

            //Assert
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task Return403OnSecurityException_OnDelete()
        {
            //Arrange
            _commentService.Setup(x => x.RemoveComment(_commentId, User.Id))
                           .Throws<SecurityException>();

            AddValidHMACHeaders("DELETE");
            SetupMocksForHMACAuth();

            //Act
            var response = await Client.DeleteAsync(ControllerAddress);

            //Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
            MockRepo.VerifyAll();
        }
    }
}