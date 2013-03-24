using System.ComponentModel.DataAnnotations;

namespace Zazz.Core.Models.Data
{
    public class School
    {
        public short Id { get; set; }

        [MaxLength(50)]
        public string Name { get; set; }
    }
}