using System;
using Moq;
using NUnit.Framework;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.UnitTests.Web.Controllers.Api
{
    [TestFixture]
    public class AlbumControllerShould : BaseHMACTests
    {
        private Mock<IAlbumService> _albumService;
        private int _albumId;

        public override void Init()
        {
            base.Init();

            _albumId = 444;
            ControllerAddress = "/api/v1/albums/" + _albumId;

            _albumService = MockRepo.Create<IAlbumService>();

            IocContainer.Configure(x =>
            {
                x.For<IAlbumService>().Use(_albumService.Object);
            });
        }
    }
}