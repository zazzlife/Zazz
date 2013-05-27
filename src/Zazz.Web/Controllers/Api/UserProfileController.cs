using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Zazz.Core.Interfaces;
using Zazz.Web.Models.Api;

namespace Zazz.Web.Controllers.Api
{
    public class UserProfileController : BaseApiController
    {
        private readonly IUserService _userService;

        public UserProfileController(IUserService userService)
        {
            _userService = userService;
        }

        // GET api/v1/userprofile
        public ApiUserProfile Get()
        {
            throw new NotImplementedException();
        }

        // GET api/v1/userprofile/5
        public ApiUserProfile Get(int id)
        {
            throw new NotImplementedException();
        }
    }
}
