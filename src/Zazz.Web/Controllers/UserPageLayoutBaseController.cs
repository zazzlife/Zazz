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
        protected readonly ITagService TagService;

        protected UserPageLayoutBaseController(IUserService userService, IPhotoService photoService,
                                            IDefaultImageHelper defaultImageHelper, ITagService tagService)
            : base(userService, photoService, defaultImageHelper)
        {
            TagService = tagService;
        }

        public IEnumerable<TagStatViewModel> GetTagStats()
        {
            return TagService.GetAllTagStats()
                             .Select(t => new TagStatViewModel
                                          {
                                              TagName = t.Category.Name,
                                              UsersCount = t.UsersCount
                                          });
        }
    }
}
