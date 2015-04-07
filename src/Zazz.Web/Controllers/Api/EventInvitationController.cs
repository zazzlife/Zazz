using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Web.Http;
using Zazz.Core.Exceptions;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Services;
using Zazz.Core.Models.Data;
using Zazz.Web.Filters;
using Zazz.Web.Interfaces;
using Zazz.Web.Models.Api;

namespace Zazz.Web.Controllers.Api
{
    [OAuth2Authorize]
    public class EventInvitationController : BaseApiController
    {

        private readonly INotificationService _notificationService;

        public EventInvitationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public void Post([FromBody] ApiEventInvitation e)
        {
           if(e.toUserId.Length < 1)
                throw new HttpResponseException(HttpStatusCode.BadRequest);

           _notificationService.CreateNewEventInvitationNotification(CurrentUserId, e.eventId, e.toUserId);    
        }
    }
}
