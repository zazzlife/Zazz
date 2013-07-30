using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Zazz.Web.Models
{
    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Username")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "{0} must be between {2} and {1} characters.")]
        [RegularExpression(@"^([a-zA-Z0-9._]+)$", ErrorMessage = "{0} contains invalid character(s)")]
        public string Username { get; set; }

        [Required, DataType(DataType.Password), AllowHtml]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "{0} must be between {2} and {1} characters.")]
        public string Password { get; set; }
    }
}