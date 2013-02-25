using System;
using System.ComponentModel.DataAnnotations;

namespace Zazz.Core.Models.Data
{
    public class ClubEventEntity : BaseClubPost
    {
        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        [MaxLength(100)]
        public string Location { get; set; }
    }
}