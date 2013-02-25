﻿using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    public class ClubAdmin : EntityBase
    {
        [ForeignKey("UserId")]
        public User User { get; set; }

        public int UserId { get; set; }

        [ForeignKey("ClubId")]
        public Club Club { get; set; }

        public int ClubId { get; set; }

    }
}