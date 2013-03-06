using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    public class Club : BaseEntity
    {
        [Required, MaxLength(30), MinLength(2)]
        public string Name { get; set; }

        public int FollowersCount { get; set; }

        [ForeignKey("CityId")]
        public City City { get; set; }

        public int? CityId { get; set; }

        [Display(AutoGenerateField = false)]
        public int? FacebookAccountId { get; set; }

        [MaxLength(1000), Display(AutoGenerateField = false)]
        public string FacebookAccessToken { get; set; }

        public virtual ICollection<ClubImage> Images { get; set; }
    }
}