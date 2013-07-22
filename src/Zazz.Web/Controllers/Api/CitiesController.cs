using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Repositories;
using Zazz.Core.Models.Data;

namespace Zazz.Web.Controllers.Api
{
    public class CitiesController : ApiController
    {
        private readonly IStaticDataRepository _staticDataRepository;

        public CitiesController(IStaticDataRepository staticDataRepository)
        {
            _staticDataRepository = staticDataRepository;
        }

        // GET api/v1/cities
        public IEnumerable<City> Get()
        {
            return _staticDataRepository.GetCities();
        }

        // GET api/v1/cities/5
        public City Get(int id)
        {
            return _staticDataRepository.GetCities()
                                        .SingleOrDefault(c => c.Id == id);
        }
    }
}
