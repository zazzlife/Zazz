using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;

namespace Zazz.Web.Models
{
    public class RegisterUserViewModel
    {
        [Required]
        [Display(Name = "Username")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "{0} must be between {2} and {1} characters.")]
        [Remote("IsAvailable", "Account", ErrorMessage = "{0} is not available.")]
        [RegularExpression(@"^([a-zA-Z0-9._]+)$", ErrorMessage = "{0} contains invalid character(s)")]
        public string UserName { get; set; }

        [StringLength(30, MinimumLength = 3), Required, DataType(DataType.Password), AllowHtml]
        public string Password { get; set; }

        [Display(Name = "Confirm Password")]
        [Required, DataType(DataType.Password), System.ComponentModel.DataAnnotations.Compare("Password"), AllowHtml]
        public string ConfirmPassword { get; set; }

        [Required]
        [Display(Name = "User Type:")]
        public UserType UserType { get; set; }

        [HiddenInput]
        public bool IsOAuth { get; set; }
    }
}