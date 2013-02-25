using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    public class UserDetail : BaseEntity
    {
        [MaxLength(50)]
        public string SchoolName { get; set; }

        [ForeignKey("MajorId")]
        public Major Major { get; set; }

        public byte MajorId { get; set; }

        [ForeignKey("CityId")]
        public City City { get; set; }

        public int CityId { get; set; }

        [MaxLength(60), Required, DataType(DataType.EmailAddress)]
        public string PublicEmail { get; set; }
    }
}