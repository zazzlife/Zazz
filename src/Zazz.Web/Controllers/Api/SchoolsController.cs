using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Repositories;
using Zazz.Core.Models.Data;
using Zazz.Web.Filters;

namespace Zazz.Web.Controllers.Api
{
    [OAuth2Authorize]
    public class SchoolsController : BaseApiController
    {
        private readonly IStaticDataRepository _staticDataRepository;

        public SchoolsController(IStaticDataRepository staticDataRepository)
        {
            _staticDataRepository = staticDataRepository;
        }

        // GET api/v1/schools
        public IEnumerable<School> Get()
        {
            return _staticDataRepository.GetSchools();
        }

        // GET api/v1/schools/5
        public School Get(int id)
        {
            return _staticDataRepository.GetSchools()
                                        .SingleOrDefault(s => s.Id == id);
        }
    }
}