using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Zazz.Core.Models.Data
{
    public class User
    {
        public int Id { get; set; }

        [MaxLength(20), MinLength(3), Required]
        public string UserName { get; set; }

        [MaxLength(60), Required, DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [MaxLength(40), Required, DataType(DataType.Password)]
        public string Password { get; set; }

        [Required, DataType(DataType.Date), Display(AutoGenerateField = false)]
        public DateTime JoinedDate { get; set; }

        [Required, Display(AutoGenerateField = false)]
        public DateTime LastActivity { get; set; }

        [Display(AutoGenerateField = false)]
        public bool IsConfirmed { get; set; }

        public ValidationToken ValidationToken { get; set; }

        public virtual ICollection<OAuthAccount> LinkedAccounts { get; set; }

        public UserInfo MoreInfo { get; set; }

        public virtual ICollection<UserImage> UploadedImages { get; set; }

        [Display(AutoGenerateField = false)]
        public int CoverPhotoId { get; set; }
    }
}