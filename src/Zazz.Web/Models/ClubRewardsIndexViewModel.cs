using System.Collections.Generic;
using Zazz.Core.Models.Data.Enums;

namespace Zazz.Web.Models
{
    public class ClubRewardsIndexViewModel
    {
        public AccountType AccountType { get; set; }

        public int TotalPoints { get; set; }

        public List<ClubRewardsIndexClubViewModel> Clubs { get; set; }
    }

    public class ClubRewardsIndexClubViewModel
    {
        public int ClubId { get; set; }

        public string ClubName { get; set; }

        public int Points { get; set; }
    }
}