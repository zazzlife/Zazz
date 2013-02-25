using System.ComponentModel.DataAnnotations;

namespace Zazz.Core.Models.Data
{
    public class City : EntityBase
    {
        [MaxLength(30)]
        public string Name { get; set; }
    }
}