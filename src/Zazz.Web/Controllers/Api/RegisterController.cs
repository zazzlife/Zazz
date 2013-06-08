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
        private readonly IAppRequestTokenService _appRequestTokenService;

        public RegisterController(IAppRequestTokenService appRequestTokenService)
        {
            _appRequestTokenService = appRequestTokenService;
        }

        [HMACAuthorize(true)]
        public ApiAppToken Get()
        {
            var authHeader = Request.Headers.Authorization.Parameter;
            var authSegments = authHeader.Split(':');
            int appId;

            if (!Int32.TryParse(authSegments[0], out appId))
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            var token = _appRequestTokenService.Create(appId, AppTokenType.Register);
            return new ApiAppToken
                   {
                       ExpirationTime = token.ExpirationTime,
                       RequestId = token.Id,
                       Token = BitConverter.ToString(token.Token).Replace("-", "")
                   };
        }
    }
}
