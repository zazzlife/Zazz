using System.Collections.Generic;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;

namespace Zazz.Web.Models
{
    public class ClubRewardsListViewModel
    {
        public string ClubName { get; set; }

        public int CurrentUserPoints { get; set; }

        public bool IsCurrentUserOwner { get; set; }

        public AccountType CurrentUserAccountType { get; set; }

        public List<ClubReward> Rewards { get; set; } 
    }
}