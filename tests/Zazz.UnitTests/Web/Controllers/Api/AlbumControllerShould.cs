﻿using System;
using System.Collections.Generic;
using System.Linq;
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
using Zazz.Core.Interfaces.Services;
using Zazz.Core.Models.Data;
using Zazz.Web.Interfaces;
using Zazz.Web.Models.Api;

namespace Zazz.UnitTests.Web.Controllers.Api
{
    [TestFixture]
    public class AlbumControllerShould : BaseOAuthTests
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
            ControllerAddress = String.Format("/api/v1/users/{0}/albums", userId);

            CreateValidAccessToken();

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
            ControllerAddress = String.Format("/api/v1/users/{0}/albums", User.Id);

            CreateValidAccessToken();

            _albumService.Setup(x => x.GetUserAlbums(User.Id, true))
                         .Returns(new EnumerableQuery<Album>(Enumerable.Empty<Album>()));

            //Act
            var response = await Client.GetAsync(ControllerAddress);

            //Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task Return400IfIdIs0_OnGet()
        {
            //Arrange
            ControllerAddress = "/api/v1/albums/0";

            CreateValidAccessToken();

            //Act
            var response = await Client.GetAsync(ControllerAddress);

            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task Return404IfAlbumDoesntExists_OnGet()
        {
            //Arrange
            CreateValidAccessToken();

            _albumService.Setup(x => x.GetAlbum(_albumId, true))
                         .Returns(() => null);

            //Act
            var response = await Client.GetAsync(ControllerAddress);

            //Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task ReturnAlbum_OnGet()
        {
            //Arrange
            CreateValidAccessToken();

            var album = new Album
                        {
                            Photos = new List<Photo>()
                        };

            _albumService.Setup(x => x.GetAlbum(_albumId, true))
                         .Returns(album);

            //Act
            var response = await Client.GetAsync(ControllerAddress);

            //Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task CreateNewAlbum_OnPost()
        {
            //Arrange
            ControllerAddress = "/api/v1/albums";

            var album = new ApiAlbum
                        {
                            Name = "name",
                        };

            var json = JsonConvert.SerializeObject(album);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            CreateValidAccessToken();

            _albumService.Setup(x => x.CreateAlbum(It.Is<Album>(a => a.Name == album.Name &&
                                                                     a.UserId == User.Id &&
                                                                     a.IsFacebookAlbum == false)));

            //Act
            var response = await Client.PostAsync(ControllerAddress, content);
            var model = JsonConvert.DeserializeObject<ApiAlbum>(await response.Content.ReadAsStringAsync());

            //Assert
            Assert.IsNotNull(model);
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task Returning400IfIdIs0_OnPut()
        {
            //Arrange
            ControllerAddress = "/api/v1/albums/0";

            var album = new ApiAlbum
                        {
                            Name = "name",
                        };

            var json = JsonConvert.SerializeObject(album);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            CreateValidAccessToken();

            //Act
            var response = await Client.PutAsync(ControllerAddress, content);

            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task Returning404IfAlbumDoesntExists_OnPut()
        {
            //Arrange

            var album = new ApiAlbum
            {
                Name = "name",
            };

            var json = JsonConvert.SerializeObject(album);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            CreateValidAccessToken();

            _albumService.Setup(x => x.UpdateAlbum(_albumId, album.Name, User.Id))
                         .Throws<NotFoundException>();

            //Act
            var response = await Client.PutAsync(ControllerAddress, content);

            //Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task Returning403IfUserIsNotAuthorized_OnPut()
        {
            //Arrange

            var album = new ApiAlbum
            {
                Name = "name",
            };

            var json = JsonConvert.SerializeObject(album);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            CreateValidAccessToken();

            _albumService.Setup(x => x.UpdateAlbum(_albumId, album.Name, User.Id))
                         .Throws<SecurityException>();

            //Act
            var response = await Client.PutAsync(ControllerAddress, content);

            //Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task Returning204OnSuccess_OnPut()
        {
            //Arrange

            var album = new ApiAlbum
            {
                Name = "name",
            };

            var json = JsonConvert.SerializeObject(album);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            CreateValidAccessToken();

            _albumService.Setup(x => x.UpdateAlbum(_albumId, album.Name, User.Id));

            //Act
            var response = await Client.PutAsync(ControllerAddress, content);

            //Assert
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task Return400IfIdIs0_OnDelete()
        {
            //Arrange
            ControllerAddress = "/api/v1/albums/0";

            CreateValidAccessToken();

            //Act
            var response = await Client.DeleteAsync(ControllerAddress);

            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task Return403IfUserIsNotAuthorized_OnDelete()
        {
            //Arrange

            CreateValidAccessToken();

            _albumService.Setup(x => x.DeleteAlbum(_albumId, User.Id))
                         .Throws<SecurityException>();

            //Act
            var response = await Client.DeleteAsync(ControllerAddress);

            //Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task Return204OnSuccess_OnDelete()
        {
            //Arrange

            CreateValidAccessToken();

            _albumService.Setup(x => x.DeleteAlbum(_albumId, User.Id));

            //Act
            var response = await Client.DeleteAsync(ControllerAddress);

            //Assert
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
            MockRepo.VerifyAll();
        }
    }
}