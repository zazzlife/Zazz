using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Web.Models;

namespace Zazz.Web.Controllers
{
    public abstract class UserPageLayoutBaseController : BaseController
    {
        protected readonly ICategoryService CategoryService;

        protected UserPageLayoutBaseController(IUserService userService, IPhotoService photoService,
                                            IDefaultImageHelper defaultImageHelper, ICategoryService categoryService)
            : base(userService, photoService, defaultImageHelper)
        {
            CategoryService = categoryService;
        }

        public IEnumerable<CategoryStatViewModel> GetTagStats()
        {
            return CategoryService.GetAllStats()
                             .Select(t => new CategoryStatViewModel
                                          {
                                              CategoryName = t.Category.Name,
                                              UsersCount = t.UsersCount
                                          });
        }
    }
}
