using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    public class UserDetail : BaseEntity
    {
        [ForeignKey("Id")]
        public virtual User User { get; set; }

        [MaxLength(30)]
        public string FullName { get; set; }

        public Gender Gender { get; set; }

        [ForeignKey("SchoolId")]
        public School School { get; set; }

        public short? SchoolId { get; set; }

        [ForeignKey("MajorId")]
        public Major Major { get; set; }

        public byte? MajorId { get; set; }

        [ForeignKey("CityId")]
        public City City { get; set; }

        public int? CityId { get; set; }

        public int CoverPhotoId { get; set; }

        public int ProfilePhotoId { get; set; }

        [MaxLength(40), DataType(DataType.EmailAddress)]
        public string PublicEmail { get; set; }

        public bool SyncFbEvents { get; set; }

        public bool SyncFbPosts { get; set; }

        public bool SyncFbImages { get; set; }

        public bool SendSyncErrorNotifications { get; set; }

        public DateTime? LastSyncError { get; set; }

        public DateTime? LasySyncErrorEmailSent { get; set; }

        [DataType(DataType.Date)]
        public DateTime JoinedDate { get; set; }
    }
}