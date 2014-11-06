using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.IO;
using System.Security;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Zazz.Core.Exceptions;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Services;
using Zazz.Core.Models.Data;
using Zazz.Web.Filters;
using Zazz.Web.Interfaces;
using Zazz.Web.Models.Api;
using Zazz.Core.Interfaces.Repositories;
using Zazz.Core.Models;
using Zazz.Core.Models.Data.Enums;

namespace Zazz.Web.Controllers.Api
{
    public class LookupSearchController : BaseApiController
    {
        private readonly IUserService _userService;

        public LookupSearchController(IUserService userService)
        {
            _userService = userService;
        }

        public class AutocompleteResponse
        {
            public int Id { get; set; }

            public string Value { get; set; }

            public string Img { get; set; }
        }


        // GET api/lookupsearch/5
        public string Get(int id)
        {
            return "";
        }

        // POST api/lookupsearch
        public IEnumerable<AutocompleteResponse> Get(string value)
        {
            var users = _userService.Search(value);


            return users.Select(u => new AutocompleteResponse
            {
                Id = u.UserId,
                Value = u.DisplayName,
                Img = u.DisplayPhoto.VerySmallLink
            }).ToList();
        }

        // PUT api/lookupsearch/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/lookupsearch/5
        public void Delete(int id)
        {
        }
    }
}
