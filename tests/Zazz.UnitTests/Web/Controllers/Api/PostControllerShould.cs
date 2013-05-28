using System;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using System.Web.Http.SelfHost;
using Moq;
using NUnit.Framework;
using Newtonsoft.Json;
using StructureMap;
using Zazz.Core.Exceptions;
using Zazz.Core.Interfaces;
using Zazz.Core.Models;
using Zazz.Core.Models.Data;
using Zazz.Infrastructure.Services;
using Zazz.Web;
using Zazz.Web.DependencyResolution;

namespace Zazz.UnitTests.Web.Controllers.Api
{
    [TestFixture]
    public class PostControllerShould : BaseHMACTests
    {
        private Mock<IPostService> _postService;
        private Post _post;
        private int _postId;

        public override void Init()
        {
            base.Init();
            
            _postId = 99;
            ControllerAddress = "/api/v1/posts/" + _postId;

            _post = new Post
                    {
                        Id = _postId,
                        FromUserId = User.Id,
                        CreatedTime = DateTime.UtcNow,
                        Message = "Message"
                    };

            _postService = MockRepo.Create<IPostService>();
            IocContainer.Configure(x =>
                                   {
                                       x.For<IPostService>().Use(_postService.Object);
                                   });
        }

        private void SetupMocksForHMACAuth()
        {
            AppRepo.Setup(x => x.GetById(App.Id))
                   .Returns(App);
            UserService.Setup(x => x.GetUserPassword(User.Id))
                       .Returns(Encoding.UTF8.GetBytes(Password));

        }

        [Test]
        public async Task Return404IfPostIsNotFound_OnGet()
        {
            //Arrange
            AddValidHMACHeaders("GET");
            SetupMocksForHMACAuth();

            _postService.Setup(x => x.GetPost(_postId))
                        .Returns(() => null);

            //Act
            var response = await Client.GetAsync(ControllerAddress);

            //Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task Return200WhenEverythingIsOK_OnGet()
        {
            //Arrange
            AddValidHMACHeaders("GET");
            SetupMocksForHMACAuth();

            _postService.Setup(x => x.GetPost(_postId))
                        .Returns(_post);

            UserService.Setup(x => x.GetUserDisplayName(_post.FromUserId))
                       .Returns("display name");
            PhotoService.Setup(x => x.GetUserImageUrl(_post.FromUserId))
                        .Returns(new PhotoLinks("url"));

            //Act
            var response = await Client.GetAsync(ControllerAddress);

            //Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            MockRepo.VerifyAll();
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public async Task Return400IfPostMessageIsMissing_OnPost(string message)
        {
            //Arrange
            _post.Message = message;
            ControllerAddress = "/api/v1/posts";

            var json = JsonConvert.SerializeObject(_post);
            var postContent = new StringContent(json, Encoding.UTF8, "application/json");

            AddValidHMACHeaders("POST", ControllerAddress, json);
            SetupMocksForHMACAuth();

            //Act
            var response = await Client.PostAsync(ControllerAddress, postContent);

            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task AssignTheCurrentUserToPostAndSaveIt_OnPost()
        {
            //Arrange
            _post.FromUserId = User.Id + 1;
            ControllerAddress = "/api/v1/posts";

            var json = JsonConvert.SerializeObject(_post);
            var postContent = new StringContent(json, Encoding.UTF8, "application/json");

            AddValidHMACHeaders("POST", ControllerAddress, json);
            SetupMocksForHMACAuth();

            _postService.Setup(x => x.NewPost(It.Is<Post>(p => p.FromUserId == User.Id)));

            //Act
            var response = await Client.PostAsync(ControllerAddress, postContent);

            //Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            MockRepo.VerifyAll();
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public async Task Return400IfPostMessageIsEmpty_OnPut(string message)
        {
            //Arrange
            _post.Message = message;
            var json = JsonConvert.SerializeObject(_post);
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
        public async Task Return404ForNotFoundException_OnPut()
        {
            //Arrange
            var json = JsonConvert.SerializeObject(_post);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            AddValidHMACHeaders("PUT", ControllerAddress, json);
            SetupMocksForHMACAuth();

            _postService.Setup(x => x.EditPost(_postId, _post.Message, User.Id))
                        .Throws<NotFoundException>();

            //Act
            var response = await Client.PutAsync(ControllerAddress, httpContent);

            //Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task Return403ForSecurityException_OnPut()
        {
            //Arrange
            var json = JsonConvert.SerializeObject(_post);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            AddValidHMACHeaders("PUT", ControllerAddress, json);
            SetupMocksForHMACAuth();

            _postService.Setup(x => x.EditPost(_postId, _post.Message, User.Id))
                        .Throws<SecurityException>();

            //Act
            var response = await Client.PutAsync(ControllerAddress, httpContent);

            //Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task Return204IfSuccessful_OnPut()
        {
            //Arrange
            var json = JsonConvert.SerializeObject(_post);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            AddValidHMACHeaders("PUT", ControllerAddress, json);
            SetupMocksForHMACAuth();

            _postService.Setup(x => x.EditPost(_postId, _post.Message, User.Id));

            //Act
            var response = await Client.PutAsync(ControllerAddress, httpContent);

            //Assert
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task Return403ForSecurityException_OnDelete()
        {
            //Arrange
            AddValidHMACHeaders("DELETE", ControllerAddress);
            SetupMocksForHMACAuth();

            _postService.Setup(x => x.RemovePost(_postId, User.Id))
                        .Throws<SecurityException>();

            //Act
            var response = await Client.DeleteAsync(ControllerAddress);

            //Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task Return204IfSuccessful_OnDelete()
        {
            //Arrange
            AddValidHMACHeaders("DELETE", ControllerAddress);
            SetupMocksForHMACAuth();

            _postService.Setup(x => x.RemovePost(_postId, User.Id));

            //Act
            var response = await Client.DeleteAsync(ControllerAddress);

            //Assert
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
            MockRepo.VerifyAll();
        }
    }
}