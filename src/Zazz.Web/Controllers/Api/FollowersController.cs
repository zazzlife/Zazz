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
    public class FollowersController : BaseApiController
    {
        public IEnumerable<ApiFollower> Get()
        {
            throw new NotImplementedException();
        }

        // POST api/v1/follows
        public void Post([FromBody]int userId)
        {
        }

        // DELETE api/v1/follows/5
        public void Delete(int id)
        {
        }
    }
}
