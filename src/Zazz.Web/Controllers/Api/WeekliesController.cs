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
    [HMACAuthorize]
    public class WeekliesController : BaseApiController
    {
        private readonly IWeeklyService _weeklyService;

        public WeekliesController(IWeeklyService weeklyService)
        {
            _weeklyService = weeklyService;
        }

        // GET api/v1/weeklies/5
        public ApiWeekly Get(int id)
        {
            if (id == 0)
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            throw new NotImplementedException();
        }

        // POST api/v1/weeklies
        public ApiWeekly Post(ApiWeekly weekly)
        {
            throw new NotImplementedException();
        }

        // Put api/v1/weeklies/5
        public void Put(int id, ApiWeekly weekly)
        {
            throw new NotImplementedException();
        }

        // Delete api/v1/weeklies/5
        public void Delete(int id)
        {
            throw new NotImplementedException();
        }
    }
}
