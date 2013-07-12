using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Zazz.Web.Filters;

namespace Zazz.Web.Controllers.Api
{
    [OAuth2Authorize]
    public class AvailableRewardsController : ApiController
    {
        public ApiUserReward Get()
        {
            return null;
        }
    }
}
