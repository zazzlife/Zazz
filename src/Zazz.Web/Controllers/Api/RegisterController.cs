using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Zazz.Core.Interfaces;
using Zazz.Web.Filters;
using Zazz.Web.Models.Api;

namespace Zazz.Web.Controllers.Api
{
    public class RegisterController : ApiController
    {
        private readonly IAppRequestTokenService _appRequestTokenService;

        public RegisterController(IAppRequestTokenService appRequestTokenService)
        {
            _appRequestTokenService = appRequestTokenService;
        }

        [HMACAuthorize(true)]
        public ApiAppToken Get()
        {
            throw new NotImplementedException();
        }
    }
}
