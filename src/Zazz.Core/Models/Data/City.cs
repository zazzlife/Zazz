using System.ComponentModel.DataAnnotations;

namespace Zazz.Core.Models.Data
{
    public class City : BaseEntity
    {
        [MaxLength(500)]
        public string Name { get; set; }
    }
}