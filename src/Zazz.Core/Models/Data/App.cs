using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    public class App : BaseEntity
    {
        [ForeignKey("User")]
        public int UserId { get; set; }

        [MaxLength(50)]
        public string Name { get; set; }

        [MaxLength(4000)]
        public string IconLink { get; set; }

        [MaxLength(4000)]
        public string SecretKey { get; set; }

        public virtual User User { get; set; }
    }
}