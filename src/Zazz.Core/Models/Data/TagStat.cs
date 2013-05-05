using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    public class TagStat : BaseEntity
    {
        public TagStat()
        {
            TagUsers = new HashSet<TagStatUser>();
        }

        [Column(TypeName = "Date")]
        public DateTime Date { get; set; }
        
        public byte TagId { get; set; }

        [ForeignKey("TagId")]
        public virtual Tag Tag { get; set; }

        public int UsersCount { get; set; }

        public ICollection<TagStatUser> TagUsers { get; set; }
    }
}