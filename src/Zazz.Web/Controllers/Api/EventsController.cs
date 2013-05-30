using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Zazz.Core.Exceptions;
using Zazz.Core.Interfaces;
using Zazz.Web.Filters;
using Zazz.Web.Interfaces;
using Zazz.Web.Models.Api;

namespace Zazz.Web.Controllers.Api
{
    [HMACAuthorize]
    public class EventsController : BaseApiController
    {
        private readonly IEventService _eventService;
        private readonly IObjectMapper _objectMapper;

        public EventsController(IEventService eventService, IObjectMapper objectMapper)
        {
            _eventService = eventService;
            _objectMapper = objectMapper;
        }

        // GET api/v1/events/5
        public ApiEvent Get(int id)
        {
            if (id == 0)
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            try
            {
                var e = _eventService.GetEvent(id);
                return _objectMapper.EventToApiEvent(e);
            }
            catch (NotFoundException)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            catch (Exception)
            {
                throw;
            }
        }

        // POST api/v1/events
        public ApiEvent Post([FromBody]string value)
        {
            throw new NotImplementedException();
        }

        // PUT api/v1/events/5
        public void Put(int id, [FromBody]string value)
        {
            throw new NotImplementedException();
        }

        // DELETE api/v1/events/5
        public void Delete(int id)
        {
            throw new NotImplementedException();
        }
    }
}
