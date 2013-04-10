using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    public class FacebookPage : BaseEntity
    {
        [ForeignKey("UserId")]
        public User User { get; set; }

        public int UserId { get; set; }

        [MaxLength(4000)]
        public string Name { get; set; }

        [MaxLength(4000)]
        public string FacebookId { get; set; }
        
        [MaxLength(4000)]
        public string AccessToken { get; set; }
    }
}