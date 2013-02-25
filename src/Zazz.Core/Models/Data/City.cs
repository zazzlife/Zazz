using System.ComponentModel.DataAnnotations;

namespace Zazz.Core.Models.Data
{
    public class City
    {
        public int Id { get; set; }

        [MaxLength(30)]
        public string Name { get; set; }
    }
}