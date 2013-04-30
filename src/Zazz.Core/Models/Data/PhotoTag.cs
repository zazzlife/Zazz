using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    [Table("Photos_Tags")]
    public class PhotoTag
    {
        [Key, Column(Order = 0)]
        public byte TagId { get; set; }

        [Key, Column(Order = 1)]
        public int PhotoId { get; set; }

        [ForeignKey("TagId")]
        public virtual Tag Tag { get; set; }

        [ForeignKey("PhotoId")]
        public virtual Photo Photo { get; set; } 
    }
}