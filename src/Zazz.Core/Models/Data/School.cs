using System.ComponentModel.DataAnnotations;

namespace Zazz.Core.Models.Data
{
    public class School
    {
        public short Id { get; set; }

        [MaxLength(35)]
        public string Name { get; set; }
    }
}