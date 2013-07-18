using System;

namespace Zazz.Web.Models.Api
{
    public class ApiUserReward
    {
        public int RewardId { get; set; }

        public int UserId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public DateTime RedeemedDate { get; set; }
    }
}