using System.ComponentModel.DataAnnotations;

namespace Zazz.Web.Models
{
    public class LoginViewModel
    {
        [Required]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "{0} must be between {1} and {2} characters.")]
        public string UserName { get; set; }

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }
    }
}