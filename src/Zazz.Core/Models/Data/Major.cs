using System.ComponentModel.DataAnnotations;

namespace Zazz.Core.Models.Data
{
    public class Major : BaseEntity
    {
        [MaxLength(500)]
        public string Name { get; set; }
    }
}