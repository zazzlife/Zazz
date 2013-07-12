using System;

namespace Zazz.Web.Controllers.Api
{
    public class ApiUserReward
    {
        public int RewardId { get; set; }

        public int UserId { get; set; }

        public string RewardName { get; set; }

        public string RewardDescription { get; set; }

        public DateTime RedeemedDate { get; set; }
    }
}