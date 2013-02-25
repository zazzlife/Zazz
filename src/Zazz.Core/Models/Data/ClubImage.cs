﻿using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    public class ClubImage : ImageEntityBase
    {
        [ForeignKey("ClubId")]
        public Club Club { get; set; }

        public int ClubId { get; set; }
    }
}