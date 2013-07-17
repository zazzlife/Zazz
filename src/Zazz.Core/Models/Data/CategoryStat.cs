using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    [Table("CategoryStatistics")]
    public class CategoryStat : BaseEntity
    {
        public DateTime LastUpdate { get; set; }

        [ForeignKey("Category")]
        public byte CategoryId { get; set; }

        public virtual Category Category { get; set; }

        public int UsersCount { get; set; }
    }
}