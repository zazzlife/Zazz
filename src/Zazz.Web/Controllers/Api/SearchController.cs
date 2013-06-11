using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Zazz.Core.Interfaces;
using Zazz.Core.Models;
using Zazz.Web.Filters;

namespace Zazz.Web.Controllers.Api
{
    [HMACAuthorize]
    public class SearchController : BaseApiController
    {
        private readonly IUserService _userService;

        public SearchController(IUserService userService)
        {
            _userService = userService;
        }

        // GET /api/v1/search?query=

        public UserSearchResult Get(string query)
        {
            throw new NotImplementedException();
        }
    }
}
