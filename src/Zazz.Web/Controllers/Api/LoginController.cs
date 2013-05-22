using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Zazz.Core.Interfaces;
using Zazz.Web.Filters;
using Zazz.Web.Models.Api;

namespace Zazz.Web.Controllers.Api
{
    public class LoginController : ApiController
    {
        private IUserService _userService;

        public LoginController(IUserService userService)
        {
            _userService = userService;
        }

        [HMACAuthorize(IgnoreUserIdAndPassword = true)]
        public LoginApiResponse Get(string username, string password)
        {
            var user = _userService.GetUser(username, true, true);
            if (user == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);


            return null;
        }
    }
}
