using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    public class Link : BaseEntity
    {
        [MaxLength(255), Required]
        public string Url { get; set; }

        public LinkType LinkType { get; set; }
    }
}