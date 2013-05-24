using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Web.Controllers.Api
{
    public class MajorsController : ApiController
    {
        private readonly IStaticDataRepository _staticDataRepository;

        public MajorsController(IStaticDataRepository staticDataRepository)
        {
            _staticDataRepository = staticDataRepository;
        }

        // GET api/v1/majors
        public IEnumerable<Major> Get()
        {
            return _staticDataRepository.GetMajors();
        }

        // GET api/v1/majors/5
        public Major Get(int id)
        {
            return _staticDataRepository.GetMajors()
                                        .SingleOrDefault(c => c.Id == id);
        }
    }
}