using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    public class TagStat : BaseEntity
    {
        public int ClubId { get; set; }

        public int Count { get; set; }

        [ForeignKey("ClubId")]
        public virtual User Club { get; set; }
    }
}
