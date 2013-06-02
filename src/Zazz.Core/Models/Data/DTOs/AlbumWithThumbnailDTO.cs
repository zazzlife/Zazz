using System;

namespace Zazz.Core.Models.Data.DTOs
{
    public class AlbumWithThumbnailDTO
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int UserId { get; set; }

        public bool IsFacebookAlbum { get; set; }

        public string FacebookId { get; set; }

        public int? PageId { get; set; }

        public DateTime CreatedDate { get; set; }

        public int ThumbnailPhotoId { get; set; }
    }
}