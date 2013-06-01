using System;
using Moq;
using NUnit.Framework;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Web.Models.Api;

namespace Zazz.UnitTests.Web.Controllers.Api
{
    [TestFixture]
    public class PhotosControllerShould : BaseHMACTests
    {
        private int _photoId;
        private Mock<IPhotoService> _photoService;
        private ApiPhoto _apiPhoto;

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

            IocContainer.Configure(x =>
            {
                x.For<IPhotoService>().Use(_photoService.Object);
            });
        }
    }
}