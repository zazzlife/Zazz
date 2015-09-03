using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    [Table("CategoryStatistics")]
    public class CategoryStat : BaseEntity
    {

        public CategoryStat()
        {
            StatUsers = new HashSet<StatUser>();
        }

        public DateTime LastUpdate { get; set; }

        [ForeignKey("Category")]
        public byte CategoryId { get; set; }

        public virtual Category Category { get; set; }

        public int UsersCount { get; set; }

        public virtual ICollection<StatUser> StatUsers { get; set; }

    }
}