using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    [Table("Photo_Categories")]
    public class PhotoCategory
    {
        [Key, Column(Order = 0)]
        public byte CategoryId { get; set; }

        [Key, Column(Order = 1)]
        public int PhotoId { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }

        [ForeignKey("PhotoId")]
        public virtual Photo Photo { get; set; } 
    }
}