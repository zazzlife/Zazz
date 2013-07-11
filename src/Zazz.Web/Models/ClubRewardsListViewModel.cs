using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

        public List<ClubRewardViewModel> Rewards { get; set; } 
    }

    public class ClubRewardViewModel
    {
        public int Id { get; set; }

        public int ClubId { get; set; }

        [Display(Name = "Club")]
        public string ClubName { get; set; }

        [MaxLength(100), Required, Display(Name = "Reward Name")]
        public string Name { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }

        [Range(0, 1000000)]
        public int Cost { get; set; }

        public bool IsEnabled { get; set; }

        public bool AlreadyPurchased { get; set; }

        [Display(Name = "Date")]
        public DateTime RedeemedDate { get; set; }
    }
}