using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using System.Web;
using System;
using Zazz.Core.Models.Data.Enums;
using Zazz.Core.Models.Data;

namespace Zazz.Web.Models
{
    public class RegisterUserHomeViewModel
    {
        [Required]
        [Display(Name = "Username")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "{0} must be between {2} and {1} characters.")]
        [Remote("IsAvailable", "Account", ErrorMessage = "{0} is not available.")]
        [RegularExpression(@"^([a-zA-Z0-9._]+)$", ErrorMessage = "{0} contains invalid character(s)")]
        public string UserName { get; set; }

        [StringLength(40), Required, DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [StringLength(30, MinimumLength = 3), Required, DataType(DataType.Password), AllowHtml]
        public string Password { get; set; }

        [Display(Name = "Confirm Password")]
        [Required, DataType(DataType.Password), System.ComponentModel.DataAnnotations.Compare("Password"), AllowHtml]
        public string ConfirmPassword { get; set; }

        [Required]
        [Display(Name = "Gender:")]
        public Gender Gender { get; set; }

        [Required]
        [Display(Name = "User Type:")]
        public UserType UserType { get; set; }

        [Display(Name = "Major")]
        public byte? MajorId { get; set; }

        public IEnumerable<Major> Majors { get; set; }

        [Display(Name = "Promoter Type")]
        public PromoterType? PromoterType { get; set; }

        public IEnumerable<PromoterType> PromoterTypes { get; set; }

        [Display(Name = "Date of Birth:")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? Birth { get; set; }

        [HiddenInput]
        public bool IsOAuth { get; set; }
    }
}
