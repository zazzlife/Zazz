using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Web.Controllers.Api
{
    public class ClubTypesController : ApiController
    {
        private readonly IStaticDataRepository _staticDataRepository;

        public ClubTypesController(IStaticDataRepository staticDataRepository)
        {
            _staticDataRepository = staticDataRepository;
        }

        // GET api/v1/clubtypes
        public IEnumerable<ClubType> Get()
        {
            return _staticDataRepository.GetClubTypes();
        }

        // GET api/clubtypes/5
        public ClubType Get(int id)
        {
            return _staticDataRepository.GetClubTypes()
                                        .SingleOrDefault(t => t.Id == id);
        }
    }
}
