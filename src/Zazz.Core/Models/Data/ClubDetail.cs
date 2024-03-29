﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Zazz.Core.Models.Data.Enums;

namespace Zazz.Core.Models.Data
{
    public class ClubDetail : BaseEntity
    {
        [ForeignKey("Id")]
        public User User { get; set; }

        [MaxLength(500)]
        public string ClubName { get; set; }

        public ClubType ClubType { get; set; }

        [MaxLength(500)]
        public string Address { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public int? CoverPhotoId { get; set; }

        [ForeignKey("SchoolId")]
        public virtual School School { get; set; }

        public int? SchoolId { get; set; }

        [ForeignKey("CityId")]
        public virtual City City { get; set; }

        public int? CityId { get; set; }

        public string ClubTypes { get; set; }

        public string url { get; set; }
    }
}