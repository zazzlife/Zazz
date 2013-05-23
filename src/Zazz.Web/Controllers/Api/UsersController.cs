using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Zazz.Web.Models.Api;

namespace Zazz.Web.Controllers.Api
{
    public class UsersController : BaseApiController
    {
        // GET /api/v1/users/5
        public ApiUser Get(int id)
        {
            throw new NotImplementedException();
        }

        // PUT /api/v1/users/5
        public void Put(int id, ApiUser user)
        {
            throw new NotImplementedException();
        }
    }
}
