using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Zazz.Web.Filters;
using Zazz.Web.Models.Api;

namespace Zazz.Web.Controllers.Api
{
    [HMACAuthorize]
    public class EventsController : BaseApiController
    {
        // GET api/v1/events/5
        public ApiEvent Get(int id)
        {
            throw new NotImplementedException();
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
