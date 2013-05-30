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
using Zazz.Web.Interfaces;
using Zazz.Web.Models.Api;

namespace Zazz.UnitTests.Web.Controllers.Api
{
    [TestFixture]
    public class EventsControllerShould : BaseHMACTests
    {
        private int _eventId;
        private Mock<IEventService> _eventService;
        private Mock<IObjectMapper> _objectMapper;

        public override void Init()
        {
            base.Init();

            _eventId = 332;
            ControllerAddress = "/api/v1/events/" + _eventId;

            _eventService = MockRepo.Create<IEventService>();
            _objectMapper = MockRepo.Create<IObjectMapper>();

            IocContainer.Configure(x =>
                                   {
                                       x.For<IEventService>().Use(_eventService.Object);
                                       x.For<IObjectMapper>().Use(_objectMapper.Object);
                                   });
        }

        [Test]
        public async Task Return400IfIdIs0_OnGet()
        {
            //Arrange
            ControllerAddress = "/api/v1/events/" + 0;

            AddValidHMACHeaders("GET");
            SetupMocksForHMACAuth();

            //Act
            var result = await Client.GetAsync(ControllerAddress);

            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task Return404IfEventWasNotFound_OnGet()
        {
            //Arrange
            _eventService.Setup(x => x.GetEvent(_eventId))
                         .Throws<NotFoundException>();

            AddValidHMACHeaders("GET");
            SetupMocksForHMACAuth();

            //Act
            var result = await Client.GetAsync(ControllerAddress);

            //Assert
            Assert.AreEqual(HttpStatusCode.NotFound, result.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task Return200OnSuccess_OnGet()
        {
            //Arrange
            _eventService.Setup(x => x.GetEvent(_eventId))
                .Returns(new ZazzEvent());
            _objectMapper.Setup(x => x.EventToApiEvent(It.IsAny<ZazzEvent>()))
                         .Returns(new ApiEvent());

            AddValidHMACHeaders("GET");
            SetupMocksForHMACAuth();

            //Act
            var result = await Client.GetAsync(ControllerAddress);

            //Assert
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            MockRepo.VerifyAll();
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public async Task Return400IfNameIsMissing_OnPost(string name)
        {
            //Arrange
            ControllerAddress = "/api/v1/events/";

            var e = new ApiEvent
                    {
                        Description = "d",
                        Name = name,
                        UtcTime = DateTime.UtcNow.AddMinutes(1),
                        Time = DateTimeOffset.Now.AddMinutes(1),
                    };

            var json = JsonConvert.SerializeObject(e);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            AddValidHMACHeaders("POST", ControllerAddress, json);
            SetupMocksForHMACAuth();

            //Act
            var result = await Client.PostAsync(ControllerAddress, httpContent);

            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
            MockRepo.VerifyAll();
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public async Task Return400IfDescriptionIsMissing_OnPost(string desc)
        {
            //Arrange
            ControllerAddress = "/api/v1/events/";

            var e = new ApiEvent
            {
                Description = desc,
                Name = "name",
                UtcTime = DateTime.UtcNow.AddMinutes(1),
                Time = DateTimeOffset.Now.AddMinutes(1),
            };

            var json = JsonConvert.SerializeObject(e);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            AddValidHMACHeaders("POST", ControllerAddress, json);
            SetupMocksForHMACAuth();

            //Act
            var result = await Client.PostAsync(ControllerAddress, httpContent);

            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task Return200OnSuccess_OnPost()
        {
            //Arrange
            ControllerAddress = "/api/v1/events/";

            var e = new ApiEvent
            {
                Description = "d",
                Name = "name",
                UtcTime = DateTime.UtcNow.AddMinutes(1),
                Time = DateTimeOffset.Now.AddMinutes(1),
            };

            _eventService.Setup(x => x.CreateEvent(It.IsAny<ZazzEvent>()));

            var json = JsonConvert.SerializeObject(e);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            AddValidHMACHeaders("POST", ControllerAddress, json);
            SetupMocksForHMACAuth();

            //Act
            var result = await Client.PostAsync(ControllerAddress, httpContent);

            //Assert
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task Return400IfIdIs0_OnPut()
        {
            //Arrange
            ControllerAddress = "/api/v1/events/" + 0;

            var e = new ApiEvent
                    {
                        Description = "d",
                        Name = "name",
                        UtcTime = DateTime.UtcNow.AddMinutes(1),
                        Time = DateTimeOffset.Now.AddMinutes(1),
                    };

            var json = JsonConvert.SerializeObject(e);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            AddValidHMACHeaders("PUT", ControllerAddress, json);
            SetupMocksForHMACAuth();

            //Act
            var result = await Client.PutAsync(ControllerAddress, httpContent);

            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task Return404IfEventDoesntExists_OnPut()
        {
            //Arrange
            var e = new ApiEvent
            {
                Description = "d",
                Name = "name",
                UtcTime = DateTime.UtcNow.AddMinutes(1),
                Time = DateTimeOffset.Now.AddMinutes(1),
            };

            _eventService.Setup(x => x.UpdateEvent(It.IsAny<ZazzEvent>(), User.Id))
                         .Throws<NotFoundException>();

            var json = JsonConvert.SerializeObject(e);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            AddValidHMACHeaders("PUT", ControllerAddress, json);
            SetupMocksForHMACAuth();

            //Act
            var result = await Client.PutAsync(ControllerAddress, httpContent);

            //Assert
            Assert.AreEqual(HttpStatusCode.NotFound, result.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task Return403IfUserIsNotAuthorizedToEditEvent_OnPut()
        {
            //Arrange
            var e = new ApiEvent
            {
                Description = "d",
                Name = "name",
                UtcTime = DateTime.UtcNow.AddMinutes(1),
                Time = DateTimeOffset.Now.AddMinutes(1),
            };

            _eventService.Setup(x => x.UpdateEvent(It.IsAny<ZazzEvent>(), User.Id))
                         .Throws<SecurityException>();

            var json = JsonConvert.SerializeObject(e);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            AddValidHMACHeaders("PUT", ControllerAddress, json);
            SetupMocksForHMACAuth();

            //Act
            var result = await Client.PutAsync(ControllerAddress, httpContent);

            //Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, result.StatusCode);
            MockRepo.VerifyAll();
        }

        [Test]
        public async Task Return204OnSuccess_OnPut()
        {
            //Arrange
            var e = new ApiEvent
            {
                Description = "d",
                Name = "name",
                UtcTime = DateTime.UtcNow.AddMinutes(1),
                Time = DateTimeOffset.Now.AddMinutes(1),
            };

            _eventService.Setup(x => x.UpdateEvent(It.IsAny<ZazzEvent>(), User.Id));

            var json = JsonConvert.SerializeObject(e);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            AddValidHMACHeaders("PUT", ControllerAddress, json);
            SetupMocksForHMACAuth();

            //Act
            var result = await Client.PutAsync(ControllerAddress, httpContent);

            //Assert
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
            MockRepo.VerifyAll();
        }
    }
}