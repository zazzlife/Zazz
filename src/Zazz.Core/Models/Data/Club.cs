using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    public class Club
    {
        public int Id { get; set; }

        [Required, MaxLength(30), MinLength(2)]
        public string Name { get; set; }

        public int FollowersCount { get; set; }

        [ForeignKey("CityId")]
        public City City { get; set; }

        public int CityId { get; set; }

        [DataType(DataType.Date), Display(AutoGenerateField = false)]
        public DateTime CreatedDate { get; set; }

        [Display(AutoGenerateField = false)]
        public int CreatedBy { get; set; }

        [Display(AutoGenerateField = false)]
        public int FacebookAccountId { get; set; }

        [MaxLength(1000), Display(AutoGenerateField = false)]
        public string FacebookAccessToken { get; set; }

        [Display(AutoGenerateField = false)]
        public DateTime FacebookTokenExpirationDate { get; set; }
    }
}