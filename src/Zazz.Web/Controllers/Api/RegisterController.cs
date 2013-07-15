using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data.Enums;
using Zazz.Web.Filters;
using Zazz.Web.Models.Api;

namespace Zazz.Web.Controllers.Api
{
    public class RegisterController : ApiController
    {
        private readonly IAuthService _authService;

        public RegisterController(IAuthService authService)
        {
            _authService = authService;
        }

        public ApiLoginResponse Post(ApiRegister register)
        {
            return null;
        }
    }
}
