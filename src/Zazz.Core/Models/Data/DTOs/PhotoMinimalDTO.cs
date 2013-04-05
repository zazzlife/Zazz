﻿namespace Zazz.Core.Models.Data.DTOs
{
    public class PhotoMinimalDTO
    {
        public int Id { get; set; }

        public int UploaderId { get; set; }

        public int? AlbumId { get; set; }

        public bool IsFacebookPhoto { get; set; }

        public string FacebookPicUrl { get; set; }
    }
}