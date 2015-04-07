using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Repositories;
using Zazz.Core.Interfaces.Services;
using Zazz.Core.Models.Data;
using Zazz.Web.Filters;
using Zazz.Web.Models.Api;

namespace Zazz.Web.Controllers.Api
{
    [OAuth2Authorize]
    public class CategoriesStatController : ApiController
    {
        protected readonly ICategoryService _categoryService;
        protected readonly ICategoryStatRepository _categoryStatRepository;

        public CategoriesStatController(
            ICategoryService categoryService,
            IUoW uow
        ) {
            _categoryService = categoryService;
            _categoryStatRepository = uow.CategoryStatRepository;
        }

        // GET api/v1/categoriesstat
        public IEnumerable<ApiCategoryStat> Get()
        {
            return _categoryService.GetAllStats()
                .Select(t => new ApiCategoryStat
                {
                    Id = t.CategoryId,
                    Name = t.Category.Name,
                    UsersCount = t.UsersCount
                });
        }

        // GET api/v1/categoriesstat/5
        public ApiCategoryStat Get(int id)
        {
            CategoryStat cs = _categoryStatRepository.GetById(id);

            return new ApiCategoryStat
            {
                Id = cs.CategoryId,
                Name = cs.Category.Name,
                UsersCount = cs.UsersCount
            };
        }
    }
}