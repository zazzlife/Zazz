using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Zazz.Core.Models.Data.Enums;

namespace Zazz.Core.Models.Data
{
    public class UserDetail : BaseEntity
    {
        [ForeignKey("Id")]
        public virtual User User { get; set; }

        [MaxLength(100)]
        public string FullName { get; set; }

        public Gender Gender { get; set; }

        public DateTime Birthdate { get; set; }

        public UserType UserType { get; set; }

        public bool IsPromoter { get; set; }

        public PromoterType? PromoterType { get; set; }

        [ForeignKey("SchoolId")]
        public virtual School School { get; set; }

        public short? SchoolId { get; set; }

        [ForeignKey("MajorId")]
        public virtual Major Major { get; set; }

        public byte? MajorId { get; set; }

        [ForeignKey("CityId")]
        public virtual City City { get; set; }

        public int? CityId { get; set; }
    }
}