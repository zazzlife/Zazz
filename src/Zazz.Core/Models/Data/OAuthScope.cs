using System.ComponentModel.DataAnnotations;

namespace Zazz.Core.Models.Data
{
    public class OAuthScope
    {
        public byte Id { get; set; }

        [MaxLength(50)]
        public string Name { get; set; }
    }
}