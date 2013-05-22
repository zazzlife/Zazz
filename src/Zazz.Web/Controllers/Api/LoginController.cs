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
    public class LoginController : ApiController
    {
        [HMACAuthorize(IgnoreUserIdAndPassword = true)]
        public LoginApiResponse Get(LoginApiRequest request)
        {
            return null;
        }
    }
}
