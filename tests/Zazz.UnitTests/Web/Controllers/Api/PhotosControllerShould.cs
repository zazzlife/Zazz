using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Newtonsoft.Json;
using Zazz.Core.Exceptions;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Web.Interfaces;
using Zazz.Web.Models.Api;

namespace Zazz.UnitTests.Web.Controllers.Api
{
    [TestFixture]
    public class PhotosControllerShould : BaseOAuthTests
    {
        private int _photoId;
        private Mock<IPhotoService> _photoService;
        private ApiPhoto _apiPhoto;
        private Mock<IObjectMapper> _objectMapper;
        private Mock<IImageValidator> _imageValidator;

        private const string PHOTO = "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";

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
            _objectMapper = MockRepo.Create<IObjectMapper>();
            _imageValidator = MockRepo.Create<IImageValidator>();

            IocContainer.Configure(x =>
            {
                x.For<IPhotoService>().Use(_photoService.Object);
                x.For<IObjectMapper>().Use(_objectMapper.Object);
                x.For<IImageValidator>().Use(_imageValidator.Object);
            });
        }

        [TestCase(0)]
        [TestCase(-1)]
        public async Task Return400IfUserIdIsLessThan1_OnGet(int userId)
        {
            //Arrange
            ControllerAddress = String.Format("/api/v1/users/{0}/photos", userId);

            CreateValidAccessToken();

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
            ControllerAddress = String.Format("/api/v1/users/{0}/photos", userId);

            _photoService.Setup(x => x.GetUserPhotos(userId, defaultPageSize, null))
                         .Returns(new EnumerableQuery<Photo>(Enumerable.Empty<Photo>()));

            CreateValidAccessToken();

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
            ControllerAddress = String.Format("/api/v1/users/{0}/photos?lastPhoto={1}", userId, lastPhoto);

            _photoService.Setup(x => x.GetUserPhotos(userId, defaultPageSize, lastPhoto))
                         .Returns(new EnumerableQuery<Photo>(Enumerable.Empty<Photo>()));

            CreateValidAccessToken();

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

            CreateValidAccessToken();

            //Act
            var result = await Client.GetAsync(ControllerAddress);

            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task Return404IfPhotoDoesntExists_OnGet()
        {
            //Arrange
            CreateValidAccessToken();

            _photoService.Setup(x => x.GetPhoto(_photoId))
                         .Returns(() => null);

            //Act
            var result = await Client.GetAsync(ControllerAddress);

            //Assert
            Assert.AreEqual(HttpStatusCode.NotFound, result.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task ReturnPhoto_OnGet()
        {
            //Arrange
            CreateValidAccessToken();

            _photoService.Setup(x => x.GetPhoto(_photoId))
                         .Returns(new Photo());
            _objectMapper.Setup(x => x.PhotoToApiPhoto(It.IsAny<Photo>()))
                         .Returns(new ApiPhoto());

            //Act
            var result = await Client.GetAsync(ControllerAddress);

            //Assert
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task Return415IfItsNotSendingMultipartFormData_OnPost()
        {
            //Arrange
            var json = JsonConvert.SerializeObject(_apiPhoto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            ControllerAddress = "/api/v1/photos";

            CreateValidAccessToken();

            //Act
            var response = await Client.PostAsync(ControllerAddress, content);

            //Assert
            Assert.AreEqual(HttpStatusCode.UnsupportedMediaType, response.StatusCode);
        }

        [Test]
        public async Task Return400IfPhotoIsNotProvided_OnPost()
        {
            //Arrange
            var values = new[]
                         {
                             new KeyValuePair<string, string>("description", "this is the description"),
                             new KeyValuePair<string, string>("albumId", "5"),
                             new KeyValuePair<string, string>("showInFeed", "true"),
                         };

            var content = new MultipartFormDataContent();
            foreach (var v in values)
                content.Add(new StringContent(v.Value), v.Key);

            var stringContent = await content.ReadAsStringAsync();

            CreateValidAccessToken();

            //Act
            var response = await Client.PostAsync(ControllerAddress, content);

            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task Return400IfPhotoIsNotValid_OnPost()
        {
            //Arrange
            var values = new[]
                         {
                             new KeyValuePair<string, string>("description", "this is the description"),
                             new KeyValuePair<string, string>("albumId", "5"),
                             new KeyValuePair<string, string>("showInFeed", "true"),
                         };

            var photoContent = new ByteArrayContent(Convert.FromBase64String(PHOTO));
            photoContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                                                      {
                                                          FileName = "file.jpg",
                                                          Name = "photo"
                                                      };


            var content = new MultipartFormDataContent();
            foreach (var v in values)
                content.Add(new StringContent(v.Value), v.Key);

            content.Add(photoContent);

            var stringContent = await content.ReadAsStringAsync();

            CreateValidAccessToken();

            _imageValidator.Setup(x => x.IsValid(It.IsAny<Stream>()))
                           .Returns(false);

            //Act
            var response = await Client.PostAsync(ControllerAddress, content);

            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task Return200WhenEverythingIsOk_OnPost()
        {
            //Arrange
            var description = "this is the description";
            var albumId = 5;
            var showInFeed = true;

            var values = new[]
                         {
                             new KeyValuePair<string, string>("description", description),
                             new KeyValuePair<string, string>("albumId", albumId.ToString()),
                             new KeyValuePair<string, string>("showInFeed", showInFeed.ToString()),
                         };

            var photoContent = new ByteArrayContent(Convert.FromBase64String(PHOTO));
            photoContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                FileName = "file.jpg",
                Name = "photo"
            };


            var content = new MultipartFormDataContent();
            foreach (var v in values)
                content.Add(new StringContent(v.Value), v.Key);

            content.Add(photoContent);

            var stringContent = await content.ReadAsStringAsync();

            CreateValidAccessToken();

            _imageValidator.Setup(x => x.IsValid(It.IsAny<Stream>()))
                           .Returns(true);

            //_photoService.Setup(x => x.SavePhoto(It.Is<Photo>(p => p.Description == description &&
            //                                                       p.UserId == User.Id &&
            //                                                       p.AlbumId == albumId &&
            //                                                       p.IsFacebookPhoto == false &&
            //                                                       p.UploadDate >= DateTime.UtcNow),
            //                                     It.IsAny<Stream>(), showInFeed))
            //             .Returns(1);

            _photoService.Setup(x => x.SavePhoto(It.IsAny<Photo>(),
                                                 It.IsAny<Stream>(), showInFeed, It.IsAny<IEnumerable<byte>>()))
                         .Returns(1);
            _objectMapper.Setup(x => x.PhotoToApiPhoto(It.IsAny<Photo>()))
                         .Returns(new ApiPhoto());

            //Act
            var response = await Client.PostAsync(ControllerAddress, content);

            //Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task Return403OnSecurityException_OnPost()
        {
            //Arrange
            var description = "this is the description";
            var albumId = 5;
            var showInFeed = true;

            var values = new[]
                         {
                             new KeyValuePair<string, string>("description", description),
                             new KeyValuePair<string, string>("albumId", albumId.ToString()),
                             new KeyValuePair<string, string>("showInFeed", showInFeed.ToString()),
                         };

            var photoContent = new ByteArrayContent(Convert.FromBase64String(PHOTO));
            photoContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                FileName = "file.jpg",
                Name = "photo"
            };


            var content = new MultipartFormDataContent();
            foreach (var v in values)
                content.Add(new StringContent(v.Value), v.Key);

            content.Add(photoContent);

            var stringContent = await content.ReadAsStringAsync();

            CreateValidAccessToken();

            _imageValidator.Setup(x => x.IsValid(It.IsAny<Stream>()))
                           .Returns(true);

            _photoService.Setup(x => x.SavePhoto(It.IsAny<Photo>(),
                                                 It.IsAny<Stream>(), showInFeed, It.IsAny<IEnumerable<byte>>()))
                         .Throws<SecurityException>();

            //Act
            var response = await Client.PostAsync(ControllerAddress, content);

            //Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task Return400IfIdIs0_OnPut()
        {
            //Arrange
            ControllerAddress = "/api/v1/photos/0";

            var json = JsonConvert.SerializeObject(_apiPhoto);
            var stringContent = new StringContent(json, Encoding.UTF8, "application/json");

            CreateValidAccessToken();
            //Act
            var response = await Client.PutAsync(ControllerAddress, stringContent);

            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task Return404IfPhotoDoesntExists_OnPut()
        {
            //Arrange
            var json = JsonConvert.SerializeObject(_apiPhoto);
            var stringContent = new StringContent(json, Encoding.UTF8, "application/json");

            CreateValidAccessToken();

            _photoService.Setup(x => x.UpdatePhoto(It.IsAny<Photo>(), User.Id))
                         .Throws<NotFoundException>();

            //Act
            var response = await Client.PutAsync(ControllerAddress, stringContent);

            //Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task Return403WhenUserIsNotAuthorizedToEditPhoto_OnPut()
        {
            //Arrange
            var json = JsonConvert.SerializeObject(_apiPhoto);
            var stringContent = new StringContent(json, Encoding.UTF8, "application/json");

            CreateValidAccessToken();
            _photoService.Setup(x => x.UpdatePhoto(It.IsAny<Photo>(), User.Id))
                         .Throws<SecurityException>();

            //Act
            var response = await Client.PutAsync(ControllerAddress, stringContent);

            //Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task Return204OnSuccess_OnPut()
        {
            //Arrange
            var json = JsonConvert.SerializeObject(_apiPhoto);
            var stringContent = new StringContent(json, Encoding.UTF8, "application/json");

            CreateValidAccessToken();
            _photoService.Setup(x => x.UpdatePhoto(It.IsAny<Photo>(), User.Id));

            //Act
            var response = await Client.PutAsync(ControllerAddress, stringContent);

            //Assert
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task Return400WhenIdIs0_OnDelete()
        {
            //Arrange
            ControllerAddress = "/api/v1/photos/0";

            CreateValidAccessToken();

            //Act
            var response = await Client.DeleteAsync(ControllerAddress);

            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        public async Task Return403WhenUserIsNotAuthorized_OnDelete()
        {
            //Arrange
            CreateValidAccessToken();

            _photoService.Setup(x => x.RemovePhoto(_photoId, User.Id))
                         .Throws<SecurityException>();

            //Act
            var response = await Client.DeleteAsync(ControllerAddress);

            //Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Test]
        public async Task Return204OnSuccess_OnDelete()
        {
            //Arrange
            CreateValidAccessToken();

            _photoService.Setup(x => x.RemovePhoto(_photoId, User.Id));

            //Act
            var response = await Client.DeleteAsync(ControllerAddress);

            //Assert
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        }
    }
}