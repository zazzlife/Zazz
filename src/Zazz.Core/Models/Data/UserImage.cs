using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    public class UserImage : BaseImageEntity
    {
        [ForeignKey("AlbumId")]
        public UserAlbum Album { get; set; }

        public int AlbumId { get; set; }
    }
}