using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    [Table("Events")]
    public class ZazzEvent : BaseEntity
    {
        [ForeignKey("UserId")]
        public User User { get; set; }

        public int UserId { get; set; }

        [Required, MaxLength(150)]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        public bool IsDateOnly { get; set; }

        public bool IsFacebookEvent { get; set; }

        public DateTime CreatedDate { get; set; }

        public long? FacebookEventId { get; set; }

        [MaxLength(300), DataType(DataType.Url)]
        public string FacebookLink { get; set; } //TODO: remove this property later

        [MaxLength(300), DataType(DataType.ImageUrl)]
        public string FacebookPhotoLink { get; set; }

        public int? PhotoId { get; set; }

        [DataType("datetimeoffset(0)")]
        public DateTimeOffset Time { get; set; }

        public DateTime TimeUtc { get; set; }

        [MaxLength(80)]
        public string Location { get; set; }

        [MaxLength(80)]
        public string Street { get; set; }

        [MaxLength(50)]
        public string City { get; set; }

        public float? Latitude { get; set; }

        public float? Longitude { get; set; }

        [DataType(DataType.Currency)]
        public float? Price { get; set; }

        [ForeignKey("PageId")]
        public FacebookPage Page { get; set; }

        public int? PageId { get; set; }
    }
}