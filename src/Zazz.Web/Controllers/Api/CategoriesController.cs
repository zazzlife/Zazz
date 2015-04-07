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
    public class CategoriesController : BaseApiController
    {
        private readonly IStaticDataRepository _staticDataRepository;

        public CategoriesController(IStaticDataRepository staticDataRepository)
        {
            _staticDataRepository = staticDataRepository;
        }

        // GET api/v1/categories
        public IEnumerable<Category> Get()
        {
            return _staticDataRepository.GetCategories();
        }

        // GET api/v1/categories/5
        public Category Get(int id)
        {
            return _staticDataRepository.GetCategories()
                                        .SingleOrDefault(t => t.Id == id);
        }
    }
}