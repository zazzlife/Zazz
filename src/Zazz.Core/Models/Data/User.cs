﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    public class User : BaseEntity
    {
        [StringLength(20, MinimumLength = 2, ErrorMessage = "{0} must be between {1} and {2} characters.")]
        public string Username { get; set; }

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

        [MaxLength(30), Display(Name = "Full name")]
        public string FullName { get; set; }

        [ForeignKey("SchoolId")]
        public School School { get; set; }

        public short? SchoolId { get; set; }

        [ForeignKey("MajorId")]
        public Major Major { get; set; }

        public byte? MajorId { get; set; }

        [ForeignKey("CityId")]
        public City City { get; set; }

        public int? CityId { get; set; }

        [MaxLength(60), DataType(DataType.EmailAddress)]
        public string PublicEmail { get; set; }

        public virtual ValidationToken ValidationToken { get; set; }

        public virtual ICollection<OAuthAccount> LinkedAccounts { get; set; }

        public virtual ICollection<UserImage> UploadedImages { get; set; }

        [Display(AutoGenerateField = false)]
        public int CoverPhotoId { get; set; }

        public virtual ICollection<UserEventComment> UserEventComments { get; set; }

        public virtual ICollection<ClubPostComment> ClubPostComments { get; set; }

        public virtual ICollection<UserFollow> FollowingUsers { get; set; }

        public virtual ICollection<UserFollowRequest> SentFollowRequests { get; set; }
    }
}