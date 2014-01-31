using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;

namespace Zazz.Web.Models
{
    public class RegisterClubViewModel
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

        [Required, StringLength(30), Display(Name = "Club Name")]
        [RegularExpression(@"^([a-zA-Z0-9 ._']+)$", ErrorMessage = "{0} contains invalid character(s)")]
        public string ClubName { get; set; }

        [Display(Name = "Club Type")]
        public ClubType ClubType { get; set; }

        [Display(Name = "Club Address")]
        public string ClubAddress { get; set; }

        [HiddenInput]
        public bool IsOAuth { get; set; }

        [Display(Name = "School")]
        public short? SchoolId { get; set; }

        [Display(Name = "City")]
        public short? CityId { get; set; }

        public IEnumerable<School> Schools { get; set; }

        public IEnumerable<City> Cities { get; set; }
    }
}