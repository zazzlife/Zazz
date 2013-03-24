using System.ComponentModel.DataAnnotations;

namespace Zazz.Core.Models.Data
{
    public class City : BaseEntity
    {
        [MaxLength(75)]
        public string Name { get; set; }
    }
}