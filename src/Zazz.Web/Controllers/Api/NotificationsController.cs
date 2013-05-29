using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Zazz.Web.Models.Api;

namespace Zazz.Web.Controllers.Api
{
    public class NotificationsController : ApiController
    {
        // GET api/v1/notifications
        public IEnumerable<ApiNotification> Get(int? lastNotification)
        {
            throw new NotImplementedException();
        }

        // DELETE api/v1/notifications/5
        public void Delete(int id)
        {
            throw new NotImplementedException();
        }
    }
}
