using System.ComponentModel.DataAnnotations;

namespace Zazz.Web.Models
{
    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "Username")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "{0} must be between {2} and {1} characters.")]
        public string UserName { get; set; }

        [MaxLength(40), Required, DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [MaxLength(40), Required, DataType(DataType.Password)]
        public string Password { get; set; }

        [Required, DataType(DataType.Password), Compare("Password")]
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