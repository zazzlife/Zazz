﻿using System;
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
using Zazz.Core.Models;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;
using Zazz.Web.Models.Api;

namespace Zazz.UnitTests.Web.Controllers.Api
{
    [TestFixture]
    public class WeekliesControllerShould : BaseOAuthTests
    {
        private int _weeklyId;
        private Mock<IWeeklyService> _weeklyService;

        public override void Init()
        {
            base.Init();

            _weeklyId = 21;
            ControllerAddress = "/api/v1/weeklies/" + _weeklyId;

            _weeklyService = MockRepo.Create<IWeeklyService>();
            IocContainer.Configure(x =>
                                   {
                                       x.For<IWeeklyService>().Use(_weeklyService.Object);
                                   });
        }

        [Test]
        public async Task Return400IfIdIs0_OnGet()
        {
            //Arrange
            ControllerAddress = "/api/v1/weeklies/0";

            CreateValidAccessToken();

            //Act
            var result = await Client.GetAsync(ControllerAddress);

            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task Return404IfWeeklyNotExists_OnGet()
        {
            //Arrange
            CreateValidAccessToken();

            _weeklyService.Setup(x => x.GetWeekly(_weeklyId))
                          .Throws<NotFoundException>();

            //Act
            var result = await Client.GetAsync(ControllerAddress);

            //Assert
            Assert.AreEqual(HttpStatusCode.NotFound, result.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task ReturnWeekly_OnGet()
        {
            //Arrange
            CreateValidAccessToken();

            var weekly = new Weekly
                         {
                             Id = _weeklyId,
                             Description = "desc",
                             Name = "name",
                         };

            _weeklyService.Setup(x => x.GetWeekly(_weeklyId))
                          .Returns(weekly);

            //Act
            var result = await Client.GetAsync(ControllerAddress);

            //Assert
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task Return400IfUserIsNotClubAdmin_OnPost()
        {
            //Arrange
            ControllerAddress = "/api/v1/weeklies";

            var weekly = new Weekly
            {
                Description = "desc",
                Name = "name",
            };

            var json = JsonConvert.SerializeObject(weekly);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _weeklyService.Setup(x => x.CreateWeekly(It.IsAny<Weekly>()))
                          .Throws<InvalidOperationException>();

            CreateValidAccessToken();

            //Act
            var result = await Client.PostAsync(ControllerAddress, content);

            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task Return400IfClubHasCreated7Weeklies_OnPost()
        {
            //Arrange
            ControllerAddress = "/api/v1/weeklies";

            var weekly = new Weekly
            {
                Description = "desc",
                Name = "name",
            };

            var json = JsonConvert.SerializeObject(weekly);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _weeklyService.Setup(x => x.CreateWeekly(It.IsAny<Weekly>()))
                          .Throws<WeekliesLimitReachedException>();

            CreateValidAccessToken();

            //Act
            var result = await Client.PostAsync(ControllerAddress, content);

            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task CreateWeekly_OnPost()
        {
            //Arrange
            ControllerAddress = "/api/v1/weeklies";

            var weekly = new Weekly
            {
                Description = "desc",
                Name = "name",
                DayOfTheWeek = DayOfTheWeek.Thursday,
                PhotoId = 44,
            };

            var json = JsonConvert.SerializeObject(weekly);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _weeklyService.Setup(x => x.CreateWeekly(It.Is<Weekly>(w => w.DayOfTheWeek == weekly.DayOfTheWeek &&
                                                                        w.Description == weekly.Description &&
                                                                        w.Name == weekly.Name &&
                                                                        w.PhotoId == weekly.PhotoId &&
                                                                        w.UserId == User.Id)));


            PhotoService.Setup(x => x.GeneratePhotoUrl(User.Id, weekly.PhotoId.Value))
                        .Returns(new PhotoLinks("links"));

            CreateValidAccessToken();
            //Act
            var response = await Client.PostAsync(ControllerAddress, content);
            var model = JsonConvert.DeserializeObject<ApiWeekly>(await response.Content.ReadAsStringAsync());

            //Assert
            Assert.IsNotNull(model);
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task Return400IfIdIs0_OnPut()
        {
            //Arrange
            ControllerAddress = "/api/v1/weeklies/0";

            var weekly = new Weekly
            {
                Description = "desc",
                Name = "name",
                DayOfTheWeek = DayOfTheWeek.Thursday,
                PhotoId = 44,
            };

            var json = JsonConvert.SerializeObject(weekly);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            CreateValidAccessToken();

            //Act
            var result = await Client.PutAsync(ControllerAddress, content);

            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task Return404IfWeeklyDoesntExists_OnPut()
        {
            //Arrange

            var weekly = new Weekly
            {
                Description = "desc",
                Name = "name",
                DayOfTheWeek = DayOfTheWeek.Thursday,
                PhotoId = 44,
            };

            var json = JsonConvert.SerializeObject(weekly);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _weeklyService.Setup(x => x.EditWeekly(It.Is<Weekly>(w => w.DayOfTheWeek == weekly.DayOfTheWeek &&
                                                                      w.Description == weekly.Description &&
                                                                      w.Name == weekly.Name &&
                                                                      w.PhotoId == weekly.PhotoId &&
                                                                      w.Id == _weeklyId), User.Id))
                          .Throws<NotFoundException>();

            CreateValidAccessToken();

            //Act
            var result = await Client.PutAsync(ControllerAddress, content);

            //Assert
            Assert.AreEqual(HttpStatusCode.NotFound, result.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task Return403IfUserIsNotAuthorizedToEdit_OnPut()
        {
            //Arrange

            var weekly = new Weekly
            {
                Description = "desc",
                Name = "name",
                DayOfTheWeek = DayOfTheWeek.Thursday,
                PhotoId = 44,
            };

            var json = JsonConvert.SerializeObject(weekly);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _weeklyService.Setup(x => x.EditWeekly(It.Is<Weekly>(w => w.DayOfTheWeek == weekly.DayOfTheWeek &&
                                                                      w.Description == weekly.Description &&
                                                                      w.Name == weekly.Name &&
                                                                      w.PhotoId == weekly.PhotoId &&
                                                                      w.Id == _weeklyId), User.Id))
                          .Throws<SecurityException>();

            CreateValidAccessToken();

            //Act
            var result = await Client.PutAsync(ControllerAddress, content);

            //Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, result.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task Return204OnSuccess_OnPut()
        {
            //Arrange

            var weekly = new Weekly
            {
                Description = "desc",
                Name = "name",
                DayOfTheWeek = DayOfTheWeek.Thursday,
                PhotoId = 44,
            };

            var json = JsonConvert.SerializeObject(weekly);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _weeklyService.Setup(x => x.EditWeekly(It.Is<Weekly>(w => w.DayOfTheWeek == weekly.DayOfTheWeek &&
                                                                      w.Description == weekly.Description &&
                                                                      w.Name == weekly.Name &&
                                                                      w.PhotoId == weekly.PhotoId &&
                                                                      w.Id == _weeklyId), User.Id));

            CreateValidAccessToken();

            //Act
            var result = await Client.PutAsync(ControllerAddress, content);

            //Assert
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task Return400IfIdIs0_OnDelete()
        {
            //Arrange
            ControllerAddress = "/api/v1/weeklies/0";

            CreateValidAccessToken();

            //Act
            var result = await Client.DeleteAsync(ControllerAddress);

            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task Return403IfUserIsNotAllowedToRemove_OnDelete()
        {
            //Arrange
            CreateValidAccessToken();

            _weeklyService.Setup(x => x.RemoveWeekly(_weeklyId, User.Id))
                          .Throws<SecurityException>();

            //Act
            var result = await Client.DeleteAsync(ControllerAddress);

            //Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, result.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task Return204OnSuccess_OnDelete()
        {
            //Arrange
            CreateValidAccessToken();

            _weeklyService.Setup(x => x.RemoveWeekly(_weeklyId, User.Id));

            //Act
            var result = await Client.DeleteAsync(ControllerAddress);

            //Assert
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
            MockRepo.VerifyAll();
        }
    }
}