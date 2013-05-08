using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    [Table("TagStatistics")]
    public class TagStat : BaseEntity
    {
        [Column(TypeName = "Date")]
        public DateTime LastUpdate { get; set; }
        
        public byte TagId { get; set; }

        [ForeignKey("TagId")]
        public virtual Tag Tag { get; set; }

        public int UsersCount { get; set; }
    }
}