using System;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.UnitTests.Web.Controllers.Api
{
    public class EventsControllerShould : BaseHMACTests
    {
        private int _eventId;

        public override void Init()
        {
            base.Init();

            _eventId = 332;
            ControllerAddress = "/api/v1/events/" + _eventId;

            IocContainer.Configure(x =>
            {
            });
        }
    }
}