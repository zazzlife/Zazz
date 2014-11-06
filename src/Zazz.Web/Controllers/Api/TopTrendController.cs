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
    public class TopTrendController : BaseApiController
    {
        private readonly IUoW _uow;
        private readonly IPhotoService _photoService;

        public TopTrendController(IUoW uow, IPhotoService photoService)
        {
            _uow = uow;
            _photoService = photoService;
        }

        // GET api/toptrend
        public IEnumerable<ApiTopTrend> Get()
        {
            var stats = _uow.TagRepository.GetClubTagStats()
               .Select(t => new ApiTopTrend
               {
                   ClubId = t.ClubId,
                   ClubUsername = t.Club.Username,
                   Count = t.Count
               })
               .ToList();

            for (var i = 0; i < stats.Count; i++)
            {
                stats[i].Photo = _photoService.GetUserDisplayPhoto(stats[i].ClubId);
            }

            return stats;
        }

        // POST api/toptrend
        public void Post([FromBody]string value)
        {
        }

        // PUT api/toptrend/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/toptrend/5
        public void Delete(int id)
        {
        }
    }
}
