using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Filters;
using Zazz.Core.Interfaces;
using Zazz.Web.Filters;
using Zazz.Web.Models.Api;

namespace Zazz.Web.Controllers.Api
{
    [HMACAuthorize]
    public class FeedsController : BaseApiController
    {
        private readonly IUoW _uow;
        private readonly IUserService _userService;
        private readonly IPhotoService _photoService;

        public FeedsController(IUoW uow, IUserService userService, IPhotoService photoService)
        {
            _uow = uow;
            _userService = userService;
            _photoService = photoService;
        }

        public IEnumerable<object> GetHomeFeeds()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<object> GetHomeFeeds(int lastFeed)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<object> GetUserFeeds(int id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<object> GetUserFeeds(int id, int lastFeed)
        {
            throw new NotImplementedException();
        }
    }
}
