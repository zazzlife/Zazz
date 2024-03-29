﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Services;
using Zazz.Core.Models;
using Zazz.Web.Filters;

namespace Zazz.Web.Controllers.Api
{
    [OAuth2Authorize]
    public class SearchController : BaseApiController
    {
        private readonly IUserService _userService;

        public SearchController(IUserService userService)
        {
            _userService = userService;
        }

        // GET /api/v1/search?query=

        public IEnumerable<UserSearchResult> Get(string query)
        {
            if (String.IsNullOrWhiteSpace(query))
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            return _userService.Search(query);
        }
    }
}
