using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Zazz.Core.Exceptions;
using Zazz.Core.Interfaces;
using Zazz.Web.Filters;
using Zazz.Web.Interfaces;
using Zazz.Web.Models.Api;

namespace Zazz.Web.Controllers.Api
{
    [HMACAuthorize]
    public class WeekliesController : BaseApiController
    {
        private readonly IWeeklyService _weeklyService;
        private readonly IObjectMapper _objectMapper;

        public WeekliesController(IWeeklyService weeklyService, IObjectMapper objectMapper)
        {
            _weeklyService = weeklyService;
            _objectMapper = objectMapper;
        }

        // GET api/v1/weeklies/5
        public ApiWeekly Get(int id)
        {
            if (id == 0)
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            try
            {
                var weekly = _weeklyService.GetWeekly(id);
                return _objectMapper.WeeklyToApiWeekly(weekly);
            }
            catch (NotFoundException)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
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
