using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Zazz.Core.Interfaces;
using Zazz.Web.Filters;

namespace Zazz.Web.Controllers.Api
{
    [OAuth2Authorize]
    public class AvailableRewardsController : BaseApiController
    {
        private readonly IUoW _uow;

        public AvailableRewardsController(IUoW uow)
        {
            _uow = uow;
        }

        public IEnumerable<ApiUserReward> Get(int id)
        {
            if (id == 0)
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            var rewards = _uow.UserRewardRepository.GetRewards(id, CurrentUserId);
            return rewards.Select(r => new ApiUserReward
                                       {
                                           RedeemedDate = r.RedeemedDate,
                                           RewardId = r.Id,
                                           UserId = id,
                                           RewardName = r.Reward.Name,
                                           RewardDescription = r.Reward.Description
                                       });
        }
    }
}
