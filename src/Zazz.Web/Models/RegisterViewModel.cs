using System.ComponentModel.DataAnnotations;

namespace Zazz.Web.Models
{
    public class RegisterViewModel
    {
        [MaxLength(20), MinLength(2), Required]
        public string UserName { get; set; }

        [MaxLength(60), Required, DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [MaxLength(40), Required, DataType(DataType.Password)]
        public string Password { get; set; }

        [MaxLength(40), Required, DataType(DataType.Password), Compare("Password")]
        public string ConfirmPassword { get; set; }

        [MaxLength(30), Display(Name = "Full name")]
        public string FullName { get; set; }

        [Display(Name = "School")]
        public short? SchoolId { get; set; }

        [Display(Name = "City")]
        public short? CityId { get; set; }

        [Display(Name = "Major")]
        public byte? MajorId { get; set; }
    }
}