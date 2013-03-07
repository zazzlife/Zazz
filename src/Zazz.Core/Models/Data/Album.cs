using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    public class Album : BaseEntity
    {
        [MaxLength(50)]
        public string Name { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }

        public int UserId { get; set; }

        public virtual ICollection<Photo> Photos { get; set; }
    }
}