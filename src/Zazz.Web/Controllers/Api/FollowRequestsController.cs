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
    public class FollowRequestsController : ApiController
    {
        // GET api/v1/followrequests
        public IEnumerable<ApiFollowRequest> Get()
        {
            throw new NotImplementedException();
        }

        // DELETE api/v1/followrequests/5?action=accept/reject
        public void Delete(int id, string action)
        {
            throw new NotImplementedException();
        }
    }
}
