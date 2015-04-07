using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Services;
using Zazz.Core.Models;
using Zazz.Core.Models.Data.Enums;
using Zazz.Infrastructure;
using Zazz.Infrastructure.Helpers;
using Zazz.Web.Filters;
using Zazz.Web.Helpers;
using Zazz.Web.Models.Api;

namespace Zazz.Web.Controllers.Api
{
    [OAuth2Authorize]
    public class MapLocationController : BaseApiController
    {

        private readonly IUserService _userService;

        public MapLocationController(IUserService userService)
        {
            _userService = userService;
        }

        // GET api/maplocation
        public IEnumerable<ApiMapDetails> Get()
        {
            return _userService.getAllClubs().ToList().Select(f => new ApiMapDetails {
                clubid = f.Id,
                clubname = f.ClubDetail.ClubName,
                address = f.ClubDetail.Address,
                city = f.ClubDetail.City,
                cityid = f.ClubDetail.CityId
            });
        }

        // GET api/maplocation/5
        public string Get(int id)
        {
            return "";
        }

        // POST api/maplocation
        public void Post([FromBody]string value)
        {
        }

        // PUT api/maplocation/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/maplocation/5
        public void Delete(int id)
        {
        }
    }
}
