using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Zazz.Core.Exceptions;
using Zazz.Core.Interfaces;
using Zazz.Web.Filters;

namespace Zazz.Web.Controllers.Api
{
    [OAuth2Authorize]
    public class RewardsController : BaseApiController
    {
        private readonly IUoW _uow;
        private readonly IClubRewardService _rewardService;

        public RewardsController(IUoW uow, IClubRewardService rewardService)
        {
            _uow = uow;
            _rewardService = rewardService;
        }

        public IEnumerable<ApiUserReward> Get(int userId)
        {
            if (userId == 0)
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            var rewards = _uow.UserRewardRepository.GetRewards(userId, CurrentUserId);
            return rewards.Select(r => new ApiUserReward
                                       {
                                           RedeemedDate = r.RedeemedDate,
                                           RewardId = r.Id,
                                           UserId = userId,
                                           RewardName = r.Reward.Name,
                                           RewardDescription = r.Reward.Description
                                       });
        }

        public void Delete(int id)
        {
            if (id == 0)
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            try
            {
                _rewardService.RemoveUserReward(id, CurrentUserId);
            }
            catch (NotFoundException)
            {
            }

        }
    }
}
