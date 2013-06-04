using System;
using NUnit.Framework;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.UnitTests.Web.Controllers.Api
{
    [TestFixture]
    public class QRCodeControllerShould : BaseHMACTests
    {
        public override void Init()
        {
            base.Init();

            ControllerAddress = "/api/v1/qrcode";

            IocContainer.Configure(x =>
            {
            });
        }
    }
}