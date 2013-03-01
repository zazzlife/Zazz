using System.ComponentModel.DataAnnotations;

namespace Zazz.Web.Models
{
    public class ResetPasswordModel
    {
        [Display(Name = "New Password")]
        [StringLength(30, MinimumLength = 3), Required, DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Display(Name = "Confirm Password")]
        [Required, DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Confirm password and New Password do not match")]
        public string ConfirmPassword { get; set; } 
    }
}