using System.ComponentModel.DataAnnotations;

namespace Zazz.Core.Models.Data
{
    public class Major
    {
        public byte Id { get; set; }

        [MaxLength(20)]
        public string Name { get; set; }
    }
}