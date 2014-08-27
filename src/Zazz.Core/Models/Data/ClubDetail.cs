using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Zazz.Core.Models.Data.Enums;
using System.Collections.Generic;
using System;

namespace Zazz.Core.Models.Data
{
    public class ClubDetail : BaseEntity
    {
        [ForeignKey("Id")]
        public User User { get; set; }

        [MaxLength(500)]
        public string ClubName { get; set; }

        public int ClubTypesBits { get; set; }

        public virtual IEnumerable<ClubType> ClubTypes
        {
            get
            {
                List<ClubType> cts = new List<ClubType>();
                foreach (ClubType ct in Enum.GetValues(typeof(ClubType)))
                {
                    if (((ClubTypesBits >> (int)ct) & 1) == 1)
                        cts.Add(ct);
                }
                return cts;
            }

            set
            {
                ClubTypesBits = 0;
                if(value != null)
                {
                    foreach (ClubType ct in value)
                    {
                        ClubTypesBits |= 1 << (int)ct;
                    }
                }
            }
        }

        [MaxLength(500)]
        public string Address { get; set; }

        public int? CoverPhotoId { get; set; }

        [ForeignKey("SchoolId")]
        public virtual School School { get; set; }

        public short? SchoolId { get; set; }

        [ForeignKey("CityId")]
        public virtual City City { get; set; }

        public int? CityId { get; set; }

        public bool ShowSync { get; set; }
    }
}