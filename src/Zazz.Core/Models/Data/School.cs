using System.ComponentModel.DataAnnotations;

namespace Zazz.Core.Models.Data
{
    public class School : BaseEntity
    {
        [MaxLength(1000)]
        public string Name { get; set; }
    }
}