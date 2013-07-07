using System.ComponentModel.DataAnnotations;

namespace Zazz.Core.Models.Data
{
    public class OAuthClient : BaseEntity
    {
        [MaxLength(50)]
        public string Name { get; set; }

        [MaxLength(1000)]
        public string ClientId { get; set; }

        [MaxLength(2000)]
        public string Secret { get; set; }

        public bool IsAllowedToRequestPasswordGrantType { get; set; }

        public bool IsAllowedToRequestFullScope { get; set; }
    }
}