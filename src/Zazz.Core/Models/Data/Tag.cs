using System.ComponentModel.DataAnnotations;

namespace Zazz.Core.Models.Data
{
    public class Tag
    {
        public byte Id { get; set; }

        [MaxLength(100)]
        public string Name { get; set; }
    }
}